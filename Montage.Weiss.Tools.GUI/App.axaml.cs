using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Lamar;
using Montage.Weiss.Tools.GUI.ViewModels;
using Montage.Weiss.Tools.GUI.Views;
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
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel {
                    Parent = () => desktop.MainWindow!,
                    Container = new Container(c => {
                        c.BootstrapDefaultServices();
                        c.Scan(s =>
                        {
                            s.AssemblyContainingType<MainWindowViewModel>();
                            s.WithDefaultConventions();
                            s.RegisterConcreteTypesAgainstTheFirstInterface();
                        });
                   })
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}