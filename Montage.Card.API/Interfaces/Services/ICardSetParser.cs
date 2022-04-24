using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Services;
using System.Runtime.CompilerServices;

namespace Montage.Card.API.Interfaces.Services;

public interface ICardSetParser<C> where C : ICard
{
    /// <summary>
    /// Indicates if this parser is compatible given the parse info provided.
    /// </summary>
    /// <param name="parseInfo"></param>
    /// <returns></returns>
    Task<bool> IsCompatible(IParseInfo parseInfo);

    /// <summary>
    /// Given the URL/Local File, parses it and obtains a list of cards that must first be processed.
    /// </summary>
    /// <param name="urlOrLocalFile"></param>
    /// <param name="progress">Allows this task to report progress as needed.</param>
    /// <param name="ct">A cancellation token that allows this task to be cancelled as needed.</param>
    /// <returns>Returns an async stream of the non-processed cards.</returns>
    IAsyncEnumerable<C> Parse(string urlOrLocalFile, IProgress<SetParserProgressReport> progress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Given the URL/Local File, parses it and obtains a list of cards that must first be processed.
    /// </summary>
    /// <param name="urlOrLocalFile"></param>
    /// <returns></returns>
    [Obsolete]
    IAsyncEnumerable<C> Parse(string urlOrLocalFile)
        => Parse(urlOrLocalFile, NoOpProgress<SetParserProgressReport>.Instance);
}
