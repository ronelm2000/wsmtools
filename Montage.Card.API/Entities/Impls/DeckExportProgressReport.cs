namespace Montage.Card.API.Entities
{
    public record DeckExportProgressReport : UpdateProgressReport
    {
        public string Exporter { get; init; } = "";

        public static DeckExportProgressReport Starting(ReadOnlySpan<char> deckName, ReadOnlySpan<char> exporterName)
        {
            return new DeckExportProgressReport()
            {
                Percentage = 0,
                ReportMessage = new Impls.MultiLanguageString
                {
                    EN = $"Exporting [{deckName}] into [{exporterName}]"
                },
                Exporter = exporterName.ToString()
            };
        }

        public DeckExportProgressReport Done(string fullPath) => this with
        {
            Percentage = 100,
            ReportMessage = new Impls.MultiLanguageString
            {
                EN = $"Exported via [{Exporter}]: {fullPath}"
            }
        };
    }
}