using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Exporters
{
    public class TTSDeckExporter : IDeckExporter
    {
        public string[] Alias => new [] { "tts", "tabletopsim" };

        public Task Export(WeissSchwarzDeck deck, string destinationFolderOrURL)
        {
            throw new NotImplementedException();
        }
    }
}
