using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;

namespace Montage.Weiss.Tools.Impls.Services;

/// <summary>
/// Allows related services to be preloaded and used as a singleton, because sometimes Lamar gets confused
/// when trying to resolve a service that is under a List.
/// 
/// This also allows related services to be referenced among that list, for example, if you need to change its properties.
/// This is only really relevant for GUI usage.
/// </summary>
public class LocatorService : ILocatorService 
{
    private readonly List<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>> exportedDeckInspectors;

    public LocatorService(IEnumerable<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>> inspectors)
    {
        exportedDeckInspectors = inspectors.ToList();
    }

    public T FindInspector<T>() where T : class,IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>
    {
        return exportedDeckInspectors
            .Where(inspector => inspector is T)
            .Select(inspector => inspector as T)
            .First()!;
    }

    public IAsyncEnumerable<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>> GetExportedDeckInspectorsAsync()
    {
        return exportedDeckInspectors.ToAsyncEnumerable();
    }
}

public interface ILocatorService
{
    public IAsyncEnumerable<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>> GetExportedDeckInspectorsAsync();
    public T FindInspector<T>() where T : class,IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>;
}