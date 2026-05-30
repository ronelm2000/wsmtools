using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Entities;

internal record ErrataSet
{
    public Dictionary<string, SerialErrata>? Serials { get; init; }
}

internal record SerialErrata
{
    public EffectErrata? Effect { get; init; }
    public NameErrata? Name { get; init; }
    public List<WeissSchwarzTrait>? Traits { get; init; }
}

internal record EffectErrata
{
    public string[]? Jp { get; init; }
    public string[]? En { get; init; }
}

internal record NameErrata
{
    [JsonPropertyName("jp")]
    public string? JP { get; init; }
    [JsonPropertyName("en")]
    public string? EN { get; init; }
}
