namespace Dnn.PersonaBar.Extensions.Components.Security.Helper
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// An abstraction of .NET framework classes to enable unit testing.
    /// </summary>
    internal interface IFileHelper
    {
        /// <summary>
        /// Similar to <see cref="Assembly.GetReferencedAssemblies"/>,
        /// except this method loads the assembly from a file path
        /// and returns the assembly full names only.
        /// </summary>
        /// <param name="assemblyFilePath">
        /// The assembly file path.
        /// </param>
        /// <returns>
        /// A <see cref="IEnumerable{T}"/> containing the full names of all referenced assemblies.
        /// </returns>
        IEnumerable<string> GetReferencedAssemblyNames(string assemblyFilePath);

        /// <inheritdoc cref="Assembly.LoadFile(string)"/>
        Assembly LoadAssembly(string assemblyFilePath);

        /// <inheritdoc cref="File.Exists(string)" />
        bool FileExists(string path);

        /// <inheritdoc cref="Directory.GetFiles(string, string, SearchOption)"/>
        string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption);
    }
}
