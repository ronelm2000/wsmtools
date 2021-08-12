using Montage.Card.API.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Services
{
    public class WSFileOutCommandProcessor : FileOutCommandProcessor
    {
        public override ILogger Log => Serilog.Log.ForContext<WSFileOutCommandProcessor>();
    }
}
