namespace Dnn.PersonaBar.Extensions.Components.Security.Helper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Default implementation of <see cref="IFileHelper"/>.
    /// </summary>
    internal class FileHelper : IFileHelper
    {
        /// <inheritdoc cref="IFileHelper.GetReferencedAssemblyNames(string)" />
        public IEnumerable<string> GetReferencedAssemblyNames(string assemblyFilePath) =>
            this.LoadAssembly(assemblyFilePath)
                .GetReferencedAssemblies()
                .Select(assembly => assembly.FullName);

        /// <inheritdoc cref="IFileHelper.LoadAssembly(string)" />
        public Assembly LoadAssembly(string assemblyFilePath)
        {
            try
            {
                return Assembly.LoadFile(assemblyFilePath);
            }
            catch (Exception ex)
            {
                throw new IOException($"Could not load assembly '{assemblyFilePath}'", ex);
            }
        }

        /// <inheritdoc cref="IFileHelper.DirectoryGetFiles(string, string, SearchOption)" />
        public string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption) =>
            Directory.GetFiles(path, searchPattern, searchOption);

        /// <inheritdoc cref="IFileHelper.FileExists(string)" />
        public bool FileExists(string path) => File.Exists(path);
    }
}
