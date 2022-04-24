using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Services;
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
        public async Task Run(IContainer ioc, IProgress<CommandProgressReport> progress, CancellationToken cancellationToken = default)
        {
            var translator = ioc.GetInstance<IActivityLogTranslator>();
            using (var db = ioc.GetInstance<CardDatabaseContext>())
                await ioc.GetInstance<IDatabaseUpdater<CardDatabaseContext, WeissSchwarzCard>>().Update(db, translator, new DatabaseUpdateArgs { DisplayLogOverride = true });
        }
    }
}
