using CommandLine;
using Lamar;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using System.Text.Json;

namespace Montage.Weiss.Tools.CLI;

[Verb("merge", HelpText = "Merges two decks into one. Source#2 (overlay) cards override source#1 (base).")]
public class MergeVerb : IVerbCommand
{
    [Value(0, HelpText = "Source deck (base).")]
    public string Source { get; set; } = string.Empty;

    [Value(1, HelpText = "Source2 deck (overlay).")]
    public string Source2 { get; set; } = string.Empty;

    [Option("with", HelpText = "Flags for parsers.", Separator = ',', Default = new string[] { })]
    public IEnumerable<string> Flags { get; set; } = [];

    private static readonly ILogger Log = Serilog.Log.ForContext<MergeVerb>();

    public async Task Run(IContainer ioc, IProgress<CommandProgressReport> progress, CancellationToken cancellationToken = default)
    {
        Log.Information("Merging decks...");

        var exportVerb1 = new ExportVerb { Source = this.Source };
        var exportVerb2 = new ExportVerb { Source = this.Source2 };

        var deckParserProgress = new Progress<DeckParserProgressReport>();

        var s1 = await exportVerb1.Parse(ioc, deckParserProgress, cancellationToken);
        var s2 = await exportVerb2.Parse(ioc, deckParserProgress, cancellationToken);

        if (s1 == WeissSchwarzDeck.Empty)
        {
            Log.Error("Failed to parse source deck (base).");
            return;
        }

        if (s2 == WeissSchwarzDeck.Empty)
        {
            Log.Error("Failed to parse source2 deck (overlay).");
            return;
        }

        var merged = Merge(s1, s2);

        var simpleDeck = new SimpleDeckForJson
        {
            Name = merged.Name,
            Remarks = merged.Remarks,
            Ratios = merged.AsSimpleDictionary()
        };

        var options = new JsonSerializerOptions { WriteIndented = false };
        var json = JsonSerializer.Serialize(simpleDeck, options);

        Program.Console.WriteLine(json);
    }

    private class MergeEntry
    {
        public WeissSchwarzCard Card { get; set; } = null!;
        public int Ratio { get; set; }
        public int Source { get; set; }
    }

    public WeissSchwarzDeck Merge(WeissSchwarzDeck s1, WeissSchwarzDeck s2)
    {
        var merged = new WeissSchwarzDeck
        {
            Name = s1.Name,
            Remarks = s1.Remarks
        };

        var allEntries = s1.Ratios
            .Select(kvp => new MergeEntry { Card = kvp.Key, Ratio = kvp.Value, Source = 1 })
            .Concat(s2.Ratios
                .Select(kvp => new MergeEntry { Card = kvp.Key, Ratio = kvp.Value, Source = 2 }))
            .GroupBy(entry => WeissSchwarzCard.RemoveFoil(entry.Card.Serial))
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(entry => entry.Card.Serial.Length).ToList()
            );

        foreach (var (_, entries) in allEntries)
        {
            var s1Entries = entries.Where(e => e.Source == 1).ToList();
            var s2Entries = entries.Where(e => e.Source == 2).ToList();

            var s1Total = s1Entries.Sum(e => e.Ratio);

            if (!s1Entries.Any())
            {
                continue;
            }

            var s2Total = s2Entries.Sum(e => e.Ratio);
            var gap = s1Total - s2Total;

            var gapFilledEntries = new List<MergeEntry>();
            if (gap > 0)
            {
                var remainingGap = gap;
                foreach (var entry in s1Entries)
                {
                    if (remainingGap <= 0) break;
                    var fillAmount = Math.Min(remainingGap, entry.Ratio);
                    if (fillAmount > 0)
                        gapFilledEntries.Add(new MergeEntry { Card = entry.Card, Ratio = fillAmount, Source = entry.Source });
                    remainingGap -= fillAmount;
                }
            }

            var overflow = s2Total - s1Total;
            var cappedS2Entries = new List<MergeEntry>();
            if (overflow > 0)
            {
                var remainingOverflow = overflow;
                var s2EntriesOrdered = s2Entries.OrderBy(e => e.Card.Serial.Length).ToList();
                foreach (var entry in s2EntriesOrdered)
                {
                    if (remainingOverflow <= 0)
                    {
                        cappedS2Entries.Add(entry);
                    }
                    else
                    {
                        var reduction = Math.Min(remainingOverflow, entry.Ratio);
                        var newRatio = entry.Ratio - reduction;
                        if (newRatio > 0)
                            cappedS2Entries.Add(new MergeEntry { Card = entry.Card, Ratio = newRatio, Source = entry.Source });
                        remainingOverflow -= reduction;
                    }
                }
            }
            else
            {
                cappedS2Entries = s2Entries;
            }

            foreach (var entry in cappedS2Entries)
            {
                merged.Ratios[entry.Card] = merged.Ratios.GetValueOrDefault(entry.Card, 0) + entry.Ratio;
            }

            foreach (var entry in gapFilledEntries)
            {
                merged.Ratios[entry.Card] = merged.Ratios.GetValueOrDefault(entry.Card, 0) + entry.Ratio;
            }
        }

        return merged;
    }

    private class SimpleDeckForJson
    {
        public string Name { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public Dictionary<string, int> Ratios { get; set; } = new();
    }
}
