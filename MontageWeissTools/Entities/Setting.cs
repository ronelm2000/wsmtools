using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities
{
    public class Setting
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public T GetValue<T>()
        {
            return JsonSerializer.Deserialize<T>(Value);
        }

        public void SetValue<T>(T value)
        {
            Value = JsonSerializer.Serialize(value);
        }
    }
}
