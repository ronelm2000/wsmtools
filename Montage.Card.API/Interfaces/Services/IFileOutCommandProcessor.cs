using System.IO;

namespace Montage.Card.API.Services;

public interface IFileOutCommandProcessor
{
    /// <summary>
    /// Provides an overridable delegate for saving a file to a destination.
    /// </summary>
    SaveStreamSupplier CreateFileStream { get; set; }
    
    public Task Process(string fullOutCommand, string fullFilePath, CancellationToken cancellationToken = default);

    public delegate Task<System.IO.Stream> SaveStreamSupplier (String destinationFolder, String fileName);
}
