using Montage.Card.API.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Utilities
{
    static class ConsoleUtils
    {
        private static ILogger Log;

        public static bool Prompted(bool isNonInteractive, bool defaultResult)
        {
            if (isNonInteractive || defaultResult) return defaultResult;
            var result = Console.ReadKey(false).Key == ConsoleKey.Y;
            Console.Write("\r");
            return result;
        }

        public static void RunExecutable(string path, params string[] parameters)
        {
            Log ??= Serilog.Log.ForContext(typeof(ConsoleUtils));
            path = TransformShortcuts(path);

            var cmd = $"{path} {parameters.ConcatAsString()}";
            Log.Information("Executing {command}", cmd);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = path;
            startInfo.Arguments = parameters.ConcatAsString(); // $"\"{deckImagePath.FullPath.EscapeQuotes()}\"";
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
}
