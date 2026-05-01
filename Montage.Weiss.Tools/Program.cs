using CommandLine;
using Lamar;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Montage.Card.API.Interfaces.Inputs;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Services;
using Montage.Weiss.Tools.Impls.Utilities;
using Octokit;
using Serilog.Events;
using Serilog.Templates;
using System.Reflection;

namespace Montage.Weiss.Tools;

public class Program
{
    /// <summary>
    /// Should only be overriden in cases where the user is using a custom IConsole implementation, which will usually only be 
    /// in a test enviornment setting where IConsole can be mocked.
    /// </summary>
    public static IConsole Console { get; set; } = Card.API.CLI.Instance;

    /// <summary>
    /// Should only be used in testing, to provide a cancellation token that is cancelled when the test is cancelled, 
    /// so that the cancellation is properly propagated to the code being tested.
    /// </summary>
    public static CancellationToken TestCancellationToken { get; set; } = default;

    //    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Options))]
    public static async Task Main(string[] args)
    {
        Serilog.Log.Logger = BootstrapLogging().CreateLogger();

        Log.Information("Starting...");

        var container = Bootstrap();
        Log.Debug(container.WhatDoIHave(serviceType: typeof(IVerbCommand)));
        Log.Debug(container.WhatDoIHave(serviceType: typeof(IDeckParser<WeissSchwarzDeck, WeissSchwarzCard>)));
        Log.Debug(container.WhatDoIHave(serviceType: typeof(ICardSetParser<WeissSchwarzCard>)));

        var progressReporter = new Progress<CommandProgressReport>();
        using var cts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, TestCancellationToken);

        progressReporter.ProgressChanged += ProgressReporter_ProgressChanged;
        Console.CancelKeyPress += (s, e) => cts.Cancel();

        var verbs = container.GetAllInstances<IVerbCommand>().Select(a => a.GetType()).ToArray();
        var result = CommandLine.Parser.Default.ParseArguments(args, verbs); //
        await result.MapResult<IVerbCommand, Task>(
            async (verb) =>
            {
                await CheckLatestVersion();
                await verb.Run(container, progressReporter, linkedCts.Token);
            },
            async (errors) => await Display(errors)
        );
    }

    private static void ProgressReporter_ProgressChanged(object? sender, CommandProgressReport e)
    {
        if (!Console.IsOutputRedirected)
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
        Console.Write($"{e.ReportMessage.EN} [{e.Percentage}%]\r");
    }

    private static async Task CheckLatestVersion()
    {
        try
        {
            var github = new GitHubClient(new ProductHeaderValue("wsmtools"));
            //var user = await github.User.Get("ronelm2000");
            var wsmtoolsLatestRelease = await github.Repository.Release.GetLatest("ronelm2000", "wsmtools");
            var versionLatestRelease = Version.Parse(wsmtoolsLatestRelease.TagName.AsSpan(1));
            var appVersion = AppVersion;
            if (wsmtoolsLatestRelease.CreatedAt.DateTime > AppReleaseDate)
            //if (wsmtoolsLatestRelease.TagName != $"v{Program.AppVersion}")
            {
                Log.Information("The latest version is: {version}", wsmtoolsLatestRelease.Name);
                Log.Information($"Please see: {wsmtoolsLatestRelease.HtmlUrl}");
                Log.Information("If you have any problems with wsmtools, check it out; your issue might have already been fixed!");
            }
            else
            {
                Log.Information("The latest version is: {version}", AppVersion);
                Log.Information("The latest version on repository: {version}", wsmtoolsLatestRelease.Name);
            }
        }
        catch (Exception e)
        {
            Log.Warning("Unable to check the latest version (likely due to internet connection issues).");
            Log.Warning("If it is, some features of wsmtools may be unusable due to the need of an internet connection.");
            Log.Warning("Technical Details: {@e}", e);
        }
    }

    public static LoggerConfiguration BootstrapLogging()
    {
        var displayTemplate = new ExpressionTemplate(
            //"[{@t:HH:mm:ss}]" +
            "[{@l:u3}]" +
            "[{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}]" +
            //"{#if LayoutArea is not null}[{LayoutArea}]{#end}" +
            //"{#if ThreadId is not null}[{ThreadId}]{#end}" +
            " {@m}\n{@x}"
            );

        var config = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Debug)
            .WriteTo.Debug(displayTemplate, restrictedToMinimumLevel: LogEventLevel.Debug);

        if (!IsOutputRedirected)
            config = config.WriteTo.Console(displayTemplate, restrictedToMinimumLevel: LogEventLevel.Information);
        else
            config = config.WriteTo.File(displayTemplate, $"{AppDomain.CurrentDomain.BaseDirectory}/wstools.out.log", restrictedToMinimumLevel: LogEventLevel.Information);
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
            x.BootstrapDefaultServices();
        });
    }

    private static async Task Display(IEnumerable<Error> errors)
    {
        var makeCLIAppear = false;
        foreach (var error in errors)
        {
            if (error is HelpVerbRequestedError || error is NoVerbSelectedError)
            {
                Console.WriteLine("This is a CLI (Command Line Interface). You must use PowerShell or Command Prompt to use all of this application's functionalities.");
                makeCLIAppear = true;
            }
            else if (!(error is HelpVerbRequestedError || error is VersionRequestedError || error is BadVerbSelectedError))
                Log.Error("{@Error}", error);
        }
        if (makeCLIAppear && !Console.IsOutputRedirected) Console.ReadKey(false);
        await ValueTask.CompletedTask;
    }

    public static string AppVersion =>
        typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

    public static string AppFilePath =>
        System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath ?? "";
    public static DateTime AppReleaseDate =>
        System.IO.File.GetLastWriteTime(AppFilePath);
}

public static class LamarContainerExtensions
{
    public static ServiceRegistry BootstrapDefaultServices(this ServiceRegistry registry)
    {
        registry.AddLogging(l => l.AddSerilog(Serilog.Log.Logger, dispose: true));
        registry.AddSingleton<ILogger>(Serilog.Log.Logger);
        //x.AddLogging(l => l.AddSerilog(Serilog.Log.Logger, dispose: true));
        registry.AddSingleton<GlobalCookieJar>();
        registry.AddSingleton<DeckLogCacheService>();

        registry.Scan(s =>
        {
            s.AssemblyContainingType<Program>();
            s.AddAllTypesOf<IExportedDeckInspector<WeissSchwarzDeck, WeissSchwarzCard>>(ServiceLifetime.Singleton);
            s.AddAllTypesOf<IDeckExporter<WeissSchwarzDeck, WeissSchwarzCard>>(ServiceLifetime.Singleton);
            s.AddAllTypesOf<IFileOutCommandProcessor>(ServiceLifetime.Singleton);

            s.RegisterConcreteTypesAgainstTheFirstInterface();

            s.WithDefaultConventions(OverwriteBehavior.Never, ServiceLifetime.Singleton);
        });
        return registry;
    }
}
