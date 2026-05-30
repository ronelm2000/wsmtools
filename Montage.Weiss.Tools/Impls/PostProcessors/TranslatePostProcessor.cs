using Fluent.IO;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Exceptions;
using Montage.Weiss.Tools.Impls.Services;
using Montage.Weiss.Tools.Utilities;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Montage.Weiss.Tools.Impls.PostProcessors;

public class TranslatePostProcessor : ICardPostProcessor<WeissSchwarzCard>, ISkippable<IParseInfo>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TranslatePostProcessor>();
    private readonly WeissSchwarzCardTranslatorService _translator;

    private static readonly JsonSerializerOptions ReportJsonOptions = new()
    {
        WriteIndented = true
    };

    public string[] Alias => ["translate"];

    public int Priority => 1;

    public TranslatePostProcessor(WeissSchwarzCardTranslatorService translator)
    {
        _translator = translator;
    }

    public Task<bool> IsCompatible(List<WeissSchwarzCard> cards, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(cards.Any(c => c.Language == CardLanguage.Japanese));
    }

    public async Task<bool> IsIncluded(IParseInfo info)
    {
        await Task.CompletedTask;
        if (info.ParserHints.Select(s => s.ToLower()).Contains("skip:translate"))
        {
            Log.Information("Skipping due to the parser hint [skip:translate].");
            return false;
        }
        if (info.ParserHints.Contains("skip:external", StringComparer.CurrentCultureIgnoreCase))
        {
            Log.Information("Skipping due to parser hint [skip:external].");
            return false;
        }
        return info.ParserHints.Contains("translation", StringComparer.CurrentCultureIgnoreCase) || info.ParserHints.Contains("translations", StringComparer.CurrentCultureIgnoreCase);
    }

    public async IAsyncEnumerable<WeissSchwarzCard> Process(
        IAsyncEnumerable<WeissSchwarzCard> originalCards,
        IProgress<PostProcessorProgressReport> progress,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var failuresByCard = new Dictionary<string, List<FailedAbilityEntry>>();

        await foreach (var card in originalCards.WithCancellation(cancellationToken))
        {
            if (card.Language != CardLanguage.Japanese)
            {
                yield return card;
                continue;
            }

            var translatedEffects = new List<string>();
            var cardEffects = new List<CardEffect>();
            var cardFailures = new List<FailedAbilityEntry>();
            var seenFailedTexts = new HashSet<string>();

            foreach (var effectText in card.Effect)
            {
                if (string.IsNullOrWhiteSpace(effectText))
                {
                    Log.Debug("Skipping empty/whitespace effect for {serial}", card.Serial);
                    continue;
                }

                var trimmed = effectText.Trim();
                if (trimmed.Length == 1 && IsNoEffectCharacter(trimmed[0]))
                {
                    Log.Debug("Skipping no-effect indicator for {serial}: [{trimmed}]", card.Serial, trimmed);
                    continue;
                }

                try
                {
                    var effect = _translator.TranslateEffect(effectText);
                    translatedEffects.Add(effect.EffectText);
                    cardEffects.Add(effect);
                }
                catch (TranslationNotImplementedException ex)
                {
                    Log.Warning("Translation failed for {serial} effect {text}", card.Serial, effectText);
                    if (seenFailedTexts.Add(effectText))
                    {
                        cardFailures.Add(new FailedAbilityEntry
                        {
                            JapaneseText = effectText,
                            Tree = ex.Effect
                        });
                    }
                    translatedEffects.Add(effectText);
                }
            }

            card.Effect = translatedEffects.ToArray();
            card.AddOptionalInfo("translation.tree", new CardEffectTree { Effects = cardEffects });

            var errata = ErrataLoader.Load();
            if (errata?.Serials?.TryGetValue(card.Serial, out var entry) == true
                && entry.Effect?.En is { Length: > 0 } enEffects)
            {
                Log.Debug("Applying EN errata override for {serial}", card.Serial);
                card.Effect = enEffects;
            }

            if (cardFailures.Count > 0)
                failuresByCard[card.Serial] = cardFailures;

            yield return card;
        }

        if (failuresByCard.Count > 0)
        {
            var report = new FailedTranslationReport
            {
                FailedTranslations = failuresByCard
                    .SelectMany(kvp => kvp.Value.Select(f => (Serial: kvp.Key, f.JapaneseText, f.Tree)))
                    .GroupBy(t => t.JapaneseText, StringComparer.Ordinal)
                    .Select(g => new FailedTranslationEntry
                    {
                        JapaneseText = g.Key,
                        Tree = g.First().Tree,
                        Serials = g.Select(t => t.Serial).Distinct().ToList()
                    })
                    .ToList()
            };

            var json = JsonSerializer.Serialize(report, ReportJsonOptions);
            var exportDir = Path.Get("./Export/");
            System.IO.Directory.CreateDirectory(exportDir.FullPath);
            await Path.Get(exportDir.FullPath, "failed_translation_report.json")
                       .WriteStringAsync(json, cancellationToken);

            var innerExceptions = report.FailedTranslations
                .SelectMany(fte => fte.Serials.Select(s =>
                    new TranslationNotImplementedException(
                        $"Failed to translate effect for {s}: {fte.JapaneseText}",
                        fte.Tree)))
                .ToList();

            throw new TranslationFailedException(
                "Translation failures occurred; see failed_translation_report.json for details.",
                new AggregateException(innerExceptions));
        }
    }

    private static bool IsNoEffectCharacter(char c) => c is '-' or '－' or 'ー' or '―' or '—' or '–';
}

public record FailedTranslationReport
{
    public required List<FailedTranslationEntry> FailedTranslations { get; init; }
}

public record FailedTranslationEntry
{
    public required string JapaneseText { get; init; }
    public required CardEffect Tree { get; init; }
    public required List<string> Serials { get; init; }
}

public record FailedAbilityEntry
{
    public required string JapaneseText { get; init; }
    public required CardEffect Tree { get; init; }
}
