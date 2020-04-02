using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.CLI
{
    [Verb("parse", HelpText = "Exports a card release set into the local database, so that it may be used to export decks later.")]
    public class ParseVerb : IVerbCommand
    {
        [Value(0, HelpText = "URL to parse. Compatible Formats are: HOTC Set Translation links, and Encore Decks Set API links.")]
        public string URI { get; set; }

        public async Task Run(IContainer container)
        {
            var Log = Serilog.Log.ForContext<ParseVerb>();
            //Log.Information("Successful! The options are {@Options}", options);
            //var uri = new Uri(URI);

            var cards = (await container.GetAllInstances<ICardSetParser>()
                .Where(parser => parser.IsCompatible(URI))
                .First()
                .Parse(URI)
                .ToListAsync())
                .ToAsyncEnumerable()
                ;

            var postProcessors = container.GetAllInstances<ICardPostProcessor>()
                .OrderBy(processor => processor.Priority);

            cards = postProcessors.Aggregate(cards, (pp, cs) => cs.Process(pp));

            using (var db = container.GetInstance<CardDatabaseContext>())
            {
                await db.Database.MigrateAsync();

                await foreach (var card in cards)
                {
                    // Do nothing for now.
                    //Log.Information("Card: {@Card}", card);
                    var dups = db.WeissSchwarzCards.AsQueryable<WeissSchwarzCard>().Where(c => c.Serial == card.Serial).ToArray();
                    if (dups.Length > 0)
                        db.WeissSchwarzCards.RemoveRange(dups);

                    db.Add(card);

                    //db.WeissSchwarzCards.Add(card);
                    Log.Information("Added to DB: {serial}", card.Serial);
                }

                await db.SaveChangesAsync();
            }

//            foreach (var pp in postProcessors)
//                cards = pp.Process(cards);

            Log.Information("Successfully parsed: {uri}", URI);
        }
    }
}
