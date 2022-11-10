using System.Text.Json;

namespace Montage.Weiss.Tools.Entities
{
    public class WeissSchwarzCardOptionalInfo
    {
        public string Serial { get; set; }
        public string Key { get; set; }
        public string ValueJSON { get; set; }

        public virtual WeissSchwarzCard Card { get; set; }

        public WeissSchwarzCardOptionalInfo() { }

        public WeissSchwarzCardOptionalInfo(WeissSchwarzCard card, string key)
        {
            this.Card = card;
            this.Key = key;
        }

        public T DeserializeValue<T>()
        {
            return JsonSerializer.Deserialize<T>(ValueJSON);
        }

        public void SerializeValue<T>(T rawValue)
        {
            ValueJSON = JsonSerializer.Serialize<T>(rawValue);
        }
    }
}