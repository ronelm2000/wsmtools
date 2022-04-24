using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck;

public class DefaultDeckParser : IDeckParser<WeissSchwarzDeck, WeissSchwarzCard>
{
    public string[] Alias => new string[] { };

    public int Priority => int.MinValue;

    public Task<bool> IsCompatible(string urlOrFile)
    {
        return Task.FromResult(true);
    }

    private readonly ILogger Log = Serilog.Log.ForContext<DefaultDeckParser>();

    public Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile, IProgress<DeckParserProgressReport> progress, CancellationToken cancellationToken = default)
    {
        Log.Error("Cannot find a compatible parser for this URL or File: {file}", sourceUrlOrFile);
        throw new NotImplementedException();
    }
}
