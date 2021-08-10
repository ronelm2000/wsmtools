using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.CLI
{
    [Verb("update", HelpText = "Updates the database using the present Activity Log.")]
    public class UpdateVerb : IVerbCommand
    {
        private ILogger Log = Serilog.Log.ForContext<UpdateVerb>();

        public delegate Task UpdateEventHandler(UpdateVerb sender, UpdateEventArgs args);

        public event UpdateEventHandler OnStarting;
        public event UpdateEventHandler OnEnding;

        public async Task Run(IContainer ioc)
        {
            var translator = ioc.GetInstance<IActivityLogTranslator>();
            using (var db = ioc.GetInstance<CardDatabaseContext>())
            {
                await db.Database.MigrateAsync();
                Log.Information("Updating Database Using Activity Log Queue...");
                var activityLog = await db.MigrationLog.AsQueryable()
                    .Where(log => !log.IsDone)
                    .OrderBy(log => log.DateAdded)
                    .AsAsyncEnumerable()
                    .ToArrayAsync()
                    ;
                foreach (var act in activityLog.Select((act,i)=>(ActLog: act, Index: i)))
                {
                    await (OnStarting?.Invoke(this, new UpdateEventArgs(act.ActLog, act.Index, activityLog.Length)) ?? Task.CompletedTask);
                    await translator.Perform(act.ActLog);
                    // await act.ActLog.ToCommand().Run(ioc);
                    act.ActLog.IsDone = true;
                    await (OnEnding?.Invoke(this, new UpdateEventArgs(act.ActLog, act.Index + 1, activityLog.Length)) ?? Task.CompletedTask);
                }
                Log.Information("Done!");
                await db.SaveChangesAsync();
            }
        }
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
