using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.Entities.JSON
{
    public class SimpleDeck
    {
        public string Name { get; set; }
        public string Remarks { get; set; }
        public Dictionary<string, int> Ratios { get; set; }
    }
}
