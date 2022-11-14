using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.PostProcessors;

public class DuplicateCardPostProcessor : ICardPostProcessor<WeissSchwarzCard>
{
    private readonly Func<CardDatabaseContext> _db;

    public int Priority => 2;

    public DuplicateCardPostProcessor(IContainer ioc)
    {
        _db = () => ioc.GetInstance<CardDatabaseContext>();
    }

    public async Task<bool> IsCompatible(List<WeissSchwarzCard> cards)
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
                .Where(c => c.Serial == card.Serial)
                .OrderByDescending(c => c.VersionTimestamp)
                .ToAsyncEnumerable()
                .ToListAsync(cancellationToken);
            if (dupCards.Count == 0)
            {
                yield return card;
                continue;
            }

            card.AdditionalInfo = dupCards[0].AdditionalInfo
                .Select(i => new WeissSchwarzCardOptionalInfo(card, i.Key) { ValueJSON = i.ValueJSON })
                .ToList();

            db.RemoveRange(dupCards);

            yield return card;
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
