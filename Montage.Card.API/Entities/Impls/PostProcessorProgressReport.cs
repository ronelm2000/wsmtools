namespace Montage.Card.API.Entities.Impls;

public record PostProcessorProgressReport : UpdateProgressReport
{
    public PostProcessorProgressReport ObtainedPostProcessingData(string releaseID)
    {
        return this with
        {
            Percentage = 10,
            ReportMessage = new MultiLanguageString
            {
                EN = $"Obtained Post-Processing Data for [{releaseID}]"
            }
        };
    }

    public PostProcessorProgressReport WithProcessedSerial(ICard card, string postProcessorName)
     => this with
     {
         Percentage = 50,
         ReportMessage = new MultiLanguageString { EN = $"Post-Processed [{card.Serial}] using {postProcessorName}." }
     };

    public PostProcessorProgressReport WithProcessingSerial(ICard card, string postProcessorName)
     => this with
     {
         Percentage = 50,
         ReportMessage = new MultiLanguageString { EN = $"Post-Processing [{card.Serial}] using {postProcessorName}." }
     };

    public PostProcessorProgressReport Finished(string postProcessorName)
     => this with
     {
         Percentage = 100,
         ReportMessage = new MultiLanguageString { EN = $"Finished [{postProcessorName}] Post-Processing." }
     };

}
