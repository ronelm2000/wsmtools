using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Montage.Card.API.Interfaces.Components;
using Montage.Card.API.Interfaces.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.CLI
{
    [Verb("parse", HelpText = "Exports a card release set into the local database, so that it may be used to export decks later.")]
    public class ParseVerb : IVerbCommand, IParseInfo
    {
        [Value(0, HelpText = "URL to parse. Compatible Formats are found at https://github.com/ronelm2000/wsmtools/")]
        public string URI { get; set; }

        [Option("with", HelpText = "Provides a hint as to what parser should be used or if post-processors are skipped (if any).", Default = new string[] { })]
        public IEnumerable<string> ParserHints { get; set; } = new string[] { };

        public async Task Run(IContainer container, IProgress<CommandProgressReport> progress, CancellationToken ct)
        {
            var Log = Serilog.Log.ForContext<ParseVerb>();
            var parser = await container.GetAllInstances<ICardSetParser<WeissSchwarzCard>>()
                .ToAsyncEnumerable()
                .WhereAwait(async parser => await parser.IsCompatible(this))
                .FirstAsync();

            var redirector = new CommandProgressAggregator(progress);
            var cardList = await parser.Parse(URI, redirector, ct).ToListAsync(ct);
            var cards = cardList.Distinct(WeissSchwarzCard.SerialComparer).ToAsyncEnumerable();

            var postProcessors = await container.GetAllInstances<ICardPostProcessor<WeissSchwarzCard>>()
                .ToAsyncEnumerable()
                .WhereAwait(async processor => await processor.IsCompatible(cardList))
                .Where(processor => (parser is IFilter<ICardPostProcessor<WeissSchwarzCard>> filter) ? filter.IsIncluded(processor) : true)
                .WhereAwait(async processor => (processor is ISkippable<IParseInfo> skippable) ? await skippable.IsIncluded(this) : true)
                .WhereAwait(async processor => (processor is ISkippable<ICardSetParser<WeissSchwarzCard>> skippable) ? await skippable.IsIncluded(parser) : true)
                .OrderByDescending(processor => processor.Priority)
                .ToArrayAsync(ct)
                ;

            redirector.PostProcessorCount = postProcessors.Length;
            cards = postProcessors.Aggregate(cards, (pp, cs) => cs.Process(pp, redirector, ct));

            await container.UpdateCardDatabase(redirector, ct);

            using (var db = container.GetInstance<CardDatabaseContext>())
            {
                await foreach (var card in cards)
                {
                    card.VersionTimestamp = Program.AppVersion;
                    var dups = db.WeissSchwarzCards.AsQueryable<WeissSchwarzCard>().Where(c => c.Serial == card.Serial).ToArray();
                    if (dups.Length > 0)
                        db.WeissSchwarzCards.RemoveRange(dups); // delete all the dups, based on serial.
                    db.Add(card);                    
                    Log.Information("Added to DB: {serial}", card.Serial);
                }

                progress.Report(new CommandProgressReport
                {
                    ReportMessage = new MultiLanguageString
                    {
                        EN = "Saving all changes..."
                    },
                    Percentage = 75
                });
                await db.SaveChangesAsync();
            }

            Log.Information("Successfully parsed: {uri}", URI);
            progress.Report(new CommandProgressReport
            {
                ReportMessage = new MultiLanguageString
                {
                    EN = $"Successfully parsed: {URI}"
                },
                Percentage = 100
            });
        }
    }

    internal class CommandProgressAggregator : IProgress<SetParserProgressReport>, IProgress<PostProcessorProgressReport>, IProgress<DatabaseUpdateReport>
    {
        private CommandProgressReport _totalReport = new CommandProgressReport();
        private IProgress<CommandProgressReport> _progress;
        public int PostProcessorCount { get; internal set; }

        public CommandProgressAggregator(IProgress<CommandProgressReport> progress)
        {
            _progress = progress;
        }

        public void Report(DatabaseUpdateReport value)
        {
            _totalReport = _totalReport with
            {
                Percentage = 0 + (int)(value.Percentage * 0.15f),
                ReportMessage = value.ReportMessage
            };
            _progress.Report(_totalReport);
        }

        public void Report(SetParserProgressReport value)
        {
            _totalReport = _totalReport with
            {
                Percentage = 15 + (int)(value.Percentage * 0.80f),
                ReportMessage = value.ReportMessage
            };
            _progress.Report(_totalReport);
        }

        public void Report(PostProcessorProgressReport value)
        {
            // TODO: Maybe need to change how total percentage computation works?
            /*
            _totalReport = _totalReport with
            {
                Percentage = 25 + (int)(value.Percentage * 100 * 0.25f),
                ReportMessage = value.ReportMessage
            };
            _progress.Report(_totalReport);
            */
        }

    }
}
