using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.API
{
    public interface IDatabaseExporter
    {
        public string[] Alias { get; }
        public Task Export(CardDatabaseContext database, IDatabaseExportInfo info);
    }
}
