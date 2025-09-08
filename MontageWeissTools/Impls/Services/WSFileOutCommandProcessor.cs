using Fluent.IO;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Montage.Weiss.Tools.Impls.Services;

public class WSFileOutCommandProcessor : FileOutCommandProcessor
{
    public override ILogger Log => Serilog.Log.ForContext<WSFileOutCommandProcessor>();
    public override IFileOutCommandProcessor.SaveStreamSupplier CreateFileStream { get; set; } = GenerateSaveFileStream;
    public override IFileOutCommandProcessor.OpenFileSupplier OpenFile { get; set; } = OpenFileByDefault;

    private static async Task<System.IO.Stream> GenerateSaveFileStream(String destinationFolder, String fileName)
    {
        if (!string.IsNullOrWhiteSpace(destinationFolder))
            return await Path.Get(destinationFolder, fileName).CreateFileStream();
        else
            return await Path.Current.Combine(fileName).CreateFileStream();
    }

    private static async Task OpenFileByDefault(String destinationFolder, String fileName)
    {
        var path = string.IsNullOrWhiteSpace(destinationFolder) ? Path.Current.Combine(fileName) : Path.Get(destinationFolder, fileName);
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                System.Diagnostics.Process.Start(new ProcessStartInfo(path.FullPath) { UseShellExecute = true });
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                System.Diagnostics.Process.Start("xdg-open", path.FullPath);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                System.Diagnostics.Process.Start("open", path.FullPath);
        }
        catch (PlatformNotSupportedException e)
        {
            Serilog.Log.ForContext<WSFileOutCommandProcessor>().Warning(e, "Platform not supported to open this file, ignoring.");
        }
        await Task.CompletedTask;
    }
}
