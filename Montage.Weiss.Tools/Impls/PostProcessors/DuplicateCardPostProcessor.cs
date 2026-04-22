using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Montage.Weiss.Tools.Impls.PostProcessors;

public class DuplicateCardPostProcessor(Func<CardDatabaseContext> database) : ICardPostProcessor<WeissSchwarzCard>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<DuplicateCardPostProcessor>();
    private readonly Func<CardDatabaseContext> _db = database ?? throw new ArgumentNullException(nameof(database));

    readonly ConcurrentDictionary<string, WeissSchwarzTrait> traitMap = new();

    public int Priority => 2;

    public async Task<bool> IsCompatible(List<WeissSchwarzCard> cards, CancellationToken cancellationToken = default)
    {
        return await ValueTask.FromResult(true);
    }

    public async IAsyncEnumerable<WeissSchwarzCard> Process(IAsyncEnumerable<WeissSchwarzCard> originalCards, IProgress<PostProcessorProgressReport> progress, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var db = _db();
        await foreach (var card in originalCards.WithCancellation(cancellationToken))
        {
            var dupCards = await db.WeissSchwarzCards.AsQueryable()
                .Include(c => c.AdditionalInfo)
                .Include(c => c.Traits)
                .Where(c => c.Serial == card.Serial)
                .OrderByDescending(c => c.VersionTimestamp)
                .ToAsyncEnumerable()
                .ToListAsync(cancellationToken);
            if (dupCards.Count > 0)
            {
                card.AdditionalInfo.AddRange(dupCards[0].AdditionalInfo
                    .Select(i => new WeissSchwarzCardOptionalInfo(card, i.Key) { ValueJSON = i.ValueJSON })
                );
                card.AdditionalInfo = card.AdditionalInfo.DistinctBy(oi => oi.Key).ToList();
                db.RemoveRange(dupCards);
            }

            foreach (var trait in card.Traits ?? Enumerable.Empty<WeissSchwarzTrait>())
            {
                if (traitMap.TryAdd(trait.TraitString, trait))
                {
                    Log.Debug("Added to Trait Map: {@trait}", trait);
                    trait.TraitID = Guid.NewGuid();
                }
            }
            foreach (var trait in card.Traits ?? Enumerable.Empty<WeissSchwarzTrait>())
                trait.TraitID = traitMap[trait.TraitString].TraitID;

            yield return card;
        }
        
        await db.SaveChangesAsync(cancellationToken);
    }
}
