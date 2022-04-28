using System.Text.Json;

namespace Montage.Card.API.Entities.Impls;

public class Setting
{
    public string? Key { get; set; }
    public string? Value { get; set; }

    public T? GetValue<T>()
    {
        return JsonSerializer.Deserialize<T>(Value ?? throw new NullReferenceException());
    }

    public void SetValue<T>(T value)
    {
        Value = JsonSerializer.Serialize(value);
    }
}