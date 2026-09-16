// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Helper methods for reading version information from assemblies.
    /// </summary>
    /// <remarks>
    /// This class provides utilities to obtain either the file version
    /// (derived from the assembly file's FileVersionInfo) or the assembly
    /// version (derived from the assembly manifest).
    /// </remarks>
    public static class AssemblyVersions
    {
        /// <summary>
        /// Gets the file version for the assembly located at <paramref name="assemblyPath"/>.
        /// </summary>
        /// <param name="assemblyPath">The full path to the assembly file (.dll or .exe).</param>
        /// <returns>
        /// A <see cref="Version"/> representing the file version. If the textual
        /// <see cref="FileVersionInfo.FileVersion"/> parses as a valid version it is returned;
        /// otherwise a <see cref="Version"/> is constructed from the numeric major/minor/build/private parts.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="assemblyPath"/> is <c>null</c> or empty.</exception>
        /// <exception cref="FileNotFoundException">The file specified by <paramref name="assemblyPath"/> does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">The caller does not have required permissions to access the file.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <remarks>
        /// This method uses <see cref="FileVersionInfo.GetVersionInfo(string)"/> to obtain
        /// the file version string. If that string cannot be parsed into a <see cref="Version"/>,
        /// the numeric version parts exposed by <see cref="FileVersionInfo"/> are used to
        /// construct a <see cref="Version"/> instance.
        /// </remarks>
        public static Version GetAssemblyFileVersion(string assemblyPath)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);
            var fileVersion = versionInfo.FileVersion;
            return Version.TryParse(fileVersion, out var version) ? version : new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
        }

        /// <summary>
        /// Asynchronously extracts an assembly from a <see cref="ZipArchiveEntry"/> to a temporary location
        /// and returns the assembly version found in the assembly manifest.
        /// </summary>
        /// <param name="assemblyEntry">The zip archive entry that contains the assembly bytes.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the copy operation to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is the <see cref="Version"/>
        /// obtained from <see cref="AssemblyName.GetAssemblyName(string)"/> for the extracted temporary file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="assemblyEntry"/> is <c>null</c>.</exception>
        /// <exception cref="IOException">An I/O error occurs while creating or copying the temporary file.</exception>
        /// <exception cref="UnauthorizedAccessException">Insufficient permissions to create the temporary file or directory.</exception>
        /// <exception cref="PathTooLongException">The generated temporary path exceeds system limits.</exception>
        /// <exception cref="BadImageFormatException">The extracted file is not a valid assembly.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled via <paramref name="cancellationToken"/> during the copy.</exception>
        /// <remarks>
        /// - The method creates a uniquely named temporary directory under <c>Globals.InstallMapPath\Temp</c> and writes
        ///   the archive entry contents to a temporary .dll file before reading its assembly manifest.
        /// - The temporary directory is removed in a <c>finally</c> block to ensure cleanup even if an exception occurs.
        /// - The returned version is the assembly version (from the assembly manifest), which is different from the file version
        ///   returned by <see cref="GetAssemblyFileVersion(string)"/>.
        /// </remarks>
        internal static async Task<Version> GetAssemblyFileVersion(ZipArchiveEntry assemblyEntry, CancellationToken cancellationToken = default)
        {
            var tempPath = Path.Combine(Globals.InstallMapPath, "Temp", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            var tempAssemblyPath = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".dll");

            Directory.CreateDirectory(tempPath);
            try
            {
                using (var tempAssemblyFileStream = File.Create(tempAssemblyPath))
                using (var assemblyStream = assemblyEntry.Open())
                {
                    const int DefaultBufferSize = 81920;
                    await assemblyStream.CopyToAsync(
                        tempAssemblyFileStream,
                        DefaultBufferSize,
                        cancellationToken);
                }

                return GetAssemblyFileVersion(tempAssemblyPath);
            }
            finally
            {
                Directory.Delete(tempPath, true);
            }
        }
    }
}
