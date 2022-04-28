using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Impls.PostProcessors;
using static Montage.Weiss.Tools.Impls.PostProcessors.DeckLogPostProcessor;

namespace Montage.Weiss.Tools.Impls.Services;

internal class DeckLogCacheService : ICachedMapService<string, Dictionary<string, DLCardEntry>>
{
    private Dictionary<string, Dictionary<string, DLCardEntry>> _cache = new();

    public Dictionary<string, DLCardEntry> this[string key]
    {
        get => _cache.GetValueOrDefault(key) ?? (_cache[key] = new Dictionary<string, DLCardEntry>());
        set => _cache[key] = value;
    }

    public IDictionary<string, Dictionary<string, DLCardEntry>> GetValues(IEnumerable<string> keys)
    {
        return keys.Select(key => (Key: key, Value: this[key]) )
                   .Where(p => p.Value is not null)
                   .ToDictionary(prop => prop.Key, prop => prop.Value);
    }
}
