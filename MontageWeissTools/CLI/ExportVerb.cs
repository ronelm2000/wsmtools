﻿using CommandLine;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Services;

namespace Montage.Weiss.Tools.CLI;

[Verb("export", HelpText = "Exports a file from one format to another, typically into files for Tabletop Simulator, for example.")]
public class ExportVerb : IVerbCommand, IExportInfo
{
    [Value(0, HelpText = "Indicates the source file/url.")]
    public string Source { get; set;  }

    [Value(1, HelpText = "Indicates the destination; usually a folder.", Default = "./Export/")]
    public string Destination { get; set; } = "./Export/";

    [Option("parser", HelpText = "Manually sets the deck parser to use. Possible values: encoredecks", Default = "encoredecks")]
    public string Parser { get; set; } = "encoredecks";

    [Option("exporter", HelpText = "Manually sets the deck exporter to use. Possible values: tabletopsim, local", Default = "tabletopsim")]
    public string Exporter { get; set; } = "tabletopsim";

    [Option("out", HelpText = "For some exporters, gives an out command to execute after exporting.", Default = "")]
    public string OutCommand { get; set; } = "";

    [Option("with", HelpText = "For some exporters, enables various flags. See each exporter for details.", Separator = ',', Default = new string[] { })]
    public IEnumerable<string> Flags { get; set; } = new string[] { };

    [Option("noninteractive", HelpText = "When set to true, there will be no prompts. Default options will be used.", Default = false)]
    public bool NonInteractive { get; set; } = false;

    [Option("nowarn", HelpText = "When set to true, all warning prompts will default to yes without user input. This flag when set ignores noninteractive flag during warnings (and is automatically true).", Default = false)]
    public bool NoWarning { get; set; } = false;

    private readonly ILogger Log = Serilog.Log.ForContext<ExportVerb>();

    private static readonly IEnumerable<string> Empty = new string[] { };

    /// <summary>
    /// For the IOC
    /// </summary>
    public ExportVerb()
    { 
    }

    public async Task Run(IContainer ioc, IProgress<CommandProgressReport> progress, CancellationToken cancellationToken = default)
    {
        if (NoWarning) NonInteractive = true;

        var aggregator = new CommandProgressAggregator(progress);

        await ioc.UpdateCardDatabase(aggregator, cancellationToken);

        Log.Information("Running...");

        var parser = await ioc.GetAllInstances<IDeckParser<WeissSchwarzDeck, WeissSchwarzCard>>()
            .ToAsyncEnumerable()
            .WhereAwait(async parser => await parser.IsCompatible(Source))
            .OrderByDescending(parser => parser.Priority)
            .FirstAsync(cancellationToken);
        var deck = await parser.Parse(Source, aggregator, cancellationToken);
        var inspectionOptions = new InspectionOptions()
        {
            IsNonInteractive = this.NonInteractive,
            NoWarning = this.NoWarning
        };
        var exporter = ioc.GetAllInstances<IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>>()
            .Where(exporter => exporter.Alias.Contains(Exporter))
            .First();

        var inspectors = ioc.GetAllInstances<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>>()
            .Where(i => !(i is IFilter<IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>> filter) || filter.IsIncluded(exporter));
        if (exporter is IFilter<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>> filter)
            inspectors = inspectors.Where(filter.IsIncluded);

        deck = await inspectors.OrderByDescending(inspector => inspector.Priority)
            .ToAsyncEnumerable()
            .AggregateAwaitAsync(deck, async (d, inspector) => await inspector.Inspect(d, inspectionOptions));

        if (deck != WeissSchwarzDeck.Empty)
            await exporter.Export(deck, this);
    }

    internal class CommandProgressAggregator : IProgress<SetParserProgressReport>, IProgress<PostProcessorProgressReport>, IProgress<DatabaseUpdateReport>, IProgress<DeckParserProgressReport>
    {
        private CommandProgressReport _totalReport = new CommandProgressReport();
        private IProgress<CommandProgressReport> _progress;
        public int PostProcessorCount { get; internal set; }

        internal CommandProgressAggregator(IProgress<CommandProgressReport> progress)
        {
            _progress = progress;
        }

        private (int PercentageBase, float PercentageRatio) GetAggregatePercentageRatios(UpdateProgressReport report)
        {
            return report switch
            {
                var x when x is DatabaseUpdateReport => (0, 0.05f),
                var x when x is SetParserProgressReport => (0, 0.05f),
                var x when x is DeckParserProgressReport => (10, 0.30f),
                _ => (0, 0f)
            };
        }

        private int GetAggregatePercentage(UpdateProgressReport report)
        {
            var percentages = GetAggregatePercentageRatios(report);
            return percentages.PercentageBase + (int)(report.Percentage * percentages.PercentageRatio);
        }

        public void Report(DatabaseUpdateReport value) => ReportUpdate(value);
        public void Report(SetParserProgressReport value) => ReportUpdate(value);
        public void Report(DeckParserProgressReport value) => ReportUpdate(value);
        public void Report(PostProcessorProgressReport value)
        {
            // TODO: Maybe need to change how total percentage computation works?
            _totalReport = _totalReport with
            {
                Percentage = 25 + (int)(value.Percentage * 100 * 0.25f),
                ReportMessage = value.ReportMessage
            };
            _progress.Report(_totalReport);
        }

        public void ReportUpdate(UpdateProgressReport value)
        {
            _totalReport = _totalReport with
            {
                Percentage = GetAggregatePercentage(value),
                ReportMessage = value.ReportMessage
            };
            _progress.Report(_totalReport);
        }

    }
}
