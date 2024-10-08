using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Flurl;
using Lamar;
using Montage.Weiss.Tools.CLI;
using Montage.Weiss.Tools.GUI.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.GUI.ViewModels.Dialogs;
public partial class ImportSetViewModel : ViewModelBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ImportSetViewModel>();

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private Func<MainWindowViewModel?> _parent;

    [ObservableProperty]
    private string setUrl;

    public ImportSetViewModel()
    {
        Parent = () => null;
        IsVisible = Design.IsDesignMode;
        SetUrl = string.Empty;
    }

    public async Task ParseSet(CancellationToken token = default)
    {
        if (Parent() is not MainWindowViewModel parentViewModel)
            return;
        if (parentViewModel.Container is not IContainer container)
            return;
        if (!Url.IsValid(SetUrl))
        {
            parentViewModel.Status = "Error parsing set: cannot validate url as valid.";
            return;
        }

        var progressReporter = new ProgressReporter(Log, message => parentViewModel.Status = message);
        var command = new ParseVerb {
            URI = SetUrl
        };
        command.SetParsed += Command_SetParsed;

        await command.Run(container, progressReporter, token);

        IsVisible = false;

        void Command_SetParsed(object? sender, string setCode)
        {
            parentViewModel.SearchBarText = $"set:{setCode}";
        }
    }
}
