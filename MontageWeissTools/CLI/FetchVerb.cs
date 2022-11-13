using CommandLine;
using Lamar;
using Montage.Card.API.Entities;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Services;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.CLI;

[Verb("fetch", HelpText = "Fetch_HelpText", ResourceType = typeof(Resources.HelpText))]
public class FetchVerb : IVerbCommand, IFetchInfo
{
    private static ILogger Log = Serilog.Log.ForContext<FetchVerb>();

    [Value(0, HelpText = "Fetch_RIDorSerialHelpText", ResourceType = typeof(Resources.HelpText), Default = new string[] { })]
    public IEnumerable<string> RIDsOrSerials { get; set; } = new string[] { };

    [Option("with", HelpText = "Fetch_WithHelpText", ResourceType = typeof(Resources.HelpText), Separator = ',', Default = new string[] { })]
    public IEnumerable<string> Flags { get; set; }

    public async Task Run(IContainer ioc, IProgress<CommandProgressReport> progress, CancellationToken cancellationToken = default)
    {
        Log.Information("Running...");
        Log.Information("Fetching all viable sets...");
        var searchTerms = RIDsOrSerials
            .SelectMany(s => s.Split(','))
            .Select(GetNeoCodeOrRIDSearchTerm)
            .ToList();
        var encoreDeckSrvc = ioc.GetInstance<EncoreDecksService>();
        var setList = await encoreDeckSrvc.GetSetListEntries(cancellationToken);
        var setListResults = setList.Where(sle => searchTerms.Any(st => sle.HasMatch(st))).ToList();
        Log.Information("Found {res} result/s.", setListResults.Count);
        foreach (var result in setListResults)
            await new ParseVerb()
            {
                URI = encoreDeckSrvc.GetCardListURI(result),
                ParserHints = Flags
            }.Run(ioc, progress, cancellationToken);

        Log.Information("Completed.");
        progress.Report(new CommandProgressReport
        {
            Percentage = 100,
            ReportMessage = new Card.API.Entities.Impls.MultiLanguageString { EN = "Completed." }
        });
    }

    private string GetNeoCodeOrRIDSearchTerm(string ridOrSerial)
    {
        var serialTuple = WeissSchwarzCard.ParseSerial(ridOrSerial);
        return serialTuple.ReleaseID ?? ridOrSerial;
    }
}
