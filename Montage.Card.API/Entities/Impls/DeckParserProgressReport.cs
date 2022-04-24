using Montage.Card.API.Entities;

namespace Montage.Card.API.Interfaces.Services;

public record DeckParserProgressReport : UpdateProgressReport
{
    public static DeckParserProgressReport AsStarting(string deckParserName)
     => new DeckParserProgressReport()
     {
         Percentage = 0,
         ReportMessage = new Entities.Impls.MultiLanguageString { EN = $"Getting deck via [{deckParserName}]" }
     };
}
