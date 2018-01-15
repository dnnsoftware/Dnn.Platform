#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Services.FileSystem.Internal
{
    /// <summary>
    /// Internal class to check file security.
    /// </summary>
    public class FileSecurityController : ServiceLocator<IFileSecurityController, FileSecurityController>, IFileSecurityController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSecurityController));

        private const int BufferSize = 4096;

        protected override Func<IFileSecurityController> GetFactory()
        {
            return () => new FileSecurityController();
        }

        public bool Validate(string fileName, Stream fileContent)
        {
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("fileContent", fileContent);

            var extension = Path.GetExtension(fileName);
            var checker = GetSecurityChecker(extension?.ToLowerInvariant().TrimStart('.'));

            //when there is no specfic file check for the file type, then treat it as validated.
            if (checker == null)
            {
                return true;
            }

            //use copy of the stream as we can't make sure how the check process the stream.
            using (var copyStream = CopyStream(fileContent))
            {
                return checker.Validate(copyStream);
            }
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
            } while (File.Exists(filePath));

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
