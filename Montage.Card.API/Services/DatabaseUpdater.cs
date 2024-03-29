﻿using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Services;
using Serilog;

namespace Montage.Card.API.Services;

public abstract class DatabaseUpdater<ICDB, C> where ICDB : IDisposable, ICardDatabase<C> where C : ICard
{
    public async Task Update(ICDB database, IActivityLogTranslator translator, DatabaseUpdateArgs? args = default)
    {
        if (args?.DisplayLogOverride ?? false || (await database.Database.GetPendingMigrationsAsync()).Count() > 0)
            Log.Information("Migrating Database...");
        await database.Database.MigrateAsync();
        
        var activityLog = await database.MigrationLog.AsQueryable()
            .Where(log => !log.IsDone)
            .OrderBy(log => log.DateAdded) 
            .AsAsyncEnumerable()
            .ToArrayAsync()
            ;
        if (args?.DisplayLogOverride ?? false || activityLog.Length > 0)
        {
            Log.Information("Pending Database Updates [{length}]", activityLog.Length);
        }

        foreach (var act in activityLog.Select((act, i) => (ActLog: act, Index: i)))
        {
            Log.Information("{verb}: {target}", act.ActLog.Activity.ToVerbString(), act.ActLog.Target);
            await OnLogStarting(new UpdateEventArgs(act.ActLog, act.Index, activityLog.Length));
            await translator.Perform(act.ActLog);
            await OnLogEnding(new UpdateEventArgs(act.ActLog, act.Index + 1, activityLog.Length));
        }
        Log.Information("Done!");
        await database.SaveChangesAsync();

    }

    public abstract ILogger Log { get; }
    public abstract Task OnLogStarting(UpdateEventArgs args);
    public abstract Task OnLogEnding(UpdateEventArgs args);
}

public class UpdateEventArgs
{
    public string Status { get; private set; }
    public double UpdateProgress { get; private set; }

    public UpdateEventArgs(ActivityLog act, int index, int length)
    {
        UpdateProgress = (double)index / length;
        Status = $"{act.Activity.ToVerbString()} ({Math.Floor(UpdateProgress * 100)}%)\n{act.Target}";
    }
}
