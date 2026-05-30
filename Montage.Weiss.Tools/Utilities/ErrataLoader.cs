using Montage.Weiss.Tools.Entities;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Montage.Weiss.Tools.Utilities;

internal static class ErrataLoader
{
    private const string ResourceName = "Montage.Weiss.Tools.Resources.errata.json";
    private static ErrataSet? _cached;
    private static readonly object _lock = new();

    public static ErrataSet? Load()
    {
        if (_cached != null) return _cached;
        lock (_lock)
        {
            if (_cached != null) return _cached;
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(ResourceName);
            if (stream == null) return null;
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            _cached = JsonSerializer.Deserialize<ErrataSet>(json);
            return _cached;
        }
    }
}
