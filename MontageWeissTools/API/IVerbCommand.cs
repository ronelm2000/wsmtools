using Lamar;
using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.API
{
    public interface IVerbCommand
    {
        public Task Run(IContainer ioc, IProgress<CommandProgressReport> progress, CancellationToken cancellationToken = default);
    }
}
