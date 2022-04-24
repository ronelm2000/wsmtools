using Montage.Card.API.Entities.Impls;

namespace Montage.Card.API.Entities;

public record UpdateProgressReport
{
    public MultiLanguageString ReportMessage { get; init; } = MultiLanguageString.Empty;
    public int Percentage { get; init; } = 0;

}
