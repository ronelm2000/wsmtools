using Montage.Card.API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Montage.Card.API.Interfaces.Services
{
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
        /// Applies post-processing on a card.
        /// </summary>
        /// <param name="original"></param>
        /// <returns>Returns the processed card. Should not return itself to respect atomicity.</returns>
        public IAsyncEnumerable<C> Process(IAsyncEnumerable<C> originalCards);
    }
}
