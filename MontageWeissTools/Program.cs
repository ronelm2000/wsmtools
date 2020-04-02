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

            Log.Debug(container.WhatDoIHave(serviceType: typeof(IDeckParser)));
            Log.Debug(container.WhatDoIHave(serviceType: typeof(ICardSetParser)));

            var verbs = container.GetAllInstances<IVerbCommand>().Select(a => a.GetType()).ToArray();
            var result = CommandLine.Parser.Default.ParseArguments(args, verbs); //
            await result.MapResult<IVerbCommand, Task>(
                (verb) => verb.Run(container), 
                (errors) => Display(errors)
            );
            await Task.CompletedTask;
        }

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
