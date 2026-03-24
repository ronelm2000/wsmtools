using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.Impls;

internal class MockDeckParser : IDeckParser<WeissSchwarzDeck, WeissSchwarzCard>
{
    public string[] Alias => ["mock", ""];

    public int Priority => int.MaxValue;
    public async Task<bool> IsCompatible(string urlOrFile)
    {
        return await ValueTask.FromResult(true);
    }
    public async Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile, IProgress<DeckParserProgressReport> progress, CancellationToken cancellationToken = default)
    {
        return await ValueTask.FromResult<WeissSchwarzDeck>(WeissSchwarzDeck.Empty);
    }
}
