// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.IO.Compression;

    public static class FileSystemExtensions
    {
        [Obsolete("Deprecated in 9.11.0. Scheduled for removal in v11.0.0, use overload taking System.IO.Compression.ZipArchiveEntry.")]
        public static void CheckZipEntry(this ICSharpCode.SharpZipLib.Zip.ZipEntry input)
        {
            CheckZipEntryName(input.Name);
        }

        public static void CheckZipEntry(this ZipArchiveEntry input)
        {
            CheckZipEntryName(input.Name);
        }

        private static void CheckZipEntryName(string inputName)
        {
            var fullName = inputName.Replace('\\', '/');
            if (fullName.StartsWith("..") || fullName.Contains("/../"))
            {
                throw new Exception("Illegal Zip File");
            }
        }
    }
}
