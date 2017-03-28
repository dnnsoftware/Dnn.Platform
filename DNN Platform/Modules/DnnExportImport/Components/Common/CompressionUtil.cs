using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Dnn.ExportImport.Components.Common
{
    public static class CompressionUtil
    {
        public static void ZipFolder(string folderPath, string archivePath)
        {
            if (File.Exists(archivePath))
                File.Delete(archivePath);
            ZipFile.CreateFromDirectory(folderPath, archivePath, CompressionLevel.Optimal, false);
        }

        public static void UnZipArchive(string archivePath, string extractFolder, bool overwrite = true)
        {
            UnZipArchiveExcept(archivePath, extractFolder, overwrite);
        }

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

        public static void AddFilesToArchive(IEnumerable<string> files, string archivePath, int folderOffset,
            string folder = null)
        {
            using (var archive = OpenCreate(archivePath))
            {
                foreach (var file in files)
                {
                    AddFileToArchive(archive, file, folderOffset, folder);
                }
            }
        }

        public static void AddFileToArchive(string file, string archivePath, int folderOffset, string folder = null)
        {
            using (var archive = OpenCreate(archivePath))
            {
                AddFileToArchive(archive, file, folderOffset, folder);
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
            if (string.IsNullOrEmpty(folder))
            {
                archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
            }
            else
            {
                using (var fs = File.OpenRead(file))
                {
                    // this is custom code that copies a file location's data to a Stream (msFileBody); this is the Stream needing compression that prompted my initial question 
                    var zipArchiveEntry = archive.CreateEntry(Path.Combine(folder, entryName), CompressionLevel.Optimal);
                    using (var zipEntryStream = zipArchiveEntry.Open())
                    {
                        //Copy the attachment stream to the zip entry stream
                        fs.CopyTo(zipEntryStream);
                    }
                }
            }
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