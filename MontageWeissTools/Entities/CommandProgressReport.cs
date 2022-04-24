using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;

namespace Montage.Weiss.Tools.Entities;

public record CommandProgressReport : UpdateProgressReport
{
    public CommandProgressReportVerbType VerbType { get; init; } = CommandProgressReportVerbType.Unknown;
    public MessageType MessageType { get; init; } = MessageType.Unknown;

    internal static CommandProgressReport Starting(CommandProgressReportVerbType vtype)
    {
        return new CommandProgressReport
        {
            MessageType = MessageType.Starting,
            VerbType = vtype,
            Percentage = 0,
            ReportMessage = new MultiLanguageString
            {
                EN = "Starting..."
            }
        };
    }

    internal CommandProgressReport AsDone()
    {
        throw new NotImplementedException();
    }
}

public enum CommandProgressReportVerbType
{
    DatabaseExport,
    Parse,
    Caching,
    Unknown
}

public enum MessageType
{
    Starting,
    InProgress,
    IsDone,
    Unknown
}

public static class CommandProgressReportExtensions
{
    public static CommandProgressReport AsCommandProgress(this UpdateProgressReport value)
        => new CommandProgressReport { Percentage = value.Percentage, ReportMessage = value.ReportMessage };
}