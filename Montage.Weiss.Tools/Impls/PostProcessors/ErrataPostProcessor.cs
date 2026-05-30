using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Utilities;
using System.Runtime.CompilerServices;

namespace Montage.Weiss.Tools.Impls.PostProcessors;

public class ErrataPostProcessor : ICardPostProcessor<WeissSchwarzCard>,
    ISkippable<IParseInfo>, ISkippable<ICardSetParser<WeissSchwarzCard>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ErrataPostProcessor>();

    public string[] Alias => ["errata"];

    public int Priority => 2;

    public async IAsyncEnumerable<WeissSchwarzCard> Process(
        IAsyncEnumerable<WeissSchwarzCard> originalCards,
        IProgress<PostProcessorProgressReport> progress,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var errata = ErrataLoader.Load();
        if (errata?.Serials is not { Count: > 0 } serials)
        {
            await foreach (var card in originalCards.WithCancellation(cancellationToken))
                yield return card;
            yield break;
        }

        await foreach (var card in originalCards.WithCancellation(cancellationToken))
        {
            if (serials.TryGetValue(card.Serial, out var entry))
                ApplyErrata(card, entry);
            yield return card;
        }
    }

    private static void ApplyErrata(WeissSchwarzCard card, SerialErrata errata)
    {
        if (errata.Effect?.Jp is { Length: > 0 } jpEffects)
            card.Effect = jpEffects;

        if (errata.Name?.JP is { Length: > 0 } jpName)
            card.Name.JP = jpName;
        if (errata.Name?.EN is { Length: > 0 } enName)
            card.Name.EN = enName;

        if (errata.Traits is { Count: > 0 } traits)
        {
            card.Traits.RemoveAll(_ => true);
            card.Traits.AddRange(traits);
        }
    }

    public Task<bool> IsCompatible(List<WeissSchwarzCard> cards, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<bool> IsIncluded(IParseInfo info) =>
        Task.FromResult(!info.ParserHints.Contains("skip:errata", StringComparer.CurrentCultureIgnoreCase));

    public Task<bool> IsIncluded(ICardSetParser<WeissSchwarzCard> parser) =>
        Task.FromResult(true);
}
