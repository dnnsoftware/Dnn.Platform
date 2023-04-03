using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace DeployClient
{
    internal class PackageCrawler
    {
        private readonly IFileSystem _fileSystem;
        private readonly IDirectoryInfo _packageDirectoryInfo;



        /// <summary>
        /// The directory that contains to installing files.
        /// </summary>
        public string PackageDirectoryPath => _packageDirectoryInfo.FullName;
        


        public PackageCrawler(string packageDirectoryPath) : this(new FileSystem(), packageDirectoryPath)
        {
        }

        public PackageCrawler(IFileSystem fileSystem, string packageDirectoryPath)
        {
            _fileSystem = fileSystem;

            _packageDirectoryInfo = GetPackageDirectory(packageDirectoryPath);
        }



        private IDirectoryInfo GetPackageDirectory(string packageDirectoryPath)
        {
            var path = string.IsNullOrWhiteSpace(packageDirectoryPath)
                ? _fileSystem.Directory.GetCurrentDirectory()
                : packageDirectoryPath;

            var directoryInfo = _fileSystem.DirectoryInfo.FromDirectoryName(path);

            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException($"Directory \"{directoryInfo.FullName}\" not found.");
            }

            return directoryInfo;
        }

        /// <summary>
        /// Returns all installation packages (*.zip files) full file paths which were found in defined <see cref="PackageDirectoryPath"/>.
        /// </summary>
        /// <returns>Enumerable of full file paths.</returns>
        internal IEnumerable<string> GetPackagesFullPaths()
        {
            const string searchPattern = "*.zip";

            return _packageDirectoryInfo.GetFiles(searchPattern, SearchOption.TopDirectoryOnly).Select(f => f.FullName);

        }
    }
}
