using Montage.Card.API.Utilities;
using Serilog;

namespace Montage.Card.API.Services;

public abstract class FileOutCommandProcessor : IFileOutCommandProcessor
{
    public abstract ILogger Log { get; }

    public async Task Process(string fullOutCommand, string fullFilePath)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT && fullOutCommand.ToLower() == "sharex")
            fullOutCommand = InstalledApplications.GetApplicationInstallPath("ShareX") + @"ShareX.exe";

        var cmd = $"{fullOutCommand} {fullFilePath}";
        Log.Information("Executing {command}", cmd);
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = fullOutCommand;
        startInfo.Arguments = $"\"{fullFilePath.EscapeQuotes()}\"";
        //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        //startInfo.RedirectStandardOutput = true;
        process.StartInfo = startInfo;

        try
        {
            if (process.Start())
            {
                Log.Information("Command executed successfully.");
                await process.WaitForExitAsync();
                Log.Information("Command exited successfully.");
            }
            //                        while (!process.HasExited)
            //                            Console.WriteLine(await process.StandardOutput.ReadLineAsync());
        }
        catch (System.ComponentModel.Win32Exception)
        {
            Log.Warning("Command specified in --out failed; execute it manually.");
        }
    }
}
