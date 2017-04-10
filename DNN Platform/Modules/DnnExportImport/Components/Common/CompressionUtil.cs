#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Dnn.ExportImport.Components.Common
{
    public static class CompressionUtil
    {
        /// <summary>
        /// Compress a full folder.
        /// </summary>
        /// <param name="folderPath">Full path of folder to compress</param>
        /// <param name="archivePath">Full path of the archived file</param>
        public static void ZipFolder(string folderPath, string archivePath)
        {
            if (File.Exists(archivePath))
                File.Delete(archivePath);
            ZipFile.CreateFromDirectory(folderPath, archivePath, CompressionLevel.Optimal, false);
        }

        /// <summary>
        /// Unzip compressed file to a folder.
        /// </summary>
        /// <param name="archivePath">Full path to archive with name</param>
        /// <param name="extractFolder">Full path to the target folder</param>
        /// <param name="overwrite">Overwrites the files on target if true.</param>
        public static void UnZipArchive(string archivePath, string extractFolder, bool overwrite = true)
        {
            UnZipArchiveExcept(archivePath, extractFolder, overwrite);
        }

        /// <summary>
        /// Unzip compressed file to a folder.
        /// </summary>
        /// <param name="archivePath">Full path to archive with name</param>
        /// <param name="extractFolder">Full path to the target folder</param>
        /// <param name="overwrite">Overwrites the files on target if true.</param>
        /// <param name="exceptionList">List of files to exlude from extraction.</param>
        /// <param name="deleteFromSoure">Delete the files from the archive after extraction</param>
        public static void UnZipArchiveExcept(string archivePath, string extractFolder, bool overwrite = true,
            IEnumerable<string> exceptionList = null, bool deleteFromSoure = false)
        {
            using (var archive = OpenCreate(archivePath))
            {
                foreach (
                    var entry in
                        archive.Entries.Where(
                            entry =>
                                ((exceptionList != null && !exceptionList.Contains(entry.FullName)) ||
                                 exceptionList == null) &&
                                !entry.FullName.EndsWith("\\") && !entry.FullName.EndsWith("/") && entry.Length > 0))
                {
                    var path = Path.GetDirectoryName(Path.Combine(extractFolder, entry.FullName));
                    if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    if (!File.Exists(Path.Combine(extractFolder, entry.FullName)) || overwrite)
                        entry.ExtractToFile(Path.Combine(extractFolder, entry.FullName), overwrite);
                    if (deleteFromSoure)
                        entry.Delete();
                }
            }
        }

        /// <summary>
        /// Unzip a single file from an archive.
        /// </summary>
        /// <param name="fileName">Name of the file to extract. This name should match the entry name in the archive. i.e. it should include complete folder structure inside the archive.</param>
        /// <param name="archivePath">Full path to archive with name</param>
        /// <param name="extractFolder">Full path to the target folder</param>
        /// <param name="overwrite">Overwrites the file on target if true.</param>
        /// <param name="deleteFromSoure">Delete the file from the archive after extraction</param>
        public static void UnZipFileFromArchive(string fileName, string archivePath, string extractFolder,
            bool overwrite = true, bool deleteFromSoure = false)
        {
            using (var archive = OpenCreate(archivePath))
            {
                var fileUnzipFullName = Path.Combine(extractFolder, fileName);
                if (File.Exists(fileUnzipFullName) && !overwrite)
                    return;

                var fileEntry = archive.GetEntry(fileName);
                if (!File.Exists(Path.Combine(extractFolder, fileEntry.FullName)) || overwrite)
                    fileEntry?.ExtractToFile(Path.Combine(extractFolder, fileName), overwrite);
                if (deleteFromSoure)
                    fileEntry?.Delete();
            }
        }

        /// <summary>
        /// Add files to an archive. If no archive exists, new one is created.
        /// </summary>
        /// <param name="files">List containing path of files to add to archive.</param>
        /// <param name="archivePath">Full path of archive file</param>
        /// <param name="folderOffset">Starting index(Index in file url) of the root folder in archive based on what the folder structure starts in archive.
        /// e.g. if file url is c:\\dnn\files\archived\foldername\1\file.jpg and we want to add all files in foldername folder 
        /// then the folder offset would be starting index of foldername</param>
        /// <param name="folder">Additional root folder to be added into archive.</param>
        public static void AddFilesToArchive(IEnumerable<string> files, string archivePath, int folderOffset,
            string folder = null)
        {
            var enumerable = files as IList<string> ?? files.ToList();
            if (!enumerable.Any()) return;
            ZipArchive archive = null;
            using (archive = OpenCreate(archivePath))
            {
                long currentTotalSize = 0;
                foreach (var file in enumerable.Where(File.Exists))
                {
                    if (currentTotalSize >= Constants.MaxZipFilesMemory)
                    {
                        currentTotalSize = 0;
                        archive.Dispose();
                        archive = OpenCreate(archivePath);
                    }
                    var fileInfo = new FileInfo(file);
                    currentTotalSize += fileInfo.Length;
                    AddFileToArchive(archive, file, folderOffset, folder);
                }
            }
        }

        /// <summary>
        /// Add single file to an archive. If no archive exists, new one is created.
        /// </summary>
        /// <param name="file">Full path of file to add</param>
        /// <param name="archivePath">Full path of archive file</param>
        /// <param name="folderOffset">Starting index(Index in file url) of the root folder in archive based on what the folder structure starts in archive.
        /// e.g. if file url is c:\\dnn\files\archived\foldername\1\file.jpg and we want to add all files in foldername folder 
        /// then the folder offset would be starting index of foldername</param>
        /// <param name="folder">Additional root folder to be added into archive.</param>
        public static void AddFileToArchive(string file, string archivePath, int folderOffset, string folder = null)
        {
            using (var archive = OpenCreate(archivePath))
            {
                if (File.Exists(file))
                {
                    AddFileToArchive(archive, file, folderOffset, folder);
                }
            }
        }

        #region Private Methods

        private static void AddFileToArchive(ZipArchive archive, string file, int folderOffset, string folder = null)
        {
            var entryName = file.Substring(folderOffset); // Makes the name in zip based on the folder
            ZipArchiveEntry existingEntry = null;
            //Deletes if the entry already exists in archive.
            if ((existingEntry = archive.GetEntry(entryName)) != null)
            {
                existingEntry.Delete();
            }
            archive.CreateEntryFromFile(file,
                string.IsNullOrEmpty(folder) ? entryName : Path.Combine(folder, entryName), CompressionLevel.Optimal);
        }

        /// <summary>
        /// Open the archive file for read and write.
        /// </summary>
        /// <param name="archiveFileName"></param>
        /// <returns></returns>
        //TODO: This will need review. We might need to seperate methods for opening in read and write mode seperately since the for read mode, whole archive is loaded in memory and is persisted.
        private static ZipArchive OpenCreate(string archiveFileName)
        {
            return File.Exists(archiveFileName)
                ? ZipFile.Open(archiveFileName, ZipArchiveMode.Update, Encoding.UTF8)
                : new ZipArchive(new FileStream(archiveFileName, FileMode.Create), ZipArchiveMode.Update);
        }

        #endregion
    }
}