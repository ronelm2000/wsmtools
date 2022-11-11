using Montage.Card.API.Entities;

namespace Montage.Card.API.Interfaces.Services;

public interface IDatabaseExporter<ICDB, IC> where ICDB : ICardDatabase<IC> where IC : ICard 
{
    public string[] Alias { get; }
    public Task Export(ICDB database, IDatabaseExportInfo info, CancellationToken cancellationToken);
}
