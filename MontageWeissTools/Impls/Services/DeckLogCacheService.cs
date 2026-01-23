using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Entities.External.DeckLog;

namespace Montage.Weiss.Tools.Impls.Services;

internal class DeckLogCacheService : ICachedMapService<(CardLanguage,string), Dictionary<string, DLCardEntry>>
{
    private Dictionary<(CardLanguage, string), Dictionary<string, DLCardEntry>> _cache = new();

    public Dictionary<string, DLCardEntry> this[(CardLanguage, string) key]
    {
        get => _cache.GetValueOrDefault(key) ?? (_cache[key] = new Dictionary<string, DLCardEntry>());
        set => _cache[key] = value ?? new Dictionary<string, DLCardEntry>();
    }

    public IDictionary<(CardLanguage, string), Dictionary<string, DLCardEntry>> GetValues(IEnumerable<(CardLanguage, string)> keys)
    {
        return keys.Select(key => (Key: key, Value: this[key]) )
                   .Where(p => p.Value is not null)
                   .ToDictionary(prop => prop.Key, prop => prop.Value);
    }
}
