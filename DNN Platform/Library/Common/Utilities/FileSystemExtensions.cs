// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;

    public static class FileSystemExtensions
    {
        public static void CheckZipEntry(this ICSharpCode.SharpZipLib.Zip.ZipEntry input)
        {
            var fullName = input.Name.Replace('\\', '/');
            if (fullName.StartsWith("..") || fullName.Contains("/../"))
            {
                throw new Exception("Illegal Zip File");
            }
        }
    }
}
