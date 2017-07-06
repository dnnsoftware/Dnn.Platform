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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static string CheckValidFileName(string fileName)
        {
            FileExtensionWhitelist whiteList = Host.AllowedExtensionWhitelist;
            if (!whiteList.IsAllowedExtension(Path.GetExtension(fileName)))
            {
                if (HttpContext.Current != null)
                {
                    return "<br />" + string.Format(Localization.GetString("RestrictedFileType"), fileName, whiteList.ToDisplayString());
                }

                return "RestrictedFileType";
            }

            return Null.NullString;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by PathUtils.Instance.AddTrailingSlash(string source) ")]
        public static string AddTrailingSlash(string strSource)
        {
            return PathUtils.Instance.AddTrailingSlash(strSource);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by PathUtils.Instance.RemoveTrailingSlash(string source) ")]
        public static string RemoveTrailingSlash(string strSource)
        {
            return PathUtils.Instance.RemoveTrailingSlash(strSource);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by PathUtils.Instance.StripFolderPath(string originalPath) ")]
        public static string StripFolderPath(string strOrigPath)
        {
            return PathUtils.Instance.StripFolderPath(strOrigPath);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by PathUtils.Instance.FormatFolderPath(string folderPath) ")]
        public static string FormatFolderPath(string folderPath)
        {
            return PathUtils.Instance.FormatFolderPath(folderPath);
        }

        /// <summary>
        /// The MapPath method maps the specified relative or virtual path to the corresponding physical directory on the server.
        /// </summary>
        /// <param name="path">Specifies the relative or virtual path to map to a physical directory. If Path starts with either 
        /// a forward (/) or backward slash (\), the MapPath method returns a path as if Path were a full, virtual path. If Path 
        /// doesn't start with a slash, the MapPath method returns a path relative to the directory of the .asp file being processed</param>
        /// <returns></returns>
        /// <remarks>If path is a null reference (Nothing in Visual Basic), then the MapPath method returns the full physical path 
        /// of the directory that contains the current application</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by PathUtils.Instance.MapPath(string path) ")]
        public static string MapPath(string path)
        {
            return PathUtils.Instance.MapPath(path);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.AddAllUserReadPermission(IFolderInfo folder, PermissionInfo permission) ")]
        public static void AddAllUserReadPermission(FolderInfo folder, PermissionInfo permission)
        {
            FolderManager.Instance.AddAllUserReadPermission(folder, permission);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a File
        /// </summary>
        /// <param name="FileName">File name</param>
        /// <param name="PortalId">The Id of the Portal</param>
		/// <param name="Folder">the folder to save file</param>
		/// <param name="HomeDirectoryMapPath"></param>
        /// <param name="contentType">The type of the content</param>
        /// <remarks>This method adds a new file
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite) ")]
        public static void AddFile(string FileName, int PortalId, string Folder, string HomeDirectoryMapPath, string contentType)
        {
            var fileManager = FileManager.Instance;

            var folder = FolderManager.Instance.GetFolder(PortalId, Folder);

            var file = fileManager.GetFile(folder, FileName);

            if (file == null)
            {
                file = new FileInfo { PortalId = PortalId, FolderId = folder.FolderID, FileName = FileName };
                using (var fileContent = fileManager.GetFileContent(file))
                {
                    fileManager.AddFile(folder, FileName, fileContent, false);
                }
            }
            else
            {
                using (var fileContent = fileManager.GetFileContent(file))
                {
                    fileManager.UpdateFile(file, fileContent);
                }
            }
        }

        /// <summary>
        /// Adds a File
        /// </summary>
        /// <param name="strFile">The File Name</param>
        /// <param name="PortalId">The Id of the Portal</param>
        /// <param name="ClearCache">A flag that indicates whether the file cache should be cleared</param>
        /// <param name="folder"></param>
        /// <remarks>This method is called by the SynchonizeFolder method, when the file exists in the file system
        /// but not in the Database
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite) ")]
        public static string AddFile(string strFile, int PortalId, bool ClearCache, FolderInfo folder)
        {
            var fileManager = FileManager.Instance;

            var fileName = GetFileName(strFile);

            var file = (FileInfo)fileManager.GetFile(folder, fileName);
            
            if (file == null)
            {
                file = new FileInfo { PortalId = PortalId, FolderId = folder.FolderID, FileName = fileName };
                using (var fileContent = fileManager.GetFileContent(file))
                {
                    fileManager.AddFile(folder, GetFileName(strFile), fileContent, false);
                }
            }
            else
            {
                using (var fileContent = fileManager.GetFileContent(file))
                {
                    fileManager.UpdateFile(file, fileContent);
                }
            }

            return "";
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a Folder
        /// </summary>
		/// <param name="portalSettings">The Portal Settings</param>
		/// <param name="parentFolder">The parent folder</param>
		/// <param name="newFolder">The new folder name</param>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.AddFolder(FolderMappingInfo folderMapping, string folderPath) ")]
        public static void AddFolder(PortalSettings portalSettings, string parentFolder, string newFolder)
        {
            var portalID = GetFolderPortalID(portalSettings);
            var folderMapping = FolderMappingController.Instance.GetDefaultFolderMapping(portalID);

            if (folderMapping != null)
            {
#pragma warning disable 612,618
                AddFolder(portalSettings, parentFolder, newFolder, folderMapping.FolderMappingID);
#pragma warning restore 612,618
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a Folder with a specificed unique identifier
        /// </summary>
		/// <param name="portalSettings">The Portal Settings</param>
		/// <param name="parentFolder">The parent folder</param>
		/// <param name="newFolder">The new folder name</param>
		/// <param name="storageLocation">The storage location</param>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.AddFolder(FolderMappingInfo folderMapping, string folderPath) ")]
        public static void AddFolder(PortalSettings portalSettings, string parentFolder, string newFolder, int storageLocation)
        {
#pragma warning disable 612,618
            AddFolder(portalSettings, parentFolder, newFolder, storageLocation, new Guid());
#pragma warning restore 612,618
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.AddFolder(FolderMappingInfo folderMapping, string folderPath) ")]
        public static void AddFolder(PortalSettings portalSettings, string parentFolder, string newFolder, int storageLocation, Guid uniqueId)
        {
            FolderMappingInfo folderMapping;

            switch (storageLocation)
            {
                case (int)FolderController.StorageLocationTypes.InsecureFileSystem:
                    folderMapping = FolderMappingController.Instance.GetFolderMapping(portalSettings.PortalId, "Standard");
                    break;
                case (int)FolderController.StorageLocationTypes.SecureFileSystem:
                    folderMapping = FolderMappingController.Instance.GetFolderMapping(portalSettings.PortalId, "Secure");
                    break;
                case (int)FolderController.StorageLocationTypes.DatabaseSecure:
                    folderMapping = FolderMappingController.Instance.GetFolderMapping(portalSettings.PortalId, "Database");
                    break;
                default:
                    folderMapping = FolderMappingController.Instance.GetDefaultFolderMapping(portalSettings.PortalId);
                    break;
            }
            
            if (folderMapping != null)
            {
                var folderManager = FolderManager.Instance;

				//get relative folder path.
                var folderPath = PathUtils.Instance.GetRelativePath(folderMapping.PortalID, parentFolder) + newFolder;

				if (Path.IsPathRooted(folderPath))
				{
					folderPath = folderPath.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
				}

				folderPath = folderPath.Replace("\\", "/");
                
                var folder = folderManager.AddFolder(folderMapping, folderPath);
                folder.UniqueId = uniqueId;
                
                folderManager.UpdateFolder(folder);
            }
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a User Folder
        /// </summary>
		/// <param name="portalSettings">Portal Settings for the Portal</param>
        /// <param name="parentFolder">The Parent Folder Name</param>
        /// <param name="userID">The UserID, in order to generate the path/foldername</param>
		/// <param name="storageLocation">The Storage Location</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static void AddUserFolder(PortalSettings portalSettings, string parentFolder, int storageLocation, int userID)
        {
            var user = UserController.GetUserById(portalSettings.PortalId, userID);
            var folderManager = new FolderManager();
            folderManager.AddUserFolder(user);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by PathUtils.Instance.GetUserFolderPath(UserInfo user) ")]
        public static string GetUserFolderPath(int userID)
        {
            var user = UserController.GetUserById(PortalController.Instance.GetCurrentPortalSettings().PortalId, userID);
            return PathUtils.Instance.GetUserFolderPath(user);
        }

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
                var crc = new Crc32();
                fs = File.OpenRead(filePath.Replace("/", "\\"));
				
				//Read file into byte array buffer
                var buffer = new byte[fs.Length];

                fs.Read(buffer, 0, buffer.Length);

                //Create Zip Entry
                var entry = new ZipEntry(Path.Combine(folder, fileName));
                entry.DateTime = DateTime.Now;
                entry.Size = fs.Length;
                fs.Close();
                crc.Reset();
                crc.Update(buffer);
                entry.Crc = crc.Value;

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
        /// Copies a File
        /// </summary>
        /// <param name="strSourceFile">The original File Name</param>
        /// <param name="strDestFile">The new File Name</param>
        /// <param name="settings">The Portal Settings for the Portal/Host Account</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.CopyFile(IFileInfo file, IFolderInfo destinationFolder) ")]
        public static string CopyFile(string strSourceFile, string strDestFile, PortalSettings settings)
        {
            var folderManager = FolderManager.Instance;
            var fileManager = FileManager.Instance;

            var portalID = GetFolderPortalID(settings);

            var folderPath = Globals.GetSubFolderPath(strSourceFile, portalID);
            var folder = folderManager.GetFolder(portalID, folderPath);

            if (folder != null)
            {
                var file = fileManager.GetFile(folder, GetFileName(strSourceFile));

                if (file != null)
                {
                    var destFolderPath = Globals.GetSubFolderPath(strDestFile, portalID);
                    var destFolder = folderManager.GetFolder(portalID, destFolderPath);

                    if (destFolder != null)
                    {
                        fileManager.CopyFile(file, destFolder);
                    }
                }
            }

            return "";
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UploadFile pocesses a single file 
        /// </summary>
        /// <param name="rootPath">The folder where the file will be put</param>
        /// <param name="fileName">The file name</param>
        /// <param name="fileData">Content of the file</param>
        /// <param name="contentType">Type of content, ie: text/html</param>
        /// <param name="newFileName"></param>
        /// <param name="unzip"></param> 
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite, bool checkPermissions, string contentType) ")]
        public static string CreateFileFromString(string rootPath, string fileName, string fileData, string contentType, string newFileName, bool unzip)
        {
            var returnValue = string.Empty;
            MemoryStream memStream = null;

            try
            {
                memStream = new MemoryStream();
                byte[] fileDataBytes = Encoding.UTF8.GetBytes(fileData);
                memStream.Write(fileDataBytes, 0, fileDataBytes.Length);
                memStream.Flush();
                memStream.Position = 0;

                var fileManager = FileManager.Instance;
                var folderManager = FolderManager.Instance;

                var settings = PortalController.Instance.GetCurrentPortalSettings();
                var portalID = GetFolderPortalID(settings);

                if (newFileName != Null.NullString)
                {
                    fileName = newFileName;
                }

                fileName = Path.GetFileName(fileName);

                var folderPath = Globals.GetSubFolderPath(rootPath + fileName, portalID);

                var folder = folderManager.GetFolder(portalID, folderPath);

                returnValue = CreateFile(folder, fileName, ((FileManager)fileManager).GetContentType(Path.GetExtension(fileName)), memStream, unzip, true, true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                returnValue = ex.Message;
            }
            finally
            {
                if (((memStream != null)))
                {
                    memStream.Close();
                    memStream.Dispose();
                }
            }

            return returnValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This checks to see if the folder is a protected type of folder 
        /// </summary>
        /// <param name="folderPath">String</param>
        /// <returns>Boolean</returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by PathUtils.Instance.IsDefaultProtectedPath(string folderPath) ")]
        public static bool DefaultProtectedFolders(string folderPath)
        {
            return PathUtils.Instance.IsDefaultProtectedPath(folderPath);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a file
        /// </summary>
		/// <param name="sourceFile">The File to delete</param>
        /// <param name="settings">The Portal Settings for the Portal/Host Account</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.DeleteFile(IFileInfo file) ")]
        public static string DeleteFile(string sourceFile, PortalSettings settings)
        {
#pragma warning disable 612,618
            return DeleteFile(sourceFile, settings, true);
#pragma warning restore 612,618
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a file
        /// </summary>
		/// <param name="sourceFile">The File to delete</param>
        /// <param name="settings">The Portal Settings for the Portal/Host Account</param>
        /// <param name="clearCache"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.DeleteFile(IFileInfo file) ")]
        public static string DeleteFile(string sourceFile, PortalSettings settings, bool clearCache)
        {
            string retValue = "";

            var fileManager = FileManager.Instance;
            var folderManager = FolderManager.Instance;

            var fileName = GetFileName(sourceFile);
            var portalID = GetFolderPortalID(settings);
            var folderPath = Globals.GetSubFolderPath(sourceFile, portalID);
            
            var folder = folderManager.GetFolder(portalID, folderPath);

            if (folder != null)
            {
                var file = fileManager.GetFile(folder, fileName);

                if (file != null)
                {
                    try
                    {
						//try and delete the Insecure file
                        fileManager.DeleteFile(file);
                    }
                    catch (PermissionsNotMetException)
                    {
                        retValue += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), folderPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);

                        if (ex.InnerException != null && ex.InnerException.GetType() == typeof(IOException))
                        {
                            retValue += "<br />" + string.Format(Localization.GetString("FileInUse"), sourceFile);
                        }
                        else
                        {
                            retValue = ex.Message;
                        }
                    }
                }
            }
            
            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a folder
        /// </summary>
        /// <param name="PortalId">The Id of the Portal</param>
        /// <param name="folder">The Directory Info object to delete</param>
        /// <param name="folderName">The Name of the folder relative to the Root of the Portal</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.DeleteFolder(IFolderInfo folder) ")]
        public static void DeleteFolder(int PortalId, DirectoryInfo folder, string folderName)
        {
            var folderManager = FolderManager.Instance;
            var folderPath = PathUtils.Instance.GetRelativePath(PortalId, folder.FullName);
            var folderInfo = folderManager.GetFolder(PortalId, folderPath);

            if (folderInfo != null)
            {
                folderManager.DeleteFolder(folderInfo);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Moved directly from FileManager code, probably should make extension lookup more generic
        /// </summary>
		/// <param name="fileLoc">File Location</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static void DownloadFile(string fileLoc)
        {
            var objFile = new System.IO.FileInfo(fileLoc);
            HttpResponse objResponse = HttpContext.Current.Response;
            string filename = objFile.Name;

            if (objFile.Exists)
            {
                objResponse.ClearContent();
                objResponse.ClearHeaders();
                objResponse.AppendHeader("content-disposition", "attachment; filename=\"" + filename + "\"");
                objResponse.AppendHeader("Content-Length", objFile.Length.ToString());
                objResponse.ContentType = new FileManager().GetContentType(objFile.Extension.Replace(".", ""));
#pragma warning disable 612,618
                WriteFile(objFile.FullName);
#pragma warning restore 612,618
                objResponse.Flush();
                objResponse.End();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Streams a file to the output stream if the user has the proper permissions
        /// </summary>
        /// <param name="settings">Portal Settings</param>
        /// <param name="FileId">FileId identifying file in database</param>
        /// <param name="ClientCache">Cache file in client browser - true/false</param>
        /// <param name="ForceDownload">Force Download File dialog box - true/false</param>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.WriteFileToResponse(IFileInfo file, ContentDisposition contentDisposition) ")]
        public static bool DownloadFile(PortalSettings settings, int FileId, bool ClientCache, bool ForceDownload)
        {
            return DownloadFile(GetFolderPortalID(settings), FileId, ClientCache, ForceDownload);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Streams a file to the output stream if the user has the proper permissions
        /// </summary>
        /// <param name="PortalId">The Id of the Portal to which the file belongs</param>
        /// <param name="FileId">FileId identifying file in database</param>
        /// <param name="ClientCache">Cache file in client browser - true/false</param>
        /// <param name="ForceDownload">Force Download File dialog box - true/false</param>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.WriteFileToResponse(IFileInfo file, ContentDisposition contentDisposition) ")]
        public static bool DownloadFile(int PortalId, int FileId, bool ClientCache, bool ForceDownload)
        {
            var download = false;
            var fileManager = FileManager.Instance;
            var file = fileManager.GetFile(FileId);
            var contentDisposition = ForceDownload ? ContentDisposition.Attachment : ContentDisposition.Inline;

            if (file != null)
            {
                try
                {
                    fileManager.WriteFileToResponse(file, contentDisposition);
                    download = true;
                }
                catch(Exception ex)
                {
					Logger.Error(ex);
                }
            }

            return download;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// gets the content type based on the extension
        /// </summary>
        /// <param name="extension">The extension</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static string GetContentType(string extension)
        {
            return new FileManager().GetContentType(extension);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.GetFileContent(IFileInfo file) ")]
        public static byte[] GetFileContent(FileInfo file)
        {
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.GetFileContent(IFileInfo file) ")]
        public static Stream GetFileStream(FileInfo objFile)
        {
            return FileManager.Instance.GetFileContent(objFile);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFiles(IFolderInfo folder) ")]
        public static ArrayList GetFilesByFolder(int PortalId, int folderId)
        {
            var filesArray = new ArrayList();

            var folderManager = FolderManager.Instance;
            var folder = folderManager.GetFolder(folderId);

            if (folder != null)
            {
                var files = folderManager.GetFiles(folder);
                foreach (var file in files)
                {
                    filesArray.Add((FileInfo)file);
                }
            }

            return filesArray;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static int GetFolderPortalId(PortalSettings settings)
        {
            return GetFolderPortalID(settings);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets all the folders for a Portal
        /// </summary>
		/// <param name="portalID">The Id of the Portal</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFolders(int portalID) ")]
        public static ArrayList GetFolders(int portalID)
        {
            var folders = FolderManager.Instance.GetFolders(portalID);
            var foldersArray = new ArrayList();

            foreach (var folder in folders)
            {
                foldersArray.Add(folder);
            }

            return foldersArray;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFolder(int portalID, string folderPath) ")]
        public static FolderInfo GetFolder(int portalID, string folderPath)
        {
            return (FolderInfo)FolderManager.Instance.GetFolder(portalID, folderPath);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets all the subFolders for a Parent
        /// </summary>
		/// <param name="portalId">The Id of the Portal</param>
		/// <param name="parentFolder"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFolders(IFolderInfo parentFolder) ")]
        public static ArrayList GetFoldersByParentFolder(int portalId, string parentFolder)
        {
            var folder = FolderManager.Instance.GetFolder(portalId, parentFolder);
            var folders = FolderManager.Instance.GetFolders(folder);

            var subFolders = new ArrayList();

            foreach (var subfolder in folders)
            {
                subFolders.Add((FolderInfo)subfolder);
            }

            return subFolders;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFolders(UserInfo user, string permissions) or FolderManager.Instance.GetFileSystemFolders(UserInfo user, string permissions) ")]
        public static ArrayList GetFoldersByUser(int portalID, bool includeSecure, bool includeDatabase, string permissions)
        {
            var userFoldersArray = new ArrayList();
            
            var user = UserController.Instance.GetCurrentUserInfo();

            //Create Home folder if it doesn't exist
            var userFolders = (!includeSecure && !includeDatabase) ? FolderManager.Instance.GetFileSystemFolders(user, permissions) : 
                FolderManager.Instance.GetFolders(user, permissions);

            foreach (var userFolder in userFolders)
            {
				//Add User folder
                userFoldersArray.Add((FolderInfo)userFolder);
            }

            return userFoldersArray;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Moves (Renames) a File
        /// </summary>
        /// <param name="strSourceFile">The original File Name</param>
        /// <param name="strDestFile">The new File Name</param>
        /// <param name="settings">The Portal Settings for the Portal/Host Account</param>
        /// <remarks>
        /// </remarks>host  dnnhost
        /// 
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.MoveFile(IFileInfo file, IFolderInfo destinationFolder) ")]
        public static string MoveFile(string strSourceFile, string strDestFile, PortalSettings settings)
        {
            var folderManager = FolderManager.Instance;
            var fileManager = FileManager.Instance;

            var portalID = GetFolderPortalID(settings);

            var folderPath = Globals.GetSubFolderPath(strSourceFile, portalID);
            var folder = folderManager.GetFolder(portalID, folderPath);

            if (folder != null)
            {
                var file = fileManager.GetFile(folder, GetFileName(strSourceFile));

                if (file != null)
                {
                    var destFolderPath = Globals.GetSubFolderPath(strDestFile, portalID);
                    var destFolder = folderManager.GetFolder(portalID, destFolderPath);
                    var destFileName = GetFileName(strDestFile);

                    if (destFolder != null)
                    {
                        fileManager.MoveFile(file, destFolder);

                        fileManager.RenameFile(file, destFileName);
                        
                    }
                }
            }

            return "";
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static void RemoveOrphanedFolders(int portalId)
        {
            var folderManager = FolderManager.Instance;
            var databaseMapping = FolderMappingController.Instance.GetFolderMapping(portalId, "Database");

            foreach (FolderInfo objFolder in folderManager.GetFolders(portalId))
            {
                if (objFolder.FolderMappingID != databaseMapping.FolderMappingID)
                {
                    if (Directory.Exists(objFolder.PhysicalPath) == false)
                    {
                        RemoveOrphanedFiles(objFolder, portalId);
                        folderManager.DeleteFolder(objFolder);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static void SaveFile(string fullFileName, byte[] buffer)
        {
            if (File.Exists(fullFileName))
            {
                File.SetAttributes(fullFileName, FileAttributes.Normal);
            }
            FileStream fs = null;
            try
            {
                fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
                fs.Write(buffer, 0, buffer.Length);
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
        /// Assigns 1 or more attributes to a file
        /// </summary>
        /// <param name="fileLoc">File Location</param>
        /// <param name="fileAttributesOn">Pass in Attributes you wish to switch on (i.e. FileAttributes.Hidden + FileAttributes.ReadOnly)</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static void SetFileAttributes(string fileLoc, int fileAttributesOn)
        {
            File.SetAttributes(fileLoc, (FileAttributes)fileAttributesOn);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets a Folders Permissions to the Administrator Role
        /// </summary>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="folderId">The Id of the Folder</param>
        /// <param name="administratorRoleId">The Id of the Administrator Role</param>
        /// <param name="relativePath">The folder's Relative Path</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.SetFolderPermissions(IFolderInfo folder, int administratorRoleId) ")]
        public static void SetFolderPermissions(int portalId, int folderId, int administratorRoleId, string relativePath)
        {
            var folderManager = FolderManager.Instance;
            var folder = folderManager.GetFolder(folderId);
            folderManager.SetFolderPermissions(folder, administratorRoleId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.CopyParentFolderPermissions(IFolderInfo folder) ")]
        public static void SetFolderPermissions(int portalId, int folderId, string relativePath)
        {
            var folderManager = FolderManager.Instance;
            var folder = folderManager.GetFolder(folderId);
            folderManager.CopyParentFolderPermissions(folder);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets a Folders Permissions the same as the Folders parent folder
        /// </summary>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="folderId">The Id of the Folder</param>
        /// <param name="permissionId"></param>
        /// <param name="roleId"></param>
        /// <param name="relativePath">The folder's Relative Path</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.SetFolderPermission(IFolderInfo folder, int permissionId, int roleId) ")]
        public static void SetFolderPermission(int portalId, int folderId, int permissionId, int roleId, string relativePath)
        {
            var folderManager = FolderManager.Instance;
            var folder = folderManager.GetFolder(folderId);
            folderManager.SetFolderPermission(folder, permissionId, roleId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.SetFolderPermission(IFolderInfo folder, int permissionId, int roleId, int userId) ")]
        public static void SetFolderPermission(int PortalId, int FolderId, int PermissionId, int RoleId, int UserId, string relativePath)
        {
            var folderManager = FolderManager.Instance;
            var folder = folderManager.GetFolder(FolderId);
            folderManager.SetFolderPermission(folder, PermissionId, RoleId, UserId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.Synchronize(int portalID) ")]
        public static void Synchronize(int PortalId, int AdministratorRoleId, string HomeDirectory, bool hideSystemFolders)
        {
            FolderManager.Instance.Synchronize(PortalId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.Synchronize(int portalID, string relativePath, bool isRecursive, bool syncFiles) ")]
        public static void SynchronizeFolder(int PortalId, string physicalPath, string relativePath, bool isRecursive, bool hideSystemFolders)
        {
            FolderManager.Instance.Synchronize(PortalId, relativePath, isRecursive, true);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="PortalId"></param>
		/// <param name="physicalPath"></param>
		/// <param name="relativePath"></param>
		/// <param name="isRecursive"></param>
		/// <param name="syncFiles"></param>
		/// <param name="forceFolderSync"></param>
		/// <param name="hideSystemFolders"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.Synchronize(int portalID, string relativePath, bool isRecursive, bool syncFiles) ")]
        public static void SynchronizeFolder(int PortalId, string physicalPath, string relativePath, bool isRecursive, bool syncFiles, bool forceFolderSync, bool hideSystemFolders)
        {
            FolderManager.Instance.Synchronize(PortalId, relativePath, isRecursive, syncFiles);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.UnzipFile(IFileInfo file, IFolderInfo destinationFolder) ")]
        public static string UnzipFile(string fileName, string DestFolder, PortalSettings settings)
        {
            var folderManager = FolderManager.Instance;
            var fileManager = FileManager.Instance;

            var portalID = settings.PortalId;

            var folderPath = PathUtils.Instance.GetRelativePath(portalID, DestFolder);
            var folder = folderManager.GetFolder(portalID, folderPath);

            if (folder != null)
            {
                var file = fileManager.GetFile(folder, GetFileName(fileName));

                if (file != null)
                {
                    fileManager.UnzipFile(file, folder);
                }
            }

            return "";
        }

        public static void UnzipResources(ZipInputStream zipStream, string destPath)
        {
            try
            {
                var zipEntry = zipStream.GetNextEntry();
                while (zipEntry != null)
                {
                    HtmlUtils.WriteKeepAlive();
<<<<<<< HEAD
                    var localFileName = zipEntry.Name;
                    var relativeDir = Path.GetDirectoryName(zipEntry.Name);
                    if (!string.IsNullOrEmpty(relativeDir) && (!Directory.Exists(Path.Combine(destPath, relativeDir))))
=======
                    LocalFileName = objZipEntry.Name;
                    RelativeDir = Path.GetDirectoryName(objZipEntry.Name);
                    if ((RelativeDir != string.Empty) && (!Directory.Exists(Path.Combine(destPath, RelativeDir))))
>>>>>>> upstream/master
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite) ")]
        public static string UploadFile(string RootPath, HttpPostedFile objHtmlInputFile)
        {
            return UploadFile(RootPath, objHtmlInputFile, Null.NullString, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite) ")]
        public static string UploadFile(string RootPath, HttpPostedFile objHtmlInputFile, bool Unzip)
        {
            return UploadFile(RootPath, objHtmlInputFile, Null.NullString, Unzip);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite) ")]
        public static string UploadFile(string RootPath, HttpPostedFile objHtmlInputFile, string NewFileName)
        {
            return UploadFile(RootPath, objHtmlInputFile, NewFileName, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite) ")]
        public static string UploadFile(string RootPath, HttpPostedFile objHtmlInputFile, string NewFileName, bool Unzip)
        {
            var fileManager = FileManager.Instance;
            var folderManager = FolderManager.Instance;

            var settings = PortalController.Instance.GetCurrentPortalSettings();
            var portalID = GetFolderPortalID(settings);

            var fileName = objHtmlInputFile.FileName;

            if (NewFileName != Null.NullString)
            {
                fileName = NewFileName;
            }

            fileName = Path.GetFileName(fileName);

            var folderPath = Globals.GetSubFolderPath(RootPath + fileName, portalID);

            var folder = folderManager.GetFolder(portalID, folderPath);

            return CreateFile(folder, fileName, ((FileManager)fileManager).GetContentType(Path.GetExtension(fileName)), objHtmlInputFile.InputStream, Unzip, true, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static void WriteFile(string strFileName)
        {
            HttpResponse objResponse = HttpContext.Current.Response;
            Stream objStream = null;
            try
            {
                objStream = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                WriteStream(objResponse, objStream);
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

                strPath = strPath.Trim().Replace("/", "\\");
                if (!string.IsNullOrEmpty(strPath))
                {
                    strPath = Globals.ApplicationMapPath + "\\" + strPath;
                    if (strPath.EndsWith("\\"))
                    {
                        if (Directory.Exists(strPath))
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
            if (String.IsNullOrEmpty(strRoot) || !Directory.Exists(strRoot.Trim()))
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static string SendFile(string URL, string FilePath)
        {
            string strMessage = "";
            try
            {
                var objWebClient = new WebClient();
                byte[] responseArray = objWebClient.UploadFile(URL, "POST", FilePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                strMessage = ex.Message;
            }
            return strMessage;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static string ReceiveFile(HttpRequest Request, string FolderPath)
        {
            string strMessage = "";
            try
            {
                if (Request.Files.AllKeys.Length != 0)
                {
                    string strKey = Request.Files.AllKeys[0];
                    HttpPostedFile objFile = Request.Files[strKey];
                    objFile.SaveAs(FolderPath + objFile.FileName);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                strMessage = ex.Message;
            }
            return strMessage;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static string PullFile(string URL, string FilePath)
        {
            string strMessage = "";
            try
            {
                var objWebClient = new WebClient();
                objWebClient.DownloadFile(URL, FilePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                strMessage = ex.Message;
            }
            return strMessage;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static string GetHash(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return ((FileManager) FileManager.Instance).GetHash(stream);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static string GetHash(Stream stream)
        {
            return ((FileManager)FileManager.Instance).GetHash(stream);
        }

        #region Obsolete Methods

#pragma warning disable 612,618

        //Overload to preserve backwards compatability
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by Synchronize(PortalId, AdministratorRoleId, HomeDirectory, hideSystemFolders). Deprecated in DotNetNuke 5.3.0")]
        public static void Synchronize(int PortalId, int AdministratorRoleId, string HomeDirectory)
        {
            Synchronize(PortalId, AdministratorRoleId, HomeDirectory, false);
        }

        //Overload to preserve backwards compatability
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by SynchronizeFolder(PortalId, physicalPath, relativePath, isRecursive, hideSystemFolders). Deprecated in DotNetNuke 5.3.0")]
        public static void SynchronizeFolder(int PortalId, string physicalPath, string relativePath, bool isRecursive)
        {
            SynchronizeFolder(PortalId, physicalPath, relativePath, isRecursive, false);
        }

        //Overload to preserve backwards compatability
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by SynchronizeFolder(PortalId, physicalPath, relativePath, isRecursive, syncFiles, forceFolderSync, hideSystemFolders). Deprecated in DotNetNuke 5.3.0")]
        public static void SynchronizeFolder(int PortalId, string physicalPath, string relativePath, bool isRecursive, bool syncFiles, bool forceFolderSync)
        {
            SynchronizeFolder(PortalId, physicalPath, relativePath, isRecursive, syncFiles, forceFolderSync, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by GetFileContent(FileInfo)")]
        public static byte[] GetFileContent(FileInfo file, int PortalId, string HomeDirectory)
        {
            return GetFileContent(file);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by GetFilesByFolder(PortalId, FolderId)")]
        public static ArrayList GetFilesByFolder(int PortalId, string folderPath)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(PortalId, folderPath, false);
            if (objFolder == null)
            {
                return null;
            }
            return GetFilesByFolder(PortalId, objFolder.FolderID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by GetFileStream(FileInfo)")]
        public static Stream GetFileStream(FileInfo objFile, int PortalId, string HomeDirectory)
        {
            return GetFileStream(objFile);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by GetFoldersByUser(ByVal PortalID As Integer, ByVal IncludeSecure As Boolean, ByVal IncludeDatabase As Boolean, ByVal Permissions As String)")]
        public static ArrayList GetFoldersByUser(int PortalID, bool IncludeSecure, bool IncludeDatabase, bool AllowAccess, string Permissions)
        {
            return GetFoldersByUser(PortalID, IncludeSecure, IncludeDatabase, Permissions);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1.  Use FolderPermissionController.GetFolderPermissionsCollectionByFolder(PortalId, Folder).ToString(Permission) ")]
        public static string GetRoles(string Folder, int PortalId, string Permission)
        {
            return FolderPermissionController.GetFolderPermissionsCollectionByFolder(PortalId, Folder).ToString(Permission);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by SynchronizeFolder(Integer, Integer, String, String, Boolean)")]
        public static void SynchronizeFolder(int PortalId, int AdministratorRoleId, string HomeDirectory, string physicalPath, string relativePath, bool isRecursive)
        {
            SynchronizeFolder(PortalId, physicalPath, relativePath, isRecursive, true, true, false);
        }

#pragma warning restore 612,618
        #endregion
    }
}
