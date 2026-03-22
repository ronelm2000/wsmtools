using Avalonia;
using Avalonia.Logging;
using Serilog;
using Serilog.Templates;
using Splat;
using Splat.Serilog;
using System;

namespace Montage.Weiss.Tools.GUI.Extensions;
public static class AppBuilderExtensions
{
    public static AppBuilder LogToSerilog(this AppBuilder builder)
    {
        var displayTemplate = new ExpressionTemplate(
            "[{@t:yyyy/MM/dd HH:mm:ss}]" +
            "[{@l:u3}]" +
            "[{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}]" +
            "{#if LayoutArea is not null}[{LayoutArea}]{#end}" +
            "{#if ThreadId is not null}[{ThreadId}]{#end}" +
            " {@m}\n{@x}"
            );

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug(displayTemplate)
            .WriteTo.File(
                displayTemplate, 
                $"{AppDomain.CurrentDomain.BaseDirectory}/wsm-gui.log",
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug
                )
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .CreateLogger();

        Locator.CurrentMutable.UseSerilogFullLogger();

        Logger.Sink = new SerilogLogSink();

        return builder;
    }

}

internal class SerilogLogSink : ILogSink
{
    public bool IsEnabled(LogEventLevel level, string area)
    {
        if (area == "Layout")
            return false;
        return Serilog.Log.IsEnabled(ToSerilogLevel(level));
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        var _logger = Serilog.Log.Logger
            .ForContext(source?.GetType() ?? typeof(AppBuilder))
            .ForContext("LayoutArea", area);
        _logger.Write(ToSerilogLevel(level), messageTemplate);
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        var _logger = Serilog.Log.Logger
            .ForContext(source?.GetType() ?? typeof(AppBuilder))
            .ForContext("LayoutArea", area);
        _logger.Write(ToSerilogLevel(level), messageTemplate, propertyValues);
    }

    private Serilog.Events.LogEventLevel ToSerilogLevel(LogEventLevel level) {
        return level switch
        {
            LogEventLevel.Fatal => Serilog.Events.LogEventLevel.Fatal,
            LogEventLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogEventLevel.Information => Serilog.Events.LogEventLevel.Information,
            LogEventLevel.Debug => Serilog.Events.LogEventLevel.Debug,
            LogEventLevel.Verbose => Serilog.Events.LogEventLevel.Verbose,
            LogEventLevel.Error => Serilog.Events.LogEventLevel.Error,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }
}