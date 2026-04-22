using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lamar;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Montage.Weiss.Tools.GUI.ViewModels;
using Montage.Weiss.Tools.GUI.Views;
using ReactiveUI;
using Serilog;

namespace Montage.Weiss.Tools.GUI;
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var container = InitializeIoC();
            desktop.MainWindow = container.GetInstance<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static IContainer InitializeIoC() => new Container(static c =>
    {
        c.BootstrapDefaultServices();
        c.Scan(s =>
        {
            s.AssemblyContainingType<MainWindowViewModel>();
            s.RegisterConcreteTypesAgainstTheFirstInterface();
            s.WithDefaultConventions(OverwriteBehavior.Never, ServiceLifetime.Singleton);
        });
    });
}