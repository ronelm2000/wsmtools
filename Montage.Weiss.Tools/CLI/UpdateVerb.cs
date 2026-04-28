using CommandLine;
using Lamar;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;

namespace Montage.Weiss.Tools.CLI;

[Verb("update", HelpText = "Updates cards in the database using specified post-processors.")]
public class UpdateVerb : IVerbCommand
{
    [Value(0, HelpText = "Release IDs to update (semicolon-separated, e.g. W53;WE27)", Required = true)]
    public string ReleaseIDs { get; set; } = "";

    [Value(1, HelpText = "Post-processor aliases (semicolon-separated, e.g. yyt;decklog)", Required = false)]
    public string PostProcessorAliases { get; set; } = "";

    public async Task Run(IContainer ioc, IProgress<CommandProgressReport> progress, CancellationToken ct = default)
    {
        var releaseIds = ReleaseIDs.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToHashSet();

        // If no Release IDs provided, run database migration (original behavior)
        if (releaseIds.Count == 0)
        {
            var translator = ioc.GetInstance<IActivityLogTranslator>();
            using (var migrationDb = ioc.GetInstance<CardDatabaseContext>())
            {
                await ioc.GetInstance<IDatabaseUpdater<CardDatabaseContext, WeissSchwarzCard>>()
                    .Update(migrationDb, translator, new DatabaseUpdateArgs { DisplayLogOverride = true });
            }
            return;
        }

        var aliasSet = PostProcessorAliases.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToLower())
            .ToHashSet();

        using (var db = ioc.GetInstance<CardDatabaseContext>())
        {
            var cards = db.WeissSchwarzCards
                .Where(c => releaseIds.Contains(c.ReleaseID))
                .ToList();

            if (cards.Count == 0)
            {
                progress.Report(new CommandProgressReport
                {
                    ReportMessage = new MultiLanguageString { EN = "No cards found for the specified Release IDs." },
                    Percentage = 100
                });
                return;
            }

            var redirector = new CommandProgressAggregator(progress);

            var postProcessors = await ioc.GetAllInstances<ICardPostProcessor<WeissSchwarzCard>>()
                .ToAsyncEnumerable()
                .Where(async (pp, c) => await pp.IsCompatible(cards, c))
                .Where(pp => aliasSet.Count == 0 || pp.Alias.Any(a => aliasSet.Contains(a.ToLower())))
                .OrderByDescending(pp => pp.Priority)
                .ToArrayAsync(ct);

            if (postProcessors.Length == 0)
            {
                progress.Report(new CommandProgressReport
                {
                    ReportMessage = new MultiLanguageString { EN = "No compatible post-processors found." },
                    Percentage = 100
                });
                return;
            }

            redirector.PostProcessorCount = postProcessors.Length;

            var processedCards = postProcessors.Aggregate(
                cards.ToAsyncEnumerable(),
                (cs, pp) => pp.Process(cs, redirector, ct));

            var updatedCards = await processedCards
                .Select(c =>
                {
                    c.VersionTimestamp = Program.AppVersion;
                    return c;
                })
                .DistinctBy(c => c.Serial)
                .ToDictionaryAsync(c => c.Serial, cancellationToken: ct);

            progress.Report(new CommandProgressReport
            {
                ReportMessage = new MultiLanguageString { EN = "Saving all changes..." },
                Percentage = 75
            });

            var keys = updatedCards.Keys.ToList();
            db.RemoveRange(db.WeissSchwarzCards.Where(c => keys.Contains(c.Serial)));
            await db.SaveChangesAsync(ct);

            await db.AddRangeAsync(updatedCards.Values, ct);
            await db.SaveChangesAsync(ct);

            progress.Report(new CommandProgressReport
            {
                ReportMessage = new MultiLanguageString { EN = $"Successfully updated {keys.Count} cards." },
                Percentage = 100
            });
        }
    }
}
