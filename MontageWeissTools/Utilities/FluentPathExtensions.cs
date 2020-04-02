using Fluent.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.Utilities
{
    public static class FluentPathExtensions
    {
        /// <summary>
        /// Creates a file from the path, exposing a stream in the process.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Path CreateFile(this Path path, string filename, Action<System.IO.FileStream> streamAction)
        {
            // if Path is null TODO: make an exception
            using (var stream = System.IO.File.Create(path.FullPath))
                streamAction?.Invoke(stream);
            return path.Files();
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
}
