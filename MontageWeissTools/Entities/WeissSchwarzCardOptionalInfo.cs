using Montage.Card.API.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Montage.Weiss.Tools.Entities;

public class WeissSchwarzCardOptionalInfo : IExactCloneable<WeissSchwarzCardOptionalInfo>
{
    public string Serial { get; set; }
    public string Key { get; set; }
    public string? ValueJSON { get; set; }

    [JsonIgnore]
    public virtual WeissSchwarzCard? Card { get; set; }

    public WeissSchwarzCardOptionalInfo()
    {
        Serial = string.Empty;
        Key = string.Empty;
    }

    public WeissSchwarzCardOptionalInfo(WeissSchwarzCard card, string key)
    {
        this.Card = card;
        this.Serial = card.Serial;
        this.Key = key;
    }

    public T? DeserializeValue<T>()
    {
        try
        {
            return JsonSerializer.Deserialize<T>(ValueJSON ?? string.Empty);
        } catch (JsonException)
        {
            return default;
        }
    }

    public void SerializeValue<T>(T rawValue)
    {
        ValueJSON = JsonSerializer.Serialize<T>(rawValue);
    }

    public WeissSchwarzCardOptionalInfo Clone()
    {
        WeissSchwarzCardOptionalInfo info = new WeissSchwarzCardOptionalInfo();
        info.Card = Card;
        info.Serial = Serial;
        info.Key = Key;
        info.ValueJSON = ValueJSON;
        return info;
    }
}
