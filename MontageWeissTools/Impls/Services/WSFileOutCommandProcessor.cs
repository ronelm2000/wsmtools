using Fluent.IO;
using Montage.Card.API.Services;
using Montage.Weiss.Tools.Utilities;

namespace Montage.Weiss.Tools.Impls.Services;

public class WSFileOutCommandProcessor : FileOutCommandProcessor
{
    public override ILogger Log => Serilog.Log.ForContext<WSFileOutCommandProcessor>();
    public override IFileOutCommandProcessor.SaveStreamSupplier CreateFileStream { get; set; } = GenerateSaveFileStream;

    private static async Task<System.IO.Stream> GenerateSaveFileStream(String destinationFolder, String fileName)
    {
        if (!string.IsNullOrWhiteSpace(destinationFolder))
            return await Path.Get(destinationFolder, fileName).CreateFileStream();
        else
            return await Path.Current.Combine(fileName).CreateFileStream();
    }
}
