// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;

    /// <summary>
    /// Internal class to check file security.
    /// </summary>
    public class FileSecurityController : ServiceLocator<IFileSecurityController, FileSecurityController>, IFileSecurityController
    {
        private const int BufferSize = 4096;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSecurityController));

        public bool Validate(string fileName, Stream fileContent)
        {
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("fileContent", fileContent);

            var extension = Path.GetExtension(fileName);
            var checker = this.GetSecurityChecker(extension?.ToLowerInvariant().TrimStart('.'));

            // when there is no specfic file check for the file type, then treat it as validated.
            if (checker == null)
            {
                return true;
            }

            // use copy of the stream as we can't make sure how the check process the stream.
            using (var copyStream = this.CopyStream(fileContent))
            {
                return checker.Validate(copyStream);
            }
        }

        protected override Func<IFileSecurityController> GetFactory()
        {
            return () => new FileSecurityController();
        }

        private IFileSecurityChecker GetSecurityChecker(string extension)
        {
            var listEntry = new ListController().GetListEntryInfo("FileSecurityChecker", extension);
            if (listEntry != null && !string.IsNullOrEmpty(listEntry.Text))
            {
                try
                {
                    var cacheKey = $"FileSecurityChecker_{extension}";
                    return Reflection.CreateObject(listEntry.Text, cacheKey) as IFileSecurityChecker;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Create File Security Checker for '{extension}' failed.", ex);
                }
            }

            return null;
        }

        private Stream CopyStream(Stream stream)
        {
            var folderPath = ((FileManager)FileManager.Instance).GetHostMapPath();
            string filePath;
            do
            {
                filePath = Path.Combine(folderPath, Path.GetRandomFileName()) + ".resx";
            }
            while (File.Exists(filePath));

            var fileStream = ((FileManager)FileManager.Instance).GetAutoDeleteFileStream(filePath);

            var array = new byte[BufferSize];

            int bytesRead;
            while ((bytesRead = stream.Read(array, 0, BufferSize)) > 0)
            {
                fileStream.Write(array, 0, bytesRead);
            }

            stream.Position = 0;
            fileStream.Position = 0;

            return fileStream;
        }
    }
}
