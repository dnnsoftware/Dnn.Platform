#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;
using Directory = SchwabenCode.QuickIO.QuickIODirectory;
using File = SchwabenCode.QuickIO.QuickIOFile;
using DirectoryInfo = SchwabenCode.QuickIO.QuickIODirectoryInfo;

#endregion

namespace DotNetNuke.Common.Utilities
{
    public class FileSystemUtils
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (FileSystemUtils));
        #region Private Methods

        private static string CreateFile(IFolderInfo folder, string fileName, string contentType, Stream fileContent, bool unzip, bool overwrite, bool checkPermissions)
        {
            var strMessage = "";
            var fileManager = FileManager.Instance;

            try
            {
                var file = fileManager.AddFile(folder, fileName, fileContent, overwrite, checkPermissions, contentType);
                if (unzip && file.Extension == "zip")
                {
                    fileManager.UnzipFile(file, folder);
                }
            }
            catch (PermissionsNotMetException)
            {
                strMessage += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), folder.FolderPath);
            }
            catch (NoSpaceAvailableException)
            {
                strMessage += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), fileName);
            }
            catch (InvalidFileExtensionException)
            {
                strMessage += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), fileName, Host.AllowedExtensionWhitelist.ToDisplayString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                strMessage += "<br />" + string.Format(Localization.GetString("SaveFileError"), fileName);
            }

            return strMessage;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the filename for a file path
        /// </summary>
        /// <param name="filePath">The full name of the file</param>
        /// -----------------------------------------------------------------------------
        private static string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath).Replace(Globals.glbProtectedExtension, "");
        }

        private static int GetFolderPortalID(PortalSettings settings)
        {
            return (settings.ActiveTab.ParentId == settings.SuperTabId) ? Null.NullInteger : settings.PortalId;
        }

        private static void RemoveOrphanedFiles(FolderInfo folder, int PortalId)
        {
            if (folder.FolderMappingID != FolderMappingController.Instance.GetFolderMapping(PortalId, "Database").FolderMappingID)
            {
                foreach (FileInfo objFile in FolderManager.Instance.GetFiles(folder))
                {
                    RemoveOrphanedFile(objFile, PortalId);
                }
            }
        }

        private static void RemoveOrphanedFile(FileInfo objFile, int PortalId)
        {
            FileManager.Instance.DeleteFile(objFile);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Writes a Stream to the appropriate File Storage
        /// </summary>
		/// <param name="objResponse">The Id of the File</param>
		/// <param name="objStream">The Input Stream</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private static void WriteStream(HttpResponse objResponse, Stream objStream)
        {
            //Buffer to read 10K bytes in chunk:
            var bytBuffer = new byte[10000];

            //Length of the file:
            int intLength;

            //Total bytes to read:
            long lngDataToRead;
            try
            {
                //Total bytes to read:
                lngDataToRead = objStream.Length;

                //Read the bytes.
                while (lngDataToRead > 0)
                {
					//Verify that the client is connected.
                    if (objResponse.IsClientConnected)
                    {
						//Read the data in buffer
                        intLength = objStream.Read(bytBuffer, 0, 10000);

                        //Write the data to the current output stream.
                        objResponse.OutputStream.Write(bytBuffer, 0, intLength);

                        //Flush the data to the HTML output.
                        objResponse.Flush();

                        lngDataToRead = lngDataToRead - intLength;
                    }
                    else
                    {
                        lngDataToRead = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                objResponse.Write("Error : " + ex.Message);
            }
            finally
            {
                if (objStream != null)
                {
                    objStream.Close();
                    objStream.Dispose();
                }
            }
        }

        #endregion

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a File to a Zip File
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static void AddToZip(ref ZipOutputStream ZipFile, string filePath, string fileName, string folder)
        {
            FileStream fs = null;
            try
            {
				//Open File Stream
                fs = File.OpenRead(filePath.Replace("/", "\\"));
				
				//Read file into byte array buffer
                var buffer = new byte[fs.Length];

                var len = fs.Read(buffer, 0, buffer.Length);
                if (len != fs.Length)
                {
                    Logger.ErrorFormat("Reading from " + filePath + " didn't read all data in buffer. " +
                                      "Requested to read {0} bytes, but was read {1} bytes", fs.Length, len);
                }

                //Create Zip Entry
                var entry = new ZipEntry(Path.Combine(folder, fileName));
                entry.DateTime = DateTime.Now;
                entry.Size = fs.Length;
                fs.Close();

                //Compress file and add to Zip file
                ZipFile.PutNextEntry(entry);
                ZipFile.Write(buffer, 0, buffer.Length);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Trys to copy a file in the file system
        /// </summary>
        /// <param name="sourceFileName">The name of the source file</param>
        /// <param name="destFileName">The name of the destination file</param>
        /// -----------------------------------------------------------------------------
        public static void CopyFile(string sourceFileName, string destFileName)
        {
            if (File.Exists(destFileName))
            {
                File.SetAttributes(destFileName, FileAttributes.Normal);
            }
            File.Copy(sourceFileName, destFileName, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes file in areas with a high degree of concurrent file access (i.e. caching, logging) 
        /// This solves file concurrency issues under heavy load.
        /// </summary>
        /// <param name="filename">String</param>
        /// <param name="waitInMilliseconds">Int16</param>
        /// <param name="maxAttempts">Int16</param>
        /// <returns>Boolean</returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static bool DeleteFileWithWait(string filename, Int16 waitInMilliseconds, Int16 maxAttempts)
        {
            if (!File.Exists(filename))
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
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                    fileDeleted = true; //we don't care if it didn't exist...the operation didn't fail, that's what we care about
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Trys to delete a file from the file system
        /// </summary>
		/// <param name="fileName">The name of the file</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                fileName = fileName.Replace('/', '\\');
                File.SetAttributes(fileName, FileAttributes.Normal);
                File.Delete(fileName);
            }
        }

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

        public static void UnzipResources(ZipInputStream zipStream, string destPath)
        {
            try
            {
                var zipEntry = zipStream.GetNextEntry();
                while (zipEntry != null)
                {
                    HtmlUtils.WriteKeepAlive();
                    var localFileName = zipEntry.Name;
                    var relativeDir = Path.GetDirectoryName(zipEntry.Name);
                    if (!string.IsNullOrEmpty(relativeDir) && (!Directory.Exists(Path.Combine(destPath, relativeDir))))
                    {
                        Directory.Create(Path.Combine(destPath, relativeDir), true);
                    }
                    if (!zipEntry.IsDirectory && (!string.IsNullOrEmpty(localFileName)))
                    {
                        var fileNamePath = Path.Combine(destPath, localFileName).Replace("/", "\\");
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
                        catch(Exception ex)
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

        public static string DeleteFiles(Array arrPaths)
        {
            var strExceptions = "";
            for (var i = 0; i < arrPaths.Length; i++)
            {
                var strPath = (arrPaths.GetValue(i) ?? "").ToString();
                var pos = strPath.IndexOf("'", StringComparison.Ordinal);
                if (pos != -1)
                {
                    // the (') represents a comment to the end of the line
                    strPath = strPath.Substring(0, pos);
                }

                strPath = strPath.Trim().Replace("/", "\\").TrimStart('\\');
                if (!string.IsNullOrEmpty(strPath))
                {
                    strPath = Path.Combine(Globals.ApplicationMapPath, strPath);
                    if (strPath.EndsWith("\\") && Directory.Exists(strPath))
                    {
                        var directoryInfo = new System.IO.DirectoryInfo(strPath);
                        var applicationPath = Globals.ApplicationMapPath + "\\";
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

        public static void DeleteFilesRecursive(string strRoot, string filter)
        {
            if (!String.IsNullOrEmpty(strRoot))
            {
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
                    foreach (string strFile in Directory.EnumerateFilePaths(new DirectoryInfo(strRoot)).Where(f => f.Contains(filter)))
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

        public static void DeleteFolderRecursive(string strRoot)
        {
            strRoot = strRoot.Replace("/", "\\");
            if (string.IsNullOrEmpty(strRoot) || !Directory.Exists(strRoot.Trim()))
            {   Logger.Info(strRoot + " does not exist. ");
                return;
            }

            foreach (string strFolder in Directory.EnumerateDirectoryPaths(strRoot))
            {
                DeleteFolderRecursive(strFolder);
            }

            foreach (string strFile in Directory.EnumerateFilePaths(new DirectoryInfo(strRoot)))
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

    }
}
