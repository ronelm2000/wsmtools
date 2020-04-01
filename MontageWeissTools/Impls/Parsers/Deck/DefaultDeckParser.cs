using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Parsers.Deck
{
    public class DefaultDeckParser : IDeckParser
    {
        public string[] Alias => new string[] { };

        public int Priority => int.MinValue;

        public bool IsCompatible(string urlOrFile)
        {
            return true;
        }

        private readonly ILogger Log = Serilog.Log.ForContext<DefaultDeckParser>();

        public Task<WeissSchwarzDeck> Parse(string sourceUrlOrFile)
        {
            Log.Error("Cannot find a compatible parser for this URL or File: {file}", sourceUrlOrFile);
            throw new NotImplementedException();
        }
    }
}
