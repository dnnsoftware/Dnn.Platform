using System;
using System.IO;
using DotNetNuke.Common.Utilities;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;

namespace Dnn.ExportImport.Components.Common
{
    public static class CompressionUtil
    {
        public static void ZipFolder(string folderPath, string archivePath, string folderName)
        {
            using (var zipStream = new ZipOutputStream(File.Create(archivePath)))
            {
                // This setting will strip the leading part of the folder path in the entries, to
                // make the entries relative to the starting folder.
                // To include the full path for each entry up to the drive root, assign folderOffset = 0.
                var folderOffset = folderPath.IndexOf(folderName, StringComparison.Ordinal) + folderName.Length +
                                   (folderName.EndsWith("\\") ? 0 : 1);
                zipStream.SetLevel(6);
                CompressFolder(folderPath, zipStream, folderOffset);
                zipStream.Close();
            }
        }

        public static void UnZipArchive(string archivePath, string extractFolder)
        {
            FileSystemUtils.UnzipResources(
                new ZipInputStream(new FileStream(archivePath, FileMode.Open, FileAccess.Read)), extractFolder);
        }

        #region Private Methods

        // Recurses down the folder structure
        //
        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            var files = Directory.GetFiles(path);

            foreach (var filename in files)
            {
                AddToZip(ref zipStream, filename, folderOffset);
            }
            var folders = Directory.GetDirectories(path);
            foreach (var folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }

        private static void AddToZip(ref ZipOutputStream zipStream, string fileName, int folderOffset)
        {
            FileStream fs = null;
            try
            {
                var entryName = fileName.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction

                //Open File Stream
                var crc = new Crc32();
                fs = File.OpenRead(fileName.Replace("/", "\\"));

                //Read file into byte array buffer
                var buffer = new byte[fs.Length];

                fs.Read(buffer, 0, buffer.Length);

                //Create Zip Entry
                var entry = new ZipEntry(entryName)
                {
                    DateTime = DateTime.Now,
                    Size = fs.Length
                };
                fs.Close();
                crc.Reset();
                crc.Update(buffer);
                entry.Crc = crc.Value;

                //Compress file and add to Zip file
                zipStream.PutNextEntry(entry);
                zipStream.Write(buffer, 0, buffer.Length);
                zipStream.CloseEntry();
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        #endregion
    }
}
