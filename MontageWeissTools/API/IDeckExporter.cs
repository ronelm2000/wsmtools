﻿using Montage.Card.API.Entities;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.API
{
    public interface IDeckExporter
    {
        public string[] Alias { get; }
        public Task Export(WeissSchwarzDeck deck, IExportInfo info);
    }
}
