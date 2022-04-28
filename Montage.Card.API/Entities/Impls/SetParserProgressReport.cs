namespace Montage.Card.API.Entities.Impls;

public record SetParserProgressReport : UpdateProgressReport
{
    public int CardsParsed { get; init; } = 0;

    public SetParserProgressReport WithParsedSerial(ICard card, int totalCardsToParse)
        => this with
        {
            ReportMessage = new MultiLanguageString { EN = $"Parsed [{card.Serial}]." },
            Percentage = 10 + (int)((CardsParsed + 1f) * 90 / totalCardsToParse),
            CardsParsed = CardsParsed + 1
        };
}
