using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.Entities;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Test
{
    [TestClass]
    public class IntegrationTests
    {
        IContainer ioc; //= Bootstrap();

        [TestMethod("Full Integration Test (Typical Use Case)")]
        public async Task FullTestRun()
        {
            Serilog.Log.Logger = BootstrapLogging().CreateLogger();
            ioc = Program.Bootstrap();

            await new ParseVerb(){ 
                URI = "https://heartofthecards.com/translations/love_live!_sunshine_school_idol_festival_6th_anniversary_booster_pack.html" 
                }.Run(ioc);

            await new ParseVerb()
            {
                URI = "https://heartofthecards.com/translations/love_live!_sunshine_vol._2_booster_pack.html"
            }.Run(ioc);

            var parseCommand = new ExportVerb("https://www.encoredecks.com/deck/wDdTKywNh", nonInteractive: true);
            await parseCommand.Run(ioc);
        }

        /*
        private static Container Bootstrap(Action<ServiceRegistry> additionalActions)
        {
            return new Container(x =>
            {
                x.AddTransient<ExportVerb>();
                x.AddTransient<ParseVerb>();
                x.AddDbContext<CardDatabaseContext>();
                x.AddLogging(l => l.AddSerilog(dispose: true));
                additionalActions(x);
            });
        }
        */
        
        private static LoggerConfiguration BootstrapLogging(Func<LoggerConfiguration, LoggerConfiguration> additionalActions = null)
        {
            additionalActions ??= (lc => lc);
            var config = new LoggerConfiguration().MinimumLevel.Is(LogEventLevel.Debug)
                                .WriteTo.Trace(
                                    restrictedToMinimumLevel: LogEventLevel.Debug,
                                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                                );
            config = additionalActions(config);
            return config;
        }
        
    }
}
