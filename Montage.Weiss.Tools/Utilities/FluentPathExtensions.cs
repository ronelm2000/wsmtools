using Fluent.IO;

namespace Montage.Weiss.Tools.Utilities;

/// <summary>
/// Provides extension methods and static helpers that bridge System.IO and Fluent.IO functionality.
/// Use this class to add missing or unimplemented methods from System.IO that are not available in Fluent.IO.Path.
/// </summary>
public static class FluentPathExtensions
{
    /// <summary>
    /// Creates a temporary file path with the specified extension in the temp directory.
    /// </summary>
    /// <param name="extension">File extension (e.g., ".json"). If empty, no extension is added.</param>
    /// <returns>A Fluent.IO.Path object pointing to a temp file with the specified extension.</returns>
    public static Path GetTempFilePath(string extension = "")
    {
        var fileName = System.IO.Path.GetRandomFileName();
        if (!string.IsNullOrEmpty(extension))
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;
            fileName = System.IO.Path.ChangeExtension(fileName, extension);
        }
        return Path.Get(System.IO.Path.GetTempPath(), fileName);
    }

    /// <summary>
    /// Creates a file from the path, exposing a stream in the process. This assumes that the path is the file itself, and not a directory.
    /// </summary>
    public static ValueTask<System.IO.Stream> CreateFileStream(this Path path)
    {
        return ValueTask.FromResult<System.IO.Stream>(System.IO.File.Create(path.FullPath));
    }

    /// <summary>
    /// Returns a stream for reading from the specified path.
    /// </summary>
    public static System.IO.Stream GetStream(this Path path)
    {
        return System.IO.File.OpenRead(path.FullPath);
    }

    /// <summary>
    /// Reads a file as string from a file asynchronously using the TAP model.
    /// </summary>
    public static async Task<string> ReadStringAsync(this Path path, CancellationToken cancellationToken = default)
    {
        return await System.IO.File.ReadAllTextAsync(path.FullPath, cancellationToken);
    }

    /// <summary>
    /// Reads a file as byte array asynchronously using the TAP model.
    /// </summary>
    public static async Task<byte[]> ReadBytesAsync(this Path path, CancellationToken cancellationToken = default)
    {
        return await System.IO.File.ReadAllBytesAsync(path.FullPath, cancellationToken);
    }

    /// <summary>
    /// Writes to file using a specified stream action.
    /// </summary>
    public static async Task WriteAsync(this Fluent.IO.Path path, Action<System.IO.FileStream> streamAction, CancellationToken cancellationToken = default)
    {
        await using var stream = System.IO.File.OpenWrite(path.FullPath);
        streamAction?.Invoke(stream);
        await stream.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Returns a FileStream for writing to the specified path.
    /// </summary>
    public static System.IO.FileStream GetOpenWriteStream(this Fluent.IO.Path path)
    {
        return System.IO.File.OpenWrite(path.FullPath);
    }

    /// <summary>
    /// Writes a string to a file asynchronously.
    /// </summary>
    public static async Task WriteStringAsync(this Fluent.IO.Path path, string content, CancellationToken cancellationToken = default)
    {
        await System.IO.File.WriteAllTextAsync(path.FullPath, content, cancellationToken);
    }

    /// <summary>
    /// Deletes the file at the specified path.
    /// </summary>
    public static void Delete(this Fluent.IO.Path path)
    {
        System.IO.File.Delete(path.FullPath);
    }
}
