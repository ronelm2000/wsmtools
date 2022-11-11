using Fluent.IO;

namespace Montage.Weiss.Tools.Utilities;

public static class FluentPathExtensions
{
    /// <summary>
    /// Creates a file from the path, exposing a stream in the process.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static Fluent.IO.Path CreateFile(this Path path, string filename, Action<System.IO.FileStream> streamAction)
    {
        // if Path is null TODO: make an exception
        using (var stream = System.IO.File.Create(path.FullPath))
            streamAction?.Invoke(stream);
        return path.Files();
    }

    public static System.IO.Stream GetStream(this Path path)
    {
        return System.IO.File.OpenRead(path.FullPath);
    }

    /// <summary>
    /// Reads a file as string from a file asynchronously using the TAP model.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> ReadStringAsync(this Path path, CancellationToken cancellationToken = default)
    {
        return await System.IO.File.ReadAllTextAsync(path.FullPath, cancellationToken);
    }

    /// <summary>
    /// Reads a file as byte array asynchronously using the TAP model.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<byte[]> ReadBytesAsync(this Path path, CancellationToken cancellationToken = default)
    {
        return await System.IO.File.ReadAllBytesAsync(path.FullPath, cancellationToken);
    }

    /// <summary>
    /// Writes to file using a specified stream action.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="streamAction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task WriteAsync(this Fluent.IO.Path path, Action<System.IO.FileStream> streamAction, CancellationToken cancellationToken = default)
    {
        await using var stream = System.IO.File.OpenWrite(path.FullPath);
        streamAction?.Invoke(stream);
        await stream.FlushAsync(cancellationToken);
    }

    public static System.IO.FileStream GetOpenWriteStream(this Fluent.IO.Path path)
    {
        return System.IO.File.OpenWrite(path.FullPath);
    }

    /// 
    /// <summary>
    /// Creates a file under the first path in the set.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="fileContent">The content of the file.</param>
    /// <returns>A set with the created file.</returns>
    // public static Path CreateFile(this Path path, string fileName, string fileContent) => path.First().CreateFiles(p => path.Create(fileName), p => fileContent);
}
