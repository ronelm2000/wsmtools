using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
//                .Enrich.WithCaller()
                .WriteTo.Console(
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                    )
                .WriteTo.Debug(
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                    )
                .CreateLogger();

            Log.Information("Starting...");

            var container = Bootstrap();

            Log.Information(container.WhatDoIHave(serviceType: typeof(IDeckParser)));
            Log.Information(container.WhatDoIHave(serviceType: typeof(ICardSetParser)));


            var verbs = container.GetAllInstances<IVerbCommand>().Select(a => a.GetType()).ToArray();
            var result = CommandLine.Parser.Default.ParseArguments(args, verbs); //
            //                .MapResult(async options => await RunAndReturnExitCodeAsync(options), _ => Task.FromResult(1));
            //or more simpler using Method group
            // .MapResult(RunAndReturnExitCodeAsync), _ => Task.FromResult(1));
            await result.MapResult<IVerbCommand, Task>(
                (verb) => verb.Run(container), 
                (errors) => Display(errors)
            );
//            result.WithParsed<IVerbCommandAction>(async verb => await verb.Run(container));
//            result.WithParsed(async (options) => await Run(options, container));
//            result.WithNotParsed(async (errors) => await Display(errors));
            await Task.CompletedTask;
        //    Console.WriteLine("Hello World!");
        }

        /*
        private static async Task Run(Options options, IContainer container)
        {
//            Log.Information("Successful! The options are {@Options}", options);
            var uri = new Uri(options.URI);

            var cards = (await container.GetAllInstances<IURLParser>()
                .Where(parser => parser.IsCompatible(uri))
                .First()
                .Parse(uri)
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

            foreach (var pp in postProcessors)
                cards = pp.Process(cards);

            Log.Information("Successfully parsed: {uri}", uri);
        }
        */

        private static Container Bootstrap()
        {
            return new Container(x =>
            {
                x.Scan(s =>
                {
                    s.AssemblyContainingType<Program>();
                    s.WithDefaultConventions();
                    s.RegisterConcreteTypesAgainstTheFirstInterface();
                });

            });
        }

        private static Task Display(IEnumerable<Error> errors)
        {
            foreach (Error error in errors)
            {
                Log.Error("{@Error}", error);
            }
            return Task.CompletedTask;
        }

    }
}
