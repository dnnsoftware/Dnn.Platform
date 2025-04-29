// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Instrumentation;
using DotNetNuke.Internal.SourceGenerators;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.DependencyInjection;

using Directory = SchwabenCode.QuickIO.QuickIODirectory;
using DirectoryInfo = SchwabenCode.QuickIO.QuickIODirectoryInfo;
using File = SchwabenCode.QuickIO.QuickIOFile;

/// <summary>
/// File System utilities.
/// </summary>
public partial class FileSystemUtils
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSystemUtils));

    /// <summary>
    /// Adds a File to a Zip File.
    /// </summary>
    /// <param name="zipFile">The Zip File to add to.</param>
    /// <param name="filePath">The path to the file to add.</param>
    /// <param name="fileName">The name of the file to add.</param>
    /// <param name="folder">The name of the folder to use for the zip entry.</param>
    public static void AddToZip(ref ZipArchive zipFile, string filePath, string fileName, string folder)
    {
        FileStream fs = null;
        try
        {
            // Open File Stream
            fs = File.OpenRead(FixPath(filePath));

            // Read file into byte array buffer
            var buffer = new byte[fs.Length];

            var len = fs.Read(buffer, 0, buffer.Length);
            if (len != fs.Length)
            {
                Logger.ErrorFormat(
                    "Reading from " +
                    filePath +
                    " didn't read all data in buffer. " +
                    "Requested to read {0} bytes, but was read {1} bytes",
                    fs.Length,
                    len);
            }

            // Create Zip Entry
            zipFile.CreateEntryFromFile(FixPath(filePath), Path.Combine(folder, fileName));
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

    /// <summary>Tries to copy a file in the file system.</summary>
    /// <param name="sourceFileName">The name of the source file.</param>
    /// <param name="destFileName">The name of the destination file.</param>
    public static void CopyFile(string sourceFileName, string destFileName)
    {
        if (File.Exists(destFileName))
        {
            File.SetAttributes(destFileName, FileAttributes.Normal);
        }

        File.Copy(sourceFileName, destFileName, true);
    }

    /// <summary>
    /// Deletes file in areas with a high degree of concurrent file access (i.e. caching, logging).
    /// This solves file concurrency issues under heavy load.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="waitInMilliseconds">The number of milliseconds to wait.</param>
    /// <param name="maxAttempts">The maximum number of attempts.</param>
    /// <returns>Whether the file is deleted.</returns>
    public static bool DeleteFileWithWait(string fileName, short waitInMilliseconds, short maxAttempts)
    {
        fileName = FixPath(fileName);
        if (!File.Exists(fileName))
        {
            return true;
        }

        bool fileDeleted = false;
        int i = 0;
        while (fileDeleted != true)
        {
            if (i > maxAttempts)
            {
                break;
            }

            i = i + 1;
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                fileDeleted = true; // we don't care if it didn't exist...the operation didn't fail, that's what we care about
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                fileDeleted = false;
            }

            if (fileDeleted == false)
            {
                Thread.Sleep(waitInMilliseconds);
            }
        }

        return fileDeleted;
    }

    /// <summary>Tries to delete a file from the file system.</summary>
    /// <param name="fileName">The name of the file.</param>
    public static void DeleteFile(string fileName)
    {
        fileName = FixPath(fileName);
        if (File.Exists(fileName))
        {
            File.SetAttributes(fileName, FileAttributes.Normal);
            File.Delete(fileName);
        }
    }

    /// <summary>
    /// Reads a file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The string content of the file.</returns>
    public static string ReadFile(string filePath)
    {
        StreamReader reader = null;
        string fileContent = string.Empty;
        try
        {
            reader = File.OpenText(filePath);
            fileContent = reader.ReadToEnd();
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }

        return fileContent;
    }

    /// <summary>
    /// Unzips a resources zip file.
    /// </summary>
    /// <param name="zipStream">The zip archive stream.</param>
    /// <param name="destPath">The destination path to extract to.</param>
    public static void UnzipResources(ZipArchive zipStream, string destPath)
    {
        try
        {
            foreach (var zipEntry in zipStream.FileEntries())
            {
                HtmlUtils.WriteKeepAlive();
                var localFileName = zipEntry.FullName;
                var relativeDir = Path.GetDirectoryName(zipEntry.FullName);
                if (!string.IsNullOrEmpty(relativeDir) && (!Directory.Exists(Path.Combine(destPath, relativeDir))))
                {
                    Directory.Create(Path.Combine(destPath, relativeDir), true);
                }

                if (!string.IsNullOrEmpty(localFileName))
                {
                    var fileNamePath = FixPath(Path.Combine(destPath, localFileName));
                    try
                    {
                        if (File.Exists(fileNamePath))
                        {
                            File.SetAttributes(fileNamePath, FileAttributes.Normal);
                            File.Delete(fileNamePath);
                        }

                        FileStream objFileStream = null;
                        try
                        {
                            File.Create(fileNamePath);
                            objFileStream = File.Open(fileNamePath);
                            zipEntry.Open().CopyToStream(objFileStream, 25000);
                        }
                        finally
                        {
                            if (objFileStream != null)
                            {
                                objFileStream.Close();
                                objFileStream.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
        }
        finally
        {
            if (zipStream != null)
            {
                zipStream.Dispose();
            }
        }
    }

    /// <summary>
    /// Deletes the files specified.
    /// </summary>
    /// <param name="arrPaths">An array of the file paths for the files to delete..</param>
    /// <returns>An empty string if succeeded or a list of errors in case of failures.</returns>
    public static string DeleteFiles(Array arrPaths)
    {
        var applicationStatusInfo = Globals.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
        var strExceptions = string.Empty;
        for (var i = 0; i < arrPaths.Length; i++)
        {
            var strPath = (arrPaths.GetValue(i) ?? string.Empty).ToString();
            var pos = strPath.IndexOf("'", StringComparison.Ordinal);
            if (pos != -1)
            {
                // the (') represents a comment to the end of the line
                strPath = strPath.Substring(0, pos);
            }

            strPath = FixPath(strPath).TrimStart('\\');
            if (!string.IsNullOrEmpty(strPath))
            {
                strPath = Path.Combine(applicationStatusInfo.ApplicationMapPath, strPath);
                if (strPath.EndsWith("\\") && Directory.Exists(strPath))
                {
                    var directoryInfo = new System.IO.DirectoryInfo(strPath);
                    var applicationPath = applicationStatusInfo.ApplicationMapPath + "\\";
                    if (directoryInfo.FullName.StartsWith(applicationPath, StringComparison.InvariantCultureIgnoreCase)
                        && !directoryInfo.FullName.Equals(applicationPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            Globals.DeleteFolderRecursive(strPath);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            strExceptions += $"Processing folder ({strPath}) Error: {ex.Message}{Environment.NewLine}";
                        }
                    }
                }
                else
                {
                    if (File.Exists(strPath))
                    {
                        try
                        {
                            File.SetAttributes(strPath, FileAttributes.Normal);
                            File.Delete(strPath);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            strExceptions += $"Processing file ({strPath}) Error: {ex.Message}{Environment.NewLine}";
                        }
                    }
                }
            }
        }

        return strExceptions;
    }

    /// <summary>
    /// Deletes files that match a filter recursively within folders.
    /// </summary>
    /// <param name="strRoot">The root path to filter from.</param>
    /// <param name="filter">The filter to select the files to delete.</param>
    public static void DeleteFilesRecursive(string strRoot, string filter)
    {
        if (!string.IsNullOrEmpty(strRoot))
        {
            strRoot = FixPath(strRoot);
            if (Directory.Exists(strRoot))
            {
                foreach (string strFolder in Directory.EnumerateDirectoryPaths(strRoot))
                {
                    var directory = new DirectoryInfo(strFolder);
                    if ((directory.Attributes & FileAttributes.Hidden) == 0 && (directory.Attributes & FileAttributes.System) == 0)
                    {
                        DeleteFilesRecursive(strFolder, filter);
                    }
                }

                foreach (string strFile in Directory.EnumerateFilePaths(strRoot).Where(f => f.Contains(filter)))
                {
                    try
                    {
                        DeleteFile(strFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Deletes a folder and all it's child files and folders.
    /// </summary>
    /// <param name="strRoot">The root path to delete from.</param>
    public static void DeleteFolderRecursive(string strRoot)
    {
        strRoot = FixPath(strRoot);
        if (string.IsNullOrEmpty(strRoot) || !Directory.Exists(strRoot))
        {
            Logger.Info(strRoot + " does not exist. ");
            return;
        }

        foreach (string strFolder in Directory.EnumerateDirectoryPaths(strRoot))
        {
            DeleteFolderRecursive(strFolder);
        }

        foreach (string strFile in Directory.EnumerateFilePaths(strRoot))
        {
            try
            {
                DeleteFile(strFile);
            }
            catch (Exception ex)
            {
                Logger.Info(strRoot + " does not exist.");
                Logger.Error(ex);
            }
        }

        try
        {
            Directory.SetAttributes(strRoot, FileAttributes.Normal);
            Directory.Delete(strRoot);
        }
        catch (Exception ex)
        {
            Logger.Info(strRoot + " does not exist.");
            Logger.Error(ex);
        }
    }

    /// <summary>Deletes all empty folders beneath a given root folder and the root folder itself as well if empty.</summary>
    /// <param name="path">The root folder path.</param>
    public static void DeleteEmptyFoldersRecursive(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            Logger.Info(path + " does not exist.");
            return;
        }

        // first take care of folders
        foreach (var folder in Directory.EnumerateDirectoryPaths(path))
        {
            DeleteEmptyFoldersRecursive(folder);
        }

        // if any files or folders left, return
        if (Directory.EnumerateFileSystemEntries(path).Any())
        {
            return;
        }

        try
        {
            // delete this empty folder
            Directory.SetAttributes(path, FileAttributes.Normal);
            Directory.Delete(path);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }

    /// <summary>
    /// Fixes the path in case the path separator is not windows style.
    /// </summary>
    /// <param name="input">The path to fix.</param>
    /// <returns>A valid Windows path.</returns>
    public static string FixPath(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input.Trim().Replace("/", "\\");
    }

    /// <summary>
    /// Adds a file to a zip.
    /// </summary>
    /// <param name="zipFile">The zip file stream to add to.</param>
    /// <param name="filePath">The path to the file to add.</param>
    /// <param name="fileName">Name of the file to use in the zip entry.</param>
    /// <param name="folder">The name of the folder to use in the zip entry..</param>
    [DnnDeprecated(9, 11, 0, "Replaced with .NET compression types.")]
    public static partial void AddToZip(ref ZipOutputStream zipFile, string filePath, string fileName, string folder)
    {
        FileStream fs = null;
        try
        {
            // Open File Stream
            fs = File.OpenRead(FixPath(filePath));

            // Read file into byte array buffer
            var buffer = new byte[fs.Length];

            var len = fs.Read(buffer, 0, buffer.Length);
            if (len != fs.Length)
            {
                Logger.ErrorFormat(
                    "Reading from " +
                    filePath +
                    " didn't read all data in buffer. " +
                    "Requested to read {0} bytes, but was read {1} bytes",
                    fs.Length,
                    len);
            }

            // Create Zip Entry
            var entry = new ZipEntry(Path.Combine(folder, fileName));
            entry.DateTime = DateTime.Now;
            entry.Size = fs.Length;
            fs.Close();

            // Compress file and add to Zip file
            zipFile.PutNextEntry(entry);
            zipFile.Write(buffer, 0, buffer.Length);
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

    /// <summary>
    /// Unzips a resources zip file.
    /// </summary>
    /// <param name="zipStream">The zip stream.</param>
    /// <param name="destPath">The destination path to extract to.</param>
    [DnnDeprecated(9, 11, 0, "Replaced with .NET compression types.")]
    public static partial void UnzipResources(ZipInputStream zipStream, string destPath)
    {
        try
        {
            var zipEntry = zipStream.GetNextEntry();
            while (zipEntry != null)
            {
                zipEntry.CheckZipEntry();
                HtmlUtils.WriteKeepAlive();
                var localFileName = zipEntry.Name;
                var relativeDir = Path.GetDirectoryName(zipEntry.Name);
                if (!string.IsNullOrEmpty(relativeDir) && (!Directory.Exists(Path.Combine(destPath, relativeDir))))
                {
                    Directory.Create(Path.Combine(destPath, relativeDir), true);
                }

                if (!zipEntry.IsDirectory && (!string.IsNullOrEmpty(localFileName)))
                {
                    var fileNamePath = FixPath(Path.Combine(destPath, localFileName));
                    try
                    {
                        if (File.Exists(fileNamePath))
                        {
                            File.SetAttributes(fileNamePath, FileAttributes.Normal);
                            File.Delete(fileNamePath);
                        }

                        FileStream objFileStream = null;
                        try
                        {
                            File.Create(fileNamePath);
                            objFileStream = File.Open(fileNamePath);
                            int intSize = 2048;
                            var arrData = new byte[2048];
                            intSize = zipStream.Read(arrData, 0, arrData.Length);
                            while (intSize > 0)
                            {
                                objFileStream.Write(arrData, 0, intSize);
                                intSize = zipStream.Read(arrData, 0, arrData.Length);
                            }
                        }
                        finally
                        {
                            if (objFileStream != null)
                            {
                                objFileStream.Close();
                                objFileStream.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }

                zipEntry = zipStream.GetNextEntry();
            }
        }
        finally
        {
            if (zipStream != null)
            {
                zipStream.Close();
                zipStream.Dispose();
            }
        }
    }
}
