using Avalonia;
using Montage.Weiss.Tools.GUI.Extensions;
using System;
using ReactiveUI.Avalonia.Splat;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Montage.Weiss.Tools.GUI;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .LogToSerilog()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUIWithMicrosoftDependencyResolver(
                services =>
                {
                    services.AddLogging(config =>
                    {
                        config.AddSerilog();
                    });
                },
                withResolver: sp =>
                {
                }
            )
            .WithInterFont()
            .LogToTrace();
}