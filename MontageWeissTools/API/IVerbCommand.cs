using Lamar;
using Montage.Weiss.Tools.Entities;

namespace Montage.Weiss.Tools.API;

public interface IVerbCommand
{
    public Task Run(IContainer ioc, IProgress<CommandProgressReport> progress, CancellationToken cancellationToken = default);
}
