using CommandLine;
using Lamar;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using System;
using Montage.Weiss.Tools.Utilities;
using Montage.Card.API.Services;
using Octokit;
using System.Text.Json;

namespace Montage.Weiss.Tools.CLI;

[Verb("merge", HelpText = "Merge_HelpText", ResourceType = typeof(Resources.HelpText))]
public class MergeVerb : IVerbCommand
{
    [Value(0, HelpText = "Merge_SourceHelpText", ResourceType = typeof(Resources.HelpText))]
    public string Source { get; set; }

    [Value(1, HelpText = "Merge_MergeMapJSONPathHelpText", ResourceType = typeof(Resources.HelpText))]
    public string MergeMapJSONPath { get; set; }

    [Option("with", HelpText = "Merge_FlagsHelpText", Separator = ',', Default = new string[] { })]
    public IEnumerable<string> Flags { get; set; } = new string[] { };

    public IProgress<DeckMergeProgressReport> Progress { get; set; } = NoOpProgress<DeckMergeProgressReport>.Instance;

    private readonly ILogger Log = Serilog.Log.ForContext<ExportVerb>();

    public async Task Run(IContainer ioc, IProgress<CommandProgressReport> progress, CancellationToken cancellationToken = default)
    {
        var aggregator = new CommandProgressAggregator(progress);
        Progress = aggregator.GetProgress<DeckMergeProgressReport>();
        var deckParserProgress = aggregator.GetProgress<DeckParserProgressReport>();
       
        var parser = await ioc.GetAllInstances<IDeckParser<WeissSchwarzDeck, WeissSchwarzCard>>()
            .ToAsyncEnumerable()
            .WhereAwait(async parser => await parser.IsCompatible(Source))
            .OrderByDescending(parser => parser.Priority)
            .FirstAsync(cancellationToken);
        var deck = await parser.Parse(Source, deckParserProgress, cancellationToken);

        var report = DeckMergeProgressReport.Starting();
        Progress.Report(report);

        var stdout = System.Console.OpenStandardOutput();
        var simplifiedDeck = new
        {
            Name = deck.Name,
            Remarks = deck.Remarks,
            Ratios = deck.AsSimpleDictionary()
        };

        await JsonSerializer.SerializeAsync(stdout, simplifiedDeck, cancellationToken: cancellationToken);
    }

    internal class CommandProgressAggregator
    {
        private CommandProgressReport _totalReport = new CommandProgressReport();
        private IProgress<CommandProgressReport> _progress;
        public int PostProcessorCount { get; internal set; }

        internal CommandProgressAggregator(IProgress<CommandProgressReport> progress)
        {
            _progress = progress;
        }

        private Dictionary<Type, (int PercentageBase, float PercentageRatio)> aggregatePercentages = new()
        {
            [typeof(DeckParserProgressReport)] = (00, 0.50f),
            [typeof(DeckMergeProgressReport)] = (50, 0.50f)
        };

        public IProgress<T> GetProgress<T>() where T : UpdateProgressReport
            => _progress.From().Translate<T>(r =>
            {
                var ratio = aggregatePercentages[typeof(T)];
                return r.AsRatio<T, CommandProgressReport>(ratio.PercentageBase, ratio.PercentageRatio);
            });
    }
}
