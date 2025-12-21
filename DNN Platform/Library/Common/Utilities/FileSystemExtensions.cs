// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    using DotNetNuke.Internal.SourceGenerators;

    using ICSharpCode.SharpZipLib.Zip;

    /// <summary>Extension methods for types related to the file system (e.g. <see cref="ZipArchiveEntry"/> and <see cref="Stream"/>).</summary>
    public static partial class FileSystemExtensions
    {
        public static void CheckZipEntry(this ZipArchiveEntry input)
        {
            var fullName = input.FullName.Replace('\\', '/');
            if (fullName.StartsWith("..") || fullName.Contains("/../"))
            {
                throw new IllegalZipFileException("Illegal Zip File");
            }
        }

        public static string ReadTextFile(this ZipArchiveEntry input)
        {
            var text = string.Empty;
            using (var reader = new StreamReader(input.Open()))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }

        public static void CopyToStream(this Stream strIn, Stream strOut, int bufferSize)
        {
            var buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = strIn.Read(buffer, 0, buffer.Length)) > 0)
            {
                strOut.Write(buffer, 0, bytesRead);
            }
        }

        public static IEnumerable<ZipArchiveEntry> FileEntries(this ZipArchive zip)
        {
            foreach (var entry in zip.Entries)
            {
                entry.CheckZipEntry();
                if (!string.IsNullOrEmpty(entry.Name))
                {
                    yield return entry;
                }
            }
        }

        /// <summary>Check whether the <paramref name="input"/> is invalid.</summary>
        /// <param name="input">The entry to check.</param>
        /// <exception cref="Exception">Illegal zip file.</exception>
        [DnnDeprecated(9, 11, 0, "Replaced with .NET compression types.")]
        public static partial void CheckZipEntry(this ZipEntry input)
        {
            var fullName = input.Name.Replace('\\', '/');
            if (fullName.StartsWith("..") || fullName.Contains("/../"))
            {
                throw new IllegalZipFileException("Illegal Zip File");
            }
        }
    }
}
