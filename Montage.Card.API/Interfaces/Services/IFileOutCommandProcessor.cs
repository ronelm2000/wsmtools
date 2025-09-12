using System.IO;

namespace Montage.Card.API.Services;

public interface IFileOutCommandProcessor
{
    /// <summary>
    /// Provides an overridable delegate for saving a file to a destination.
    /// </summary>
    SaveStreamSupplier CreateFileStream { get; set; }

    /// <summary>
    /// Provides an overridable delegate for opening a file by its default action. For example, in Windows, a docx file
    /// should open Word or LibreOffice, and in Android it should open the Open As screen.
    /// </summary>
    OpenFileSupplier OpenFile { get; set; }

    /// <summary>
    /// Provides an overridable delegate for opening a URL by its default action. For example, in Windows, a http(s) URL
    /// should open the default browser, and in Android it should open the default browser or app that handles the URL.
    /// </summary>
    OpenURLSupplier OpenURL { get; set; }

    public Task Process(string fullOutCommand, string fullFilePath, CancellationToken cancellationToken = default);

    public delegate Task<System.IO.Stream> SaveStreamSupplier(String destinationFolder, String fileName);
    public delegate Task OpenFileSupplier(String fileFolder, String fileName);
    public delegate Task OpenURLSupplier(String url);
}
