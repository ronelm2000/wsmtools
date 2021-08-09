using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Card.API.Entities
{
    public interface IDatabaseExportInfo : IExportInfo
    {
        /// <summary>
        /// If this is not empty, signals the exporter that the export range is just limited to the following
        /// Release IDs.
        /// </summary>
        public IEnumerable<string> ReleaseIDs { get; }
    }
}
