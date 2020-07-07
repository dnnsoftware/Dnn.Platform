// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.IO.MemoryMappedFiles;
    using System.Linq;
    using System.Text;

    public static class CompressionUtil
    {
        /// <summary>
        /// Compress a full folder.
        /// </summary>
        /// <param name="folderPath">Full path of folder to compress.</param>
        /// <param name="archivePath">Full path of the archived file.</param>
        public static void ZipFolder(string folderPath, string archivePath)
        {
            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            ZipFile.CreateFromDirectory(folderPath, archivePath, CompressionLevel.Fastest, false);
        }

        /// <summary>
        /// Unzip compressed file to a folder.
        /// </summary>
        /// <param name="archivePath">Full path to archive with name.</param>
        /// <param name="extractFolder">Full path to the target folder.</param>
        /// <param name="overwrite">Overwrites the files on target if true.</param>
        public static void UnZipArchive(string archivePath, string extractFolder, bool overwrite = true)
        {
            UnZipArchiveExcept(archivePath, extractFolder, overwrite);
        }

        /// <summary>
        /// Unzip compressed file to a folder.
        /// </summary>
        /// <param name="archivePath">Full path to archive with name.</param>
        /// <param name="extractFolder">Full path to the target folder.</param>
        /// <param name="overwrite">Overwrites the files on target if true.</param>
        /// <param name="exceptionList">List of files to exlude from extraction.</param>
        /// <param name="deleteFromSoure">Delete the files from the archive after extraction.</param>
        public static void UnZipArchiveExcept(string archivePath, string extractFolder, bool overwrite = true,
            IEnumerable<string> exceptionList = null, bool deleteFromSoure = false)
        {
            if (!File.Exists(archivePath))
            {
                return;
            }

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
                    {
                        Directory.CreateDirectory(path);
                    }

                    if (!File.Exists(Path.Combine(extractFolder, entry.FullName)) || overwrite)
                    {
                        entry.ExtractToFile(Path.Combine(extractFolder, entry.FullName), overwrite);
                    }

                    if (deleteFromSoure)
                    {
                        entry.Delete();
                    }
                }
            }
        }

        /// <summary>
        /// Unzip a single file from an archive.
        /// </summary>
        /// <param name="fileName">Name of the file to extract. This name should match the entry name in the archive. i.e. it should include complete folder structure inside the archive.</param>
        /// <param name="archivePath">Full path to archive with name.</param>
        /// <param name="extractFolder">Full path to the target folder.</param>
        /// <param name="overwrite">Overwrites the file on target if true.</param>
        /// <param name="deleteFromSoure">Delete the file from the archive after extraction.</param>
        public static void UnZipFileFromArchive(string fileName, string archivePath, string extractFolder,
            bool overwrite = true, bool deleteFromSoure = false)
        {
            if (!File.Exists(archivePath))
            {
                return;
            }

            using (var archive = OpenCreate(archivePath))
            {
                var fileUnzipFullName = Path.Combine(extractFolder, fileName);
                if (File.Exists(fileUnzipFullName) && !overwrite)
                {
                    return;
                }

                var fileEntry = archive.GetEntry(fileName);
                if (!File.Exists(Path.Combine(extractFolder, fileEntry.FullName)) || overwrite)
                {
                    fileEntry?.ExtractToFile(Path.Combine(extractFolder, fileName), overwrite);
                }

                if (deleteFromSoure)
                {
                    fileEntry?.Delete();
                }
            }
        }

        /// <summary>
        /// Add files to an archive. If no archive exists, new one is created.
        /// </summary>
        /// <param name="archive">Source archive to write the files to.</param>
        /// <param name="files">List containing path of files to add to archive.</param>
        /// <param name="folderOffset">Starting index(Index in file url) of the root folder in archive based on what the folder structure starts in archive.
        /// e.g. if file url is c:\\dnn\files\archived\foldername\1\file.jpg and we want to add all files in foldername folder
        /// then the folder offset would be starting index of foldername.</param>
        /// <param name="folder">Additional root folder to be added into archive.</param>
        public static void AddFilesToArchive(ZipArchive archive, IEnumerable<string> files, int folderOffset,
            string folder = null)
        {
            var enumerable = files as IList<string> ?? files.ToList();
            if (!enumerable.Any())
            {
                return;
            }

            foreach (var file in enumerable.Where(File.Exists))
            {
                AddFileToArchive(archive, file, folderOffset, folder);
            }
        }

        /// <summary>
        /// Add single file to an archive. If no archive exists, new one is created.
        /// </summary>
        /// <param name="file">Full path of file to add.</param>
        /// <param name="archivePath">Full path of archive file.</param>
        /// <param name="folderOffset">Starting index(Index in file url) of the root folder in archive based on what the folder structure starts in archive.
        /// e.g. if file url is c:\\dnn\files\archived\foldername\1\file.jpg and we want to add all files in foldername folder
        /// then the folder offset would be starting index of foldername.</param>
        /// <param name="folder">Additional root folder to be added into archive.</param>
        /// <returns></returns>
        public static bool AddFileToArchive(string file, string archivePath, int folderOffset, string folder = null)
        {
            using (var archive = OpenCreate(archivePath))
            {
                if (File.Exists(file))
                {
                    return AddFileToArchive(archive, file, folderOffset, folder);
                }
            }

            return false;
        }

        public static bool AddFileToArchive(ZipArchive archive, string file, int folderOffset, string folder = null)
        {
            var entryName = file.Substring(folderOffset); // Makes the name in zip based on the folder
            ZipArchiveEntry existingEntry;

            // Deletes if the entry already exists in archive.
            if ((existingEntry = archive.GetEntry(entryName)) != null)
            {
                existingEntry.Delete();
            }

            var fileInfo = new FileInfo(file);
            if (fileInfo.Length < 1610612736)
            {
                archive.CreateEntryFromFile(
                    file,
                    string.IsNullOrEmpty(folder) ? entryName : Path.Combine(folder, entryName), CompressionLevel.Fastest);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Open the archive file for read and write.
        /// </summary>
        /// <param name="archiveFileName"></param>
        /// <returns></returns>
        public static ZipArchive OpenCreate(string archiveFileName)
        {
            return File.Exists(archiveFileName)
                ? ZipFile.Open(archiveFileName, ZipArchiveMode.Update, Encoding.UTF8)
                : new ZipArchive(new FileStream(archiveFileName, FileMode.Create), ZipArchiveMode.Update);
        }
    }
}
