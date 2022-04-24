namespace Montage.Card.API.Entities.Impls;

public record PostProcessorProgressReport : UpdateProgressReport
{
    public PostProcessorProgressReport WithProcessedSerial(ICard card, string postProcessorName)
     => this with
     {
         Percentage = 50,
         ReportMessage = new MultiLanguageString { EN = $"Post-Processed [{card}] using {postProcessorName}." }
     };
}
