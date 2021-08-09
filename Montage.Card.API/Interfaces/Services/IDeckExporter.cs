using Montage.Card.API.Entities;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.API
{
    public interface IDeckExporter<D,C> where D : IDeck<C> where C : ICard
    {
        public string[] Alias { get; }
        public Task Export(D deck, IExportInfo info);
    }
}
