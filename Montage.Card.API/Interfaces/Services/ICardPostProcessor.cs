using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Services;

namespace Montage.Card.API.Interfaces.Services;

/// <summary>
/// A model for post-processing a card that has been successfully parsed.
/// This includes adding an missing details (like images), or corrections.
/// </summary>
public interface ICardPostProcessor<C> where C : ICard
{
    /// <summary>
    /// Indicates the priority of the post-processor.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Indicates if the list of cards exported are compatible for this post-processor in the first place.
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    public Task<bool> IsCompatible(List<C> cards);

    /// <summary>
    /// Applies post-processing on a card/s.
    /// </summary>
    /// <param name="originalCards">The cards above this filter.</param>
    /// <param name="progress">Allows this task to report progress as needed.</param>
    /// <param name="ct">A cancellation token that allows this task to be cancelled as needed.</param>
    /// <returns>Returns an async stream of the processed cards.</returns>
    public IAsyncEnumerable<C> Process(IAsyncEnumerable<C> originalCards, IProgress<PostProcessorProgressReport> progress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies post-processing on a card/s.
    /// </summary>
    /// <param name="originalCards">The cards that will be obtained.</param>
    /// <returns>Returns an async stream of the processed cards.</returns>
    [Obsolete]
    public IAsyncEnumerable<C> Process(IAsyncEnumerable<C> originalCards)
    {
        return Process(originalCards, NoOpProgress<PostProcessorProgressReport>.Instance, CancellationToken.None);
    }
}
