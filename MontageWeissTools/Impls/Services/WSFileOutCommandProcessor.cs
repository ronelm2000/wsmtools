using Montage.Card.API.Services;

namespace Montage.Weiss.Tools.Impls.Services;

public class WSFileOutCommandProcessor : FileOutCommandProcessor
{
    public override ILogger Log => Serilog.Log.ForContext<WSFileOutCommandProcessor>();
}
