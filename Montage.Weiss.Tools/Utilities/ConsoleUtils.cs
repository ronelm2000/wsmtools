using Montage.Card.API.Interfaces.Inputs;
using Montage.Card.API.Utilities;
using System.ComponentModel;

namespace Montage.Weiss.Tools.Utilities;

public static class ConsoleUtils
{
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(ConsoleUtils));

    public static Task<bool> IsPrompted(bool isNonInteractive, bool defeaultResult, CancellationToken cancellationToken = default)
        => IsPrompted(isNonInteractive, defeaultResult, Card.API.CLI.Instance, cancellationToken);

    /// <summary>
    /// This method prompts the user for a yes/no input. If the application is running in non-interactive mode or if the default result is true, it returns the default result without prompting. Otherwise, it waits for the user to press a key and returns true if the key is 'Y' (case-insensitive), and false otherwise. After reading the key, it clears the line by writing a carriage return.
    /// </summary>
    /// <param name="isNonInteractive"></param>
    /// <param name="defaultResult"></param>
    /// <param name="console"></param>
    /// <returns></returns>
    public static async Task<bool> IsPrompted(bool isNonInteractive, bool defaultResult, IConsole console, CancellationToken cancellationToken = default)
    {
        if (isNonInteractive || defaultResult)
            return defaultResult;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (console.KeyAvailable)
            {
                var result = Console.ReadKey(false).Key == ConsoleKey.Y;
                console.Write("\r");
                return result;
            }
            await Task.Delay(100, cancellationToken);
        }

        throw new TaskCanceledException();
    }

    public static void RunExecutable(string path, params string[] parameters)
    {
        path = TransformShortcuts(path);

        var cmd = $"{path} {parameters.ConcatAsString()}";
        Log.Information("Executing {command}", cmd);
        var process = new System.Diagnostics.Process();
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = path,
            Arguments = parameters.ConcatAsString() // $"\"{deckImagePath.FullPath.EscapeQuotes()}\"";
        };
        process.StartInfo = startInfo;

        try
        {
            if (process.Start())
                Log.Information("Command executed successfully.");

        }
        catch (Win32Exception)
        {
            Log.Warning("Command specified in --out failed; execute it manually.");
        }
    }

    private static string TransformShortcuts(string possibleShortcutExec)
    {
        return possibleShortcutExec.ToLower() switch
        {
            "sharex" when (Environment.OSVersion.Platform == PlatformID.Win32NT)
                => InstalledApplications.GetApplicationInstallPath("ShareX") + @"ShareX.exe",
            _ => possibleShortcutExec
        };
    }
}
