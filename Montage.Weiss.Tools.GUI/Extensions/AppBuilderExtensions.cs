using Avalonia;
using Avalonia.Logging;
using Serilog;
using Splat;
using Splat.Serilog;
using System;

namespace Montage.Weiss.Tools.GUI.Extensions;
public static class AppBuilderExtensions
{
    public static AppBuilder LogToSerilog(this AppBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .WriteTo.File($"{AppDomain.CurrentDomain.BaseDirectory}/wsm-gui.log", Serilog.Events.LogEventLevel.Debug)
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
        Serilog.Log.Logger.ForContext(source?.GetType() ?? typeof(AppBuilder)).Write(ToSerilogLevel(level), $"[{area}] {messageTemplate}");
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        Serilog.Log.Logger.ForContext(source?.GetType() ?? typeof(AppBuilder)).Write(ToSerilogLevel(level), $"[{area}] {messageTemplate}", propertyValues);
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