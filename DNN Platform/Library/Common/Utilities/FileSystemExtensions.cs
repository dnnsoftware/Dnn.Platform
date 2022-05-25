// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.IO;
    using System.IO.Compression;

    public static class FileSystemExtensions
    {
        public static void CheckZipEntry(this ZipArchiveEntry input)
        {
            var fullName = input.FullName.Replace('\\', '/');
            if (fullName.StartsWith("..") || fullName.Contains("/../"))
            {
                throw new Exception("Illegal Zip File");
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
    }
}
