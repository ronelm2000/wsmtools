using Lamar;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Entities;

namespace Montage.Weiss.Tools.Impls.Services;

public class WeissSchwarzDatabaseUpdater : DatabaseUpdater<CardDatabaseContext, WeissSchwarzCard>, IDatabaseUpdater<CardDatabaseContext, WeissSchwarzCard>
{
    public override ILogger Log => Serilog.Log.ForContext<WeissSchwarzDatabaseUpdater>();

    public delegate Task UpdateEventHandler(WeissSchwarzDatabaseUpdater sender, UpdateEventArgs args);
    public event UpdateEventHandler OnStarting;
    public event UpdateEventHandler OnEnding;

    public override Task OnLogEnding(UpdateEventArgs args) => OnStarting?.Invoke(this, args) ?? Task.CompletedTask;
    public override Task OnLogStarting(UpdateEventArgs args) => OnEnding?.Invoke(this, args) ?? Task.CompletedTask;
}

internal static class DatabaseUpdaterExtensions
{
    public static async Task UpdateCardDatabase(this IContainer ioc)
    {
        using (var db = ioc.GetInstance<CardDatabaseContext>())
            await ioc.GetInstance<IDatabaseUpdater<CardDatabaseContext, WeissSchwarzCard>>() //
                .Update(db, ioc.GetInstance<IActivityLogTranslator>());
    }
}
