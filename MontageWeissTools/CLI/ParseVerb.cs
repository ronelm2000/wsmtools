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
    public class ParseVerb : IVerbCommand, IParseInfo
    {
        [Value(0, HelpText = "URL to parse. Compatible Formats are found at https://github.com/ronelm2000/wsmtools/")]
        public string URI { get; set; }

        [Option("with", HelpText = "Provides a hint as to what parser should be used.", Default = new string[] { })]
        public IEnumerable<string> ParserHints { get; set; }

        public async Task Run(IContainer container)
        {
            var Log = Serilog.Log.ForContext<ParseVerb>();

            var cardList = await container.GetAllInstances<ICardSetParser>()
                .Where(parser => parser.IsCompatible(this))
                .First()
                .Parse(URI)
                .ToListAsync();
            var cards = cardList.Distinct(WeissSchwarzCard.SerialComparer).ToAsyncEnumerable();

            var postProcessors = container.GetAllInstances<ICardPostProcessor>()
                .Where(procesor => procesor.IsCompatible(cardList))
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
