using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Impls.Services
{
    public class WeissSchwarzDatabaseUpdater : DatabaseUpdater<CardDatabaseContext, WeissSchwarzCard>, IDatabaseUpdater<CardDatabaseContext, WeissSchwarzCard>
    {
        public override ILogger Log => Serilog.Log.ForContext<WeissSchwarzDatabaseUpdater>();

        public delegate Task UpdateEventHandler(WeissSchwarzDatabaseUpdater sender, UpdateEventArgs args);
        public event UpdateEventHandler OnStarting;
        public event UpdateEventHandler OnEnding;

        public override Task OnLogEnding(UpdateEventArgs args) => OnStarting?.Invoke(this, args) ?? Task.CompletedTask;
        public override Task OnLogStarting(UpdateEventArgs args) => OnEnding?.Invoke(this, args) ?? Task.CompletedTask;

 //       public override Task OnLogEnding(UpdateEventArgs args) => Task.CompletedTask;
//        public override Task OnLogStarting(UpdateEventArgs args) => Task.CompletedTask;

    }
}
