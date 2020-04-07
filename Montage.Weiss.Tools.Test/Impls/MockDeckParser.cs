using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test.Impls
{
    class MockDeckParser : IDeckParser
    {
        public string[] Alias => new[] { "mock", "" };

        public int Priority => int.MaxValue;

        public bool IsCompatible(string urlOrFile)
        {
            return true;
        }

        public Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile)
        {
            return null;
        }
    }
}
