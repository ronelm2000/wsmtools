using CommandLine;
using Lamar;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.CLI
{
    [Verb("export", HelpText = "Exports a file from one format to another, typically into files for Tabletop Simulator, for example.")]
    public class ExportVerb : IVerbCommand
    {
        [Value(0, HelpText = "Indicates the source file/url.")]
        public string Source { get; }
        
        [Value(1, HelpText = "Indicates the destination; usually a folder.", Default = "./Export/")]
        public string Destination { get; }

        [Option("parser", HelpText = "Manually sets the deck parser to use. Possible values: encoredecks", Default = "encoredecks")]
        public string Parser { get; }

        [Option("exporter", HelpText = "Manually sets the deck exporter to use. Possible values: tabletopsim", Default = "tabletopsim")]
        public string Exporter { get; }

        [Option("out", HelpText = "For some exporters, gives an out command to execute after exporting.", Default = "")]
        public string OutCommand { get; }

        private readonly ILogger Log = Serilog.Log.ForContext<ExportVerb>();

        /// <summary>
        /// For the IOC
        /// </summary>
        public ExportVerb()
        { 
        }

        public ExportVerb(string source, string destination, string parser, string exporter, string outCommand)
        {
            Source = source;
            Destination = destination;
            Parser = parser;
            Exporter = exporter;
            OutCommand = outCommand;
        }

        public async Task Run(IContainer ioc)
        {
            Log.Information("Running...");

            var parser = ioc.GetAllInstances<IDeckParser>()
                .Where(parser => parser.IsCompatible(Source))
                .OrderByDescending(parser => parser.Priority)
                //                .Where(parser => parser.Alias.Contains(Parser) && parser.IsCompatible(Source))
                .First();

            var deck = await parser.Parse(Source);

            deck = await ioc.GetAllInstances<IExportedDeckInspector>()
                .ToAsyncEnumerable()
                .AggregateAwaitAsync(deck, async (d, inspector) => await inspector.Inspect(d, false));

            if (deck != WeissSchwarzDeck.Empty)
            {
                var exporter = ioc.GetAllInstances<IDeckExporter>()
                    .Where(exporter => exporter.Alias.Contains(Exporter))
                    .First();

                await exporter.Export(deck, this);
            }
        }
    }
}
