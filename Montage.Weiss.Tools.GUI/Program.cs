using Avalonia;
using Montage.Weiss.Tools.GUI.Extensions;
using System;
using ReactiveUI.Avalonia; // UseReactiveUI, RegisterReactiveUIViews* (core)
using ReactiveUI.Avalonia;
using Splat;
using ReactiveUI.Avalonia.Splat; // Autofac, DryIoc, Ninject, Microsoft.Extensions.DependencyInjection integrations

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
                },
                withResolver: sp =>
                {

                }
            )
            .WithInterFont()
            .LogToTrace();
}