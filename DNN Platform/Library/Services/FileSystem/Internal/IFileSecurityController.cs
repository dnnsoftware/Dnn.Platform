using System.IO;

namespace DotNetNuke.Services.FileSystem.Internal
{
    /// <summary>
    /// Internal class to check file security.
    /// </summary>
    public interface IFileSecurityController
    {
        /// <summary>
        /// Checks if the file has valid content.
        /// </summary>
        /// <param name="fileName">The File Name.</param>
        /// <param name="fileContent">The File Content.</param>
        bool Validate(string fileName, Stream fileContent);
    }
}
