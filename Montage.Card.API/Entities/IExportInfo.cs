﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Card.API.Entities
{
    public interface IExportInfo
    {
        string Source { get; }
        string Destination { get; }
        string Parser { get; }
        string Exporter { get; }
        string OutCommand { get; }
        public IEnumerable<string> Flags { get; }
        public bool NonInteractive { get; }
    }
}
