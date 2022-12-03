using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.Test.Commons;

internal class TestUtils
{
    public static LoggerConfiguration BootstrapLogging(Func<LoggerConfiguration, LoggerConfiguration> additionalActions = null)
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
