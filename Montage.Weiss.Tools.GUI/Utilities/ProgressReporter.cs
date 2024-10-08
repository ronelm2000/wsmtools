using System;
using Serilog;
using Montage.Weiss.Tools.Entities;
using Montage.Card.API.Entities;
using Montage.Card.API.Interfaces.Services;

namespace Montage.Weiss.Tools.GUI.Utilities;

public class ProgressReporter(ILogger log, Action<string> actionPass) : IProgress<CommandProgressReport>, IProgress<DeckExportProgressReport>, IProgress<DeckParserProgressReport>
{
    public void Report(CommandProgressReport report)
    {
        var message = report.ReportMessage.EN ?? report.ToString();
        log.Debug(message);
        actionPass(message);
    }

    public void Report(DeckExportProgressReport report)
    {
        var message = report.ReportMessage.EN ?? report.ToString();
        log.Debug(message);
        actionPass(message);
    }

    public void Report(DeckParserProgressReport report)
    {
        var message = report.ReportMessage.EN ?? report.ToString();
        log.Debug(message);
        actionPass(message);
    }
}
