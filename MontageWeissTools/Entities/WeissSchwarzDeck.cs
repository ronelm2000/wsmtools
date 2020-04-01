using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Montage.Weiss.Tools.Entities
{
    public class WeissSchwarzDeck
    {
        public string Name { get; set; }
        public Dictionary<WeissSchwarzCard, int> Ratios { get; set; } = new Dictionary<WeissSchwarzCard, int>();
        public string Remarks { get; set; }

        internal Dictionary<string,int> AsSimpleDictionary()
        {
            return Ratios.Select(kyd => (kyd.Key.Serial, kyd.Value))
                         .ToDictionary(kyd => kyd.Serial, kyd => kyd.Value);
        }
    }
}
