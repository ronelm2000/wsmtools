using CommandLine;
using Flurl.Http;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Montage.Card.API.Interfaces.Services;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.API;
using Montage.Weiss.Tools.Entities;
using Montage.Weiss.Tools.Impls.Services;
using Montage.Weiss.Tools.Impls.Utilities;
using Octokit;
using Serilog.Events;
using System.Reflection;

namespace Montage.Weiss.Tools;

public class Program
{
    public static async Task Main(string[] args)
    {
        Serilog.Log.Logger = BootstrapLogging().CreateLogger();

        Log.Information("Starting...");

        var container = Bootstrap();
        Log.Debug(container.WhatDoIHave(serviceType: typeof(IVerbCommand)));
        Log.Debug(container.WhatDoIHave(serviceType: typeof(IDeckParser<WeissSchwarzDeck, WeissSchwarzCard>)));
        Log.Debug(container.WhatDoIHave(serviceType: typeof(ICardSetParser<WeissSchwarzCard>)));
        FlurlHttp.Configure(settings => settings.HttpClientFactory = container.GetService<PollyHttpClientFactory>());

        var progressReporter = new Progress<CommandProgressReport>();
        var cts = new CancellationTokenSource();
        progressReporter.ProgressChanged += ProgressReporter_ProgressChanged;
        Console.CancelKeyPress += (s, e) => cts.Cancel();

        var verbs = container.GetAllInstances<IVerbCommand>().Select(a => a.GetType()).ToArray();
        var result = CommandLine.Parser.Default.ParseArguments(args, verbs); //
        await result.MapResult<IVerbCommand, Task>(
            async (verb) => {
                await CheckLatestVersion();
                await verb.Run(container, progressReporter);
            },
            (errors) => Display(errors)
        );
        await Task.CompletedTask;
    }

    private static void ProgressReporter_ProgressChanged(object sender, CommandProgressReport e)
    {
        Console.Write($"{e.ReportMessage.EN} [{e.Percentage}%]\r");
    }

    private static async Task CheckLatestVersion()
    {
        try
        {
            var github = new GitHubClient(new ProductHeaderValue("wsmtools"));
            //var user = await github.User.Get("ronelm2000");
            var wsmtoolsLatestRelease = await github.Repository.Release.GetLatest("ronelm2000", "wsmtools");
            var versionLatestRelease = Version.Parse(wsmtoolsLatestRelease.TagName.Substring(1));
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
        } catch (Exception e)
        {
            Log.Warning("Unable to check the latest version (likely due to internet connection issues).");
            Log.Warning("If it is, some features of wsmtools may be unusable due to the need of an internet connection.");
            Log.Warning("Technical Details: {@e}", e);
        }
    }

    public static LoggerConfiguration BootstrapLogging()
    {
        var config = new LoggerConfiguration().MinimumLevel.Is(LogEventLevel.Debug)
                        .WriteTo.Debug(
                            restrictedToMinimumLevel: LogEventLevel.Debug,
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                        );

        if (!IsOutputRedirected)
            config = config.WriteTo.Console(
                            restrictedToMinimumLevel: LogEventLevel.Information,
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                            );
        else
            config = config.WriteTo.File(
                    "./wstools.out.log",
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                    );
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
            //x.AddLogging(l => l.AddSerilog(Serilog.Log.Logger, dispose: true));
            x.AddSingleton<GlobalCookieJar>();
            x.AddSingleton<ILogger>(Serilog.Log.Logger);
            x.AddSingleton<DeckLogCacheService>();
            x.Scan(s =>
            {
                s.AssemblyContainingType<Program>();
                s.WithDefaultConventions();
                s.RegisterConcreteTypesAgainstTheFirstInterface();
            });

        });
    }

    private static Task Display(IEnumerable<Error> errors)
    {
        var makeCLIAppear = false;
        foreach (Error error in errors)
        {
            if (error is HelpVerbRequestedError || error is NoVerbSelectedError)
            {
                Console.WriteLine("This is a CLI (Command Line Interface). You must use PowerShell or Command Prompt to use all of this application's functionalities.");
                makeCLIAppear = true;
            }
            else if (!(error is HelpVerbRequestedError || error is VersionRequestedError || error is BadVerbSelectedError))
                Log.Error("{@Error}", error);
        }
        if (makeCLIAppear) Console.ReadKey(false);
        return Task.CompletedTask;
    }

    public static string AppVersion =>
        typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

    public static DateTime AppReleaseDate =>
        System.IO.File.GetLastWriteTime(typeof(Program).Assembly.Location);
}
