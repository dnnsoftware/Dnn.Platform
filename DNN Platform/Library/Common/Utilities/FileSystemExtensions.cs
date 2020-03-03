using System;

namespace DotNetNuke.Common.Utilities
{
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
