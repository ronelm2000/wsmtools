using CommandLine;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Serilog.Log.Logger = BootstrapLogging().CreateLogger();

            Log.Information("Starting...");

            var container = Bootstrap();
            Log.Debug(container.WhatDoIHave(serviceType: typeof(IVerbCommand)));
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

        public static LoggerConfiguration BootstrapLogging()
        {
            var config = new LoggerConfiguration().MinimumLevel.Is(LogEventLevel.Debug)
                            .WriteTo.Debug(
                                restrictedToMinimumLevel: LogEventLevel.Debug,
                                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                            );

            if (!IsOutputRedirected)
                config = config.WriteTo.Console(
                                restrictedToMinimumLevel: LogEventLevel.Information,
                                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                                );
            else
                config = config.WriteTo.File(
                        "./wstools.out.log",
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                        );
            return config;
        }

        /// <summary>
        /// Indicates if stdout piping is being used and it's not dotnet test
        /// </summary>
        public static bool IsOutputRedirected => Console.IsOutputRedirected && System.Diagnostics.Trace.Listeners.Count < 2;

        public static Container Bootstrap()
        {
            return new Container(x =>
            {
                //x.AddLogging(l => l.AddSerilog(Serilog.Log.Logger, dispose: true));
                x.AddSingleton<ILogger>(Serilog.Log.Logger);
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
            var makeCLIAppear = false;
            foreach (Error error in errors)
            {
                if (error is HelpVerbRequestedError || error is NoVerbSelectedError)
                {
                    Console.WriteLine("This is a CLI (Command Line Interface). You must use PowerShell or Command Prompt to use all of this application's functionalities.");
                    makeCLIAppear = true;
                }
                else if (!(error is HelpVerbRequestedError || error is VersionRequestedError || error is BadVerbSelectedError))
                    Log.Error("{@Error}", error);
            }
            if (makeCLIAppear) Console.ReadKey(false);
            return Task.CompletedTask;
        }

    }
}
