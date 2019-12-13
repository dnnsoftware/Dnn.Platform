using System.IO;

namespace DotNetNuke.Services.FileSystem.Internal
{
    /// <summary>
    /// File Content Security Checker.
    /// </summary>
    public interface IFileSecurityChecker
    {
        /// <summary>
        /// Checks if the file has valid content.
        /// </summary>
        /// <param name="fileContent">The File Content.</param>
        bool Validate(Stream fileContent);
    }
}
