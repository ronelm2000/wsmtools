using CommandLine;
using Lamar;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;

namespace Montage.Weiss.Tools.CLI;

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
