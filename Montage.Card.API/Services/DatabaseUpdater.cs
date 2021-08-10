using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Services
{
    public abstract class DatabaseUpdater<ICDB, C> where ICDB : IDisposable, ICardDatabase<C> where C : ICard
    {
        public async Task Update(ICDB database, IActivityLogTranslator translator)
        {
            Log.Information("Migrating Database...");
            await database.Database.MigrateAsync();
            Log.Information("Updating Database Using Activity Log Queue...");
            var activityLog = await database.MigrationLog.AsQueryable()
                .Where(log => !log.IsDone)
                .OrderBy(log => log.DateAdded) 
                .AsAsyncEnumerable()
                .ToArrayAsync()
                ;

            foreach (var act in activityLog.Select((act, i) => (ActLog: act, Index: i)))
            {
                await OnLogStarting(new UpdateEventArgs(act.ActLog, act.Index, activityLog.Length));
                await translator.Perform(act.ActLog);
                // await act.ActLog.ToCommand().Run(ioc);
                act.ActLog.IsDone = true;
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
}
