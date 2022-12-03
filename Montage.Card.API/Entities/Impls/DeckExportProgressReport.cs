namespace Montage.Card.API.Entities;

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

    public DeckExportProgressReport Done() => this with
    {
        Percentage = 100,
        ReportMessage = new Impls.MultiLanguageString
        {
            EN = $"Exported via [{Exporter}]"
        }
    };


    public DeckExportProgressReport LoadingImages(string serial, int index, int count)
    => this with
    {
        Percentage = 0 + (index * 75 / count),
        ReportMessage = new Impls.MultiLanguageString
        {
            EN = $"Loading Image from Serial [{serial}] [{index}/{count}]"
        }
    };

    public DeckExportProgressReport GeneratingDeckImage()
     => this with
     {
         Percentage = 76,
         ReportMessage = new Impls.MultiLanguageString
         {
             EN = $"Generating Deck Image..."
         }
     };

    public DeckExportProgressReport SizingImages(string sizingVerb, (int Width, int Height) bounds)
     => this with
     {
         Percentage = 78,
         ReportMessage = new Impls.MultiLanguageString
         {
             EN = $"{sizingVerb} Card Image Size to [{bounds.Width} x {bounds.Height}]..."
         }
     };
}