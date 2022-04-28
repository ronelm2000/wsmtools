using Montage.Card.API.Entities;

namespace Montage.Card.API.Interfaces.Services;

public interface IDeckExporter<D,C> where D : IDeck<C> where C : ICard
{
    public string[] Alias { get; }
    public Task Export(D deck, IExportInfo info);
}
