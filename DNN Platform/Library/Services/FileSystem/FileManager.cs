// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem.EventArgs;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Log.EventLog;
    using ICSharpCode.SharpZipLib.Zip;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// Exposes methods to manage files.
    /// </summary>
    public class FileManager : ComponentBase<IFileManager, FileManager>, IFileManager
    {
        private const int BufferSize = 4096;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileManager));

        public virtual IDictionary<string, string> ContentTypes
        {
            get { return FileContentTypeManager.Instance.ContentTypes; }
        }

        private FileExtensionWhitelist WhiteList
        {
            get
            {
                var user = UserController.Instance.GetCurrentUserInfo();
                if (user != null)
                {
                    if (user.IsSuperUser)
                    {
                        return Host.AllowedExtensionWhitelist;
                    }

                    if (!user.IsAdmin)
                    {
                        var settings = PortalSettings.Current;
                        if (settings != null)
                        {
                            return settings.AllowedExtensionsWhitelist;
                        }
                    }
                }

                return Host.AllowedExtensionWhitelist;
            }
        }

        /// <summary>
        /// Adds a file to the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        public virtual IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent)
        {
            return this.AddFile(folder, fileName, fileContent, true, false, false, this.GetContentType(Path.GetExtension(fileName)), this.GetCurrentUserID());
        }

        /// <summary>
        /// Adds a file to the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exits.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        public virtual IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite)
        {
            return this.AddFile(folder, fileName, fileContent, overwrite, false, false, this.GetContentType(Path.GetExtension(fileName)), this.GetCurrentUserID());
        }

        /// <summary>
        /// Adds a file to the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exists.</param>
        /// <param name="checkPermissions">Indicates if permissions have to be met.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder, fileName or fileContent are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.InvalidFileExtensionException">Thrown when the extension of the specified file is not allowed.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.NoSpaceAvailableException">Thrown when the portal has no space available to store the specified file.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.PermissionsNotMetException">Thrown when permissions are not met.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        public virtual IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite, bool checkPermissions, string contentType)
        {
            return this.AddFile(folder, fileName, fileContent, overwrite, checkPermissions, false, contentType, this.GetCurrentUserID());
        }

        /// <summary>
        /// Adds a file to the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exists.</param>
        /// <param name="checkPermissions">Indicates if permissions have to be met.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="createdByUserID">ID of the user that creates the file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder, fileName or fileContent are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.InvalidFileExtensionException">Thrown when the extension of the specified file is not allowed.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.NoSpaceAvailableException">Thrown when the portal has no space available to store the specified file.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.PermissionsNotMetException">Thrown when permissions are not met.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        public virtual IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite, bool checkPermissions, string contentType, int createdByUserID)
        {
            return this.AddFile(folder, fileName, fileContent, overwrite, checkPermissions, false, contentType, createdByUserID);
        }

        /// <summary>
        /// Adds a file to the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exists.</param>
        /// <param name="checkPermissions">Indicates if permissions have to be met.</param>
        /// <param name="ignoreWhiteList">Indicates whether the whitelist should be ignored.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="createdByUserID">ID of the user that creates the file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder, fileName or fileContent are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.InvalidFileExtensionException">Thrown when the extension of the specified file is not allowed.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.NoSpaceAvailableException">Thrown when the portal has no space available to store the specified file.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.PermissionsNotMetException">Thrown when permissions are not met.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        public virtual IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite, bool checkPermissions, bool ignoreWhiteList, string contentType, int createdByUserID)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            this.CheckFileAddingRestrictions(folder, fileName, checkPermissions, ignoreWhiteList);

            // DNN-2949 If IgnoreWhiteList is set to true , then file should be copied and info logged into Event Viewer
            if (!this.IsAllowedExtension(fileName) && ignoreWhiteList)
            {
                var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Following file was imported/uploaded, but is not an authorized filetype: ", fileName));
                LogController.Instance.AddLog(log);
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

            bool fileExists = this.FileExists(folder, fileName, true);
            bool needToWriteFile = fileContent != null && (overwrite || !folderProvider.FileExists(folder, fileName));
            bool usingSeekableStream = false;

            if (fileContent != null && !needToWriteFile && this.FileExists(folder, fileName))
            {
                return this.GetFile(folder, fileName);
            }

            var oldFile = fileExists ? this.GetFile(folder, fileName, true) : null;

            var now = DateTime.Now;
            var extension = Path.GetExtension(fileName);
            var file = new FileInfo
            {
                PortalId = folder.PortalID,
                FileName = fileName,
                Extension = (!string.IsNullOrEmpty(extension)) ? extension.Replace(".", string.Empty) : string.Empty,
                Width = Null.NullInteger,
                Height = Null.NullInteger,
                ContentType = contentType,
                Folder = folder.FolderPath,
                FolderId = folder.FolderID,
                LastModificationTime = now,
                StartDate = now,
                EndDate = Null.NullDate,
                EnablePublishPeriod = false,
                ContentItemID = oldFile != null ? oldFile.ContentItemID : Null.NullInteger,
                Title = oldFile != null ? oldFile.Title : Null.NullString,
                SHA1Hash = oldFile != null ? oldFile.SHA1Hash : string.Empty,
            };

            try
            {
                Workflow folderWorkflow = null;
                var contentFileName = fileName;
                if (needToWriteFile)
                {
                    if (!fileContent.CanSeek)
                    {
                        fileContent = this.GetSeekableStream(fileContent);
                        usingSeekableStream = true;
                    }

                    this.CheckFileWritingRestrictions(folder, fileName, fileContent, oldFile, createdByUserID);

                    // Retrieve Metadata
                    this.SetInitialFileMetadata(ref fileContent, file, folderProvider);

                    // Workflow
                    folderWorkflow = WorkflowManager.Instance.GetWorkflow(folder.WorkflowID);
                    if (folderWorkflow != null)
                    {
                        this.SetContentItem(file);

                        file.FileId = oldFile != null ? oldFile.FileId : Null.NullInteger;
                        if (folderWorkflow.WorkflowID == SystemWorkflowManager.Instance.GetDirectPublishWorkflow(folderWorkflow.PortalID).WorkflowID)
                        {
                            if (file.FileId == Null.NullInteger)
                            {
                                this.AddFile(file, createdByUserID);
                                fileExists = true;
                            }
                            else
                            {
                                // File Events for updating will be not fired. Only events for adding nust be fired
                                this.UpdateFile(file, true, false);
                            }

                            contentFileName = this.ProcessVersioning(folder, oldFile, file, createdByUserID);
                        }
                        else
                        {
                            contentFileName = this.UpdateWhileApproving(folder, createdByUserID, file, oldFile, fileContent);

                            // This case will be to overwrite an existing file or initial file workflow
                            this.ManageFileAdding(createdByUserID, folderWorkflow, fileExists, file);
                        }
                    }

                    // Versioning
                    else
                    {
                        contentFileName = this.ProcessVersioning(folder, oldFile, file, createdByUserID);
                    }
                }
                else
                {
                    file.Size = (int)folderProvider.GetFileSize(file);
                    file.SHA1Hash = folderProvider.GetHashCode(file);
                }

                var isDatabaseProvider = folderMapping.FolderProviderType == "DatabaseFolderProvider";

                try
                {
                    // add file into database first if folder provider is default providers
                    // add file into database after file saved into folder provider for remote folder providers to avoid multiple thread issue.
                    if (isDatabaseProvider)
                    {
                        if (folderWorkflow == null || !fileExists)
                        {
                            this.ManageFileAdding(createdByUserID, folderWorkflow, fileExists, file);
                        }

                        if (needToWriteFile)
                        {
                            folderProvider.AddFile(folder, contentFileName, fileContent);
                        }
                    }
                    else
                    {
                        if (needToWriteFile)
                        {
                            folderProvider.AddFile(folder, contentFileName, fileContent);
                        }

                        if (folderWorkflow == null || !fileExists)
                        {
                            this.ManageFileAdding(createdByUserID, folderWorkflow, fileExists, file);
                        }
                    }

                    var providerLastModificationTime = folderProvider.GetLastModificationTime(file);
                    if (file.LastModificationTime != providerLastModificationTime)
                    {
                        DataProvider.Instance().UpdateFileLastModificationTime(file.FileId, providerLastModificationTime);
                    }

                    var providerHash = folderProvider.GetHashCode(file);
                    if (file.SHA1Hash != providerHash)
                    {
                        DataProvider.Instance()
                            .UpdateFileHashCode(file.FileId, providerHash);
                    }
                }
                catch (FileLockedException fle)
                {
                    Logger.Error(fle);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);

                    if (!folderProvider.FileExists(folder, file.FileName))
                    {
                        FileDeletionController.Instance.DeleteFileData(file);
                    }

                    throw new FolderProviderException(
                        Localization.GetExceptionMessage(
                            "AddFileUnderlyingSystemError",
                            "The underlying system threw an exception. The file has not been added."),
                        ex);
                }

                DataCache.RemoveCache("GetFileById" + file.FileId);
                this.ClearFolderCache(folder.PortalID);
                var addedFile = this.GetFile(file.FileId, true); // The file could be pending to be approved, but it should be returned

                this.NotifyFileAddingEvents(folder, createdByUserID, fileExists, folderWorkflow, addedFile);

                return addedFile;
            }
            finally
            {
                if (usingSeekableStream)
                {
                    fileContent.Dispose();
                }
            }
        }

        /// <summary>
        /// Copies the specified file into the specified folder.
        /// </summary>
        /// <param name="file">The file to copy.</param>
        /// <param name="destinationFolder">The folder where to copy the file to.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file or destinationFolder are null.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> with the information of the copied file.</returns>
        public virtual IFileInfo CopyFile(IFileInfo file, IFolderInfo destinationFolder)
        {
            Requires.NotNull("file", file);
            Requires.NotNull("destinationFolder", destinationFolder);

            if (file.FolderMappingID == destinationFolder.FolderMappingID)
            {
                if (!FolderPermissionController.Instance.CanAddFolder(destinationFolder))
                {
                    throw new PermissionsNotMetException(Localization.GetExceptionMessage("CopyFilePermissionsNotMet", "Permissions are not met. The file has not been copied."));
                }

                if (!PortalController.Instance.HasSpaceAvailable(destinationFolder.PortalID, file.Size))
                {
                    throw new NoSpaceAvailableException(Localization.GetExceptionMessage("CopyFileNoSpaceAvailable", "The portal has no space available to store the specified file. The file has not been copied."));
                }

                var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
                try
                {
                    // check for existing file
                    var existingFile = this.GetFile(destinationFolder, file.FileName, true);
                    if (existingFile != null)
                    {
                        this.DeleteFile(existingFile);
                    }

                    var folder = FolderManager.Instance.GetFolder(file.FolderId);
                    FolderProvider.Instance(folderMapping.FolderProviderType).CopyFile(folder.MappedPath, file.FileName, destinationFolder.MappedPath, folderMapping);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    throw new FolderProviderException(Localization.GetExceptionMessage("CopyFileUnderlyingSystemError", "The underlying system throw an exception. The file has not been copied."), ex);
                }

                // copy Content Item
                var contentItemID = this.CopyContentItem(file.ContentItemID);

                var fileId = DataProvider.Instance().AddFile(
                    file.PortalId,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    file.FileName,
                    file.Extension,
                    file.Size,
                    file.Width,
                    file.Height,
                    file.ContentType,
                    destinationFolder.FolderPath,
                    destinationFolder.FolderID,
                    this.GetCurrentUserID(),
                    file.SHA1Hash,
                    DateTime.Now,
                    file.Title,
                    file.Description,
                    file.StartDate,
                    file.EndDate,
                    file.EnablePublishPeriod,
                    contentItemID);

                var copiedFile = this.GetFile(fileId, true);

                // Notify added file event
                this.OnFileAdded(copiedFile, destinationFolder, this.GetCurrentUserID());

                return copiedFile;
            }

            using (var fileContent = this.GetFileContent(file))
            {
                // check for existing file
                var existingFile = this.GetFile(destinationFolder, file.FileName, true);
                if (existingFile != null)
                {
                    this.DeleteFile(existingFile);
                }

                return this.AddFile(destinationFolder, file.FileName, fileContent, true, true, file.ContentType);
            }
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="file">The file to delete.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual void DeleteFile(IFileInfo file)
        {
            Requires.NotNull("file", file);
            FileDeletionController.Instance.DeleteFile(file);
            this.ClearFolderCache(file.PortalId);

            // Notify File Delete Event
            this.OnFileDeleted(file, this.GetCurrentUserID());
        }

        /// <summary>
        /// Deletes the specified files.
        /// </summary>
        /// <param name="files">The files to delete.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when files is null.</exception>
        public virtual void DeleteFiles(IEnumerable<IFileInfo> files)
        {
            Requires.NotNull("files", files);

            foreach (var file in files)
            {
                this.DeleteFile(file);
            }
        }

        /// <summary>
        /// Checks the existence of the specified file in the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to check the existence of the file.</param>
        /// <param name="fileName">The file name to check the existence of.</param>
        /// <returns>A bool value indicating whether the file exists or not in the specified folder.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when fileName is null or empty.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual bool FileExists(IFolderInfo folder, string fileName)
        {
            return this.FileExists(folder, fileName, false);
        }

        /// <summary>
        /// Checks the existence of the specified file in the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to check the existence of the file.</param>
        /// <param name="fileName">The file name to check the existence of.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>A bool value indicating whether the file exists or not in the specified folder.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when fileName is null or empty.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual bool FileExists(IFolderInfo folder, string fileName, bool retrieveUnpublishedFiles)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var file = this.GetFile(folder, fileName, retrieveUnpublishedFiles);
            var existsFile = file != null;
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            try
            {
                existsFile = existsFile && FolderProvider.Instance(folderMapping.FolderProviderType).FileExists(folder, fileName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception."), ex);
            }

            return existsFile;
        }

        /// <summary>
        /// Gets the Content Type for the specified file extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>The Content Type for the specified extension.</returns>
        public virtual string GetContentType(string extension)
        {
            return FileContentTypeManager.Instance.GetContentType(extension);
        }

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="fileID">The file identifier.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(int fileID)
        {
            return this.GetFile(fileID, false);
        }

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="fileID">The file identifier.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(int fileID, bool retrieveUnpublishedFiles)
        {
            if (fileID == 0 || fileID == -1)
            {
                return null;
            }

            var strCacheKey = "GetFileById" + fileID;
            var file = DataCache.GetCache(strCacheKey);
            if (file == null)
            {
                file = CBO.Instance.FillObject<FileInfo>(DataProvider.Instance().GetFileById(fileID, retrieveUnpublishedFiles));
                if (file != null)
                {
                    var intCacheTimeout = 20 * Convert.ToInt32(this.GetPerformanceSetting());
                    DataCache.SetCache(strCacheKey, file, TimeSpan.FromMinutes(intCacheTimeout));
                }
            }

            return (IFileInfo)file;
        }

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="folder">The folder where the file is stored.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(IFolderInfo folder, string fileName)
        {
            return this.GetFile(folder, fileName, false);
        }

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="folder">The folder where the file is stored.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(IFolderInfo folder, string fileName, bool retrieveUnpublishedFiles)
        {
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("folder", folder);

            return CBO.Instance.FillObject<FileInfo>(DataProvider.Instance().GetFile(fileName, folder.FolderID, retrieveUnpublishedFiles));
        }

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="portalId">The portal ID or Null.NullInteger for the Host.</param>
        /// <param name="relativePath">Relative path to the file.</param>
        /// <remarks>Host and portal settings commonly return a relative path to a file.  This method uses that relative path to fetch file metadata.</remarks>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(int portalId, string relativePath)
        {
            return this.GetFile(portalId, relativePath, false);
        }

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="portalId">The portal ID or Null.NullInteger for the Host.</param>
        /// <param name="relativePath">Relative path to the file.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <remarks>Host and portal settings commonly return a relative path to a file.  This method uses that relative path to fetch file metadata.</remarks>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(int portalId, string relativePath, bool retrieveUnpublishedFiles)
        {
            Requires.NotNullOrEmpty("relativePath", relativePath);

            var folderPath = string.Empty;
            var seperatorIndex = relativePath.LastIndexOf('/');

            if (seperatorIndex > 0)
            {
                folderPath = relativePath.Substring(0, seperatorIndex + 1);
            }

            var folderInfo = FolderManager.Instance.GetFolder(portalId, folderPath);
            if (folderInfo == null)
            {
                return null;
            }

            var fileName = relativePath.Substring(folderPath.Length);
            return this.GetFile(folderInfo, fileName, retrieveUnpublishedFiles);
        }

        /// <summary>
        /// Gets the content of the specified file.
        /// </summary>
        /// <param name="file">The file to get the content from.</param>
        /// <returns>A stream with the content of the file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual Stream GetFileContent(IFileInfo file)
        {
            Requires.NotNull("file", file);

            Stream stream = null;

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);

            if (folderMapping != null)
            {
                try
                {
                    stream = FolderProvider.Instance(folderMapping.FolderProviderType).GetFileStream(file);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);

                    throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception"), ex);
                }
            }

            return stream;
        }

        /// <summary>
        /// Gets a seekable Stream based on the specified non-seekable Stream.
        /// </summary>
        /// <param name="stream">A non-seekable Stream.</param>
        /// <returns>A seekable Stream.</returns>
        public virtual Stream GetSeekableStream(Stream stream)
        {
            Requires.NotNull("stream", stream);

            if (stream.CanSeek)
            {
                return stream;
            }

            var folderPath = this.GetHostMapPath();
            string filePath;

            do
            {
                filePath = Path.Combine(folderPath, Path.GetRandomFileName()) + ".resx";
            }
            while (File.Exists(filePath));

            var fileStream = this.GetAutoDeleteFileStream(filePath);

            var array = new byte[BufferSize];

            int bytesRead;
            while ((bytesRead = stream.Read(array, 0, BufferSize)) > 0)
            {
                fileStream.Write(array, 0, bytesRead);
            }

            fileStream.Position = 0;

            return fileStream;
        }

        /// <summary>
        /// Gets the direct Url to the file.
        /// </summary>
        /// <param name="file">The file to get the Url.</param>
        /// <returns>The direct Url to the file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public string GetUrl(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);

            try
            {
                return FolderProvider.Instance(folderMapping.FolderProviderType).GetFileUrl(file);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception."), ex);
            }
        }

        /// <summary>
        /// Gets a flag that dertermines if the file is an Image.
        /// </summary>
        /// <param name="file">The file to test.</param>
        /// <returns>The flag as a boolean value.</returns>
        public virtual bool IsImageFile(IFileInfo file)
        {
            return (Globals.glbImageFileTypes + ",").IndexOf(file.Extension.ToLowerInvariant().Replace(".", string.Empty) + ",") > -1;
        }

        /// <summary>
        /// Moves the specified file into the specified folder.
        /// </summary>
        /// <param name="file">The file to move.</param>
        /// <param name="destinationFolder">The folder where to move the file to.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file or destinationFolder are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.InvalidFileExtensionException">Thrown when the extension of the specified file is not allowed.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.NoSpaceAvailableException">Thrown when the portal has no space available to store the specified file.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.PermissionsNotMetException">Thrown when permissions are not met.</exception>
        /// <returns>An <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> with the information of the moved file.</returns>
        public virtual IFileInfo MoveFile(IFileInfo file, IFolderInfo destinationFolder)
        {
            Requires.NotNull("file", file);
            Requires.NotNull("destinationFolder", destinationFolder);

            // check whether the file is already in the dest folder.
            if (file.FolderId == destinationFolder.FolderID)
            {
                return file;
            }

            var lockReason = string.Empty;
            if (FileLockingController.Instance.IsFileLocked(file, out lockReason))
            {
                throw new FileLockedException(Localization.GetExceptionMessage(lockReason, "File locked. The file cannot be updated. Reason: " + lockReason));
            }

            // check for existing file
            var existingFile = this.GetFile(destinationFolder, file.FileName, true);
            if (existingFile != null)
            {
                this.DeleteFile(existingFile);
            }

            var destinationFolderMapping = FolderMappingController.Instance.GetFolderMapping(destinationFolder.PortalID, destinationFolder.FolderMappingID);
            var destinationFolderProvider = FolderProvider.Instance(destinationFolderMapping.FolderProviderType);

            var sourceFolderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
            var sourceFolderProvider = FolderProvider.Instance(sourceFolderMapping.FolderProviderType);

            if (destinationFolderMapping.FolderMappingID == sourceFolderMapping.FolderMappingID && destinationFolderProvider.SupportsMoveFile)
            {
                // Implement Move
                destinationFolderProvider.MoveFile(file, destinationFolder);
            }
            else
            {
                // Implement Copy/Delete
                using (var fileContent = this.GetFileContent(file))
                {
                    if (destinationFolderMapping.MappingName == "Database")
                    {
                        destinationFolderProvider.UpdateFile(file, fileContent);
                    }
                    else
                    {
                        this.AddFileToFolderProvider(fileContent, file.FileName, destinationFolder, destinationFolderProvider);
                    }
                }

                this.DeleteFileFromFolderProvider(file, sourceFolderProvider);
            }

            if (file.FolderMappingID == destinationFolder.FolderMappingID)
            {
                this.MoveVersions(file, destinationFolder, sourceFolderProvider, destinationFolderProvider);
            }
            else
            {
                FileVersionController.Instance.DeleteAllUnpublishedVersions(file, true);
            }

            var oldFilePath = file.Folder;
            file.FolderId = destinationFolder.FolderID;
            file.Folder = destinationFolder.FolderPath;
            file.FolderMappingID = destinationFolder.FolderMappingID;
            file = this.UpdateFile(file);

            // Notify File Moved event
            this.OnFileMoved(file, oldFilePath, this.GetCurrentUserID());

            return file;
        }

        /// <summary>
        /// Renames the specified file.
        /// </summary>
        /// <param name="file">The file to rename.</param>
        /// <param name="newFileName">The new filename to assign to the file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FileAlreadyExistsException">Thrown when the folder already contains a file with the same name.</exception>
        /// <returns>An <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> with the information of the renamed file.</returns>
        public virtual IFileInfo RenameFile(IFileInfo file, string newFileName)
        {
            Requires.NotNull("file", file);
            Requires.NotNullOrEmpty("newFileName", newFileName);

            if (file.FileName == newFileName)
            {
                return file;
            }

            if (!this.IsAllowedExtension(newFileName))
            {
                throw new InvalidFileExtensionException(string.Format(Localization.GetExceptionMessage("AddFileExtensionNotAllowed", "The extension '{0}' is not allowed. The file has not been added."), Path.GetExtension(newFileName)));
            }

            if (!this.IsValidFilename(newFileName))
            {
                throw new InvalidFilenameException(string.Format(Localization.GetExceptionMessage("AddFilenameNotAllowed", "The file name '{0}' is not allowed. The file has not been added."), newFileName));
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            if (this.FileExists(folder, newFileName))
            {
                throw new FileAlreadyExistsException(Localization.GetExceptionMessage("RenameFileAlreadyExists", "This folder already contains a file with the same name. The file has not been renamed."));
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);

            try
            {
                FolderProvider.Instance(folderMapping.FolderProviderType).RenameFile(file, newFileName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(Localization.GetExceptionMessage("RenameFileUnderlyingSystemError", "The underlying system threw an exception. The file has not been renamed."), ex);
            }

            var oldfileName = file.FileName;
            file.FileName = newFileName;
            if (Path.HasExtension(newFileName))
            {
                file.Extension = Path.GetExtension(newFileName).Replace(".", string.Empty);
            }

            var renamedFile = this.UpdateFile(file);

            // Notify File Renamed event
            this.OnFileRenamed(renamedFile, oldfileName, this.GetCurrentUserID());

            return renamedFile;
        }

        /// <summary>
        /// Sets the specified FileAttributes of the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="fileAttributes">The file attributes to add.</param>
        public void SetAttributes(IFileInfo file, FileAttributes fileAttributes)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);

            try
            {
                FolderProvider.Instance(folderMapping.FolderProviderType).SetFileAttributes(file, fileAttributes);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception."), ex);
            }
        }

        /// <summary>
        /// Extracts the files and folders contained in the specified zip file to the folder where the file belongs.
        /// </summary>
        /// <param name="file">The file to unzip.</param>
        /// <exception cref="System.ArgumentException">Thrown when file is not a zip compressed file.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when file or destination folder are null.</exception>
        public virtual void UnzipFile(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var destinationFolder = FolderManager.Instance.GetFolder(file.FolderId);

            this.UnzipFile(file, destinationFolder);
        }

        /// <summary>
        /// Extracts the files and folders contained in the specified zip file to the specified folder.
        /// </summary>
        /// <param name="file">The file to unzip.</param>
        /// <param name="destinationFolder">The folder to unzip to.</param>
        /// <exception cref="System.ArgumentException">Thrown when file is not a zip compressed file.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when file or destination folder are null.</exception>
        public virtual void UnzipFile(IFileInfo file, IFolderInfo destinationFolder)
        {
            this.UnzipFile(file, destinationFolder, null);
        }

        /// <summary>
        /// Extracts the files and folders contained in the specified zip file to the specified folder.
        /// </summary>
        /// <param name="file">The file to unzip.</param>
        /// <param name="destinationFolder">The folder to unzip to.</param>
        /// <param name="invalidFiles">Files which can't exact.</param>
        /// <returns>Total files count in the zip file.</returns>
        /// <exception cref="System.ArgumentException">Thrown when file is not a zip compressed file.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when file or destination folder are null.</exception>
        public virtual int UnzipFile(IFileInfo file, IFolderInfo destinationFolder, IList<string> invalidFiles)
        {
            Requires.NotNull("file", file);
            Requires.NotNull("destinationFolder", destinationFolder);

            if (file.Extension != "zip")
            {
                throw new ArgumentException(Localization.GetExceptionMessage("InvalidZipFile", "The file specified is not a zip compressed file."));
            }

            return this.ExtractFiles(file, destinationFolder, invalidFiles);
        }

        /// <summary>
        /// Updates the metadata of the specified file.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.InvalidMetadataValuesException">Thrown when the file metadata are not valid.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as the updated file.</returns>
        public virtual IFileInfo UpdateFile(IFileInfo file)
        {
            Requires.NotNull("file", file);
            string message;
            if (!ValidMetadata(file, out message))
            {
                throw new InvalidMetadataValuesException(message);
            }

            return this.UpdateFile(file, true);
        }

        /// <summary>
        /// Regenerates the hash and updates the metadata of the specified file.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="fileContent">Stream used to regenerate the hash.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as the updated file.</returns>
        public virtual IFileInfo UpdateFile(IFileInfo file, Stream fileContent)
        {
            Requires.NotNull("file", file);

            if (fileContent != null)
            {
                if (this.IsImageFile(file))
                {
                    Image image = null;

                    try
                    {
                        image = this.GetImageFromStream(fileContent);

                        file.Width = image.Width;
                        file.Height = image.Height;
                    }
                    catch
                    {
                        file.ContentType = "application/octet-stream";
                    }
                    finally
                    {
                        if (image != null)
                        {
                            image.Dispose();
                        }
                    }
                }

                file.SHA1Hash = FolderProvider.Instance(FolderMappingController.Instance.GetFolderMapping(file.FolderMappingID).FolderProviderType).GetHashCode(file, fileContent);
            }

            // Get file size from folder provider.
            try
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
                if (folderMapping != null)
                {
                    var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
                    file.Size = (int)folderProvider.GetFileSize(file);
                    file.LastModificationTime = folderProvider.GetLastModificationTime(file);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return this.UpdateFile(file);
        }

        /// <summary>
        /// Writes the content of the specified file into the specified stream.
        /// </summary>
        /// <param name="file">The file to write into the stream.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file or stream are null.</exception>
        public virtual void WriteFile(IFileInfo file, Stream stream)
        {
            Requires.NotNull("file", file);
            Requires.NotNull("stream", stream);

            using (var srcStream = this.GetFileContent(file))
            {
                const int bufferSize = 4096;
                var buffer = new byte[bufferSize];

                int bytesRead;
                while ((bytesRead = srcStream.Read(buffer, 0, bufferSize)) > 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
            }
        }

        /// <summary>
        /// Downloads the specified file.
        /// </summary>
        /// <param name="file">The file to download.</param>
        /// <param name="contentDisposition">Indicates how to display the document once downloaded.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.PermissionsNotMetException">Thrown when permissions are not met.</exception>
        public virtual void WriteFileToResponse(IFileInfo file, ContentDisposition contentDisposition)
        {
            Requires.NotNull("file", file);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            if (!FolderPermissionController.Instance.CanViewFolder(folder))
            {
                throw new PermissionsNotMetException(Localization.GetExceptionMessage("WriteFileToResponsePermissionsNotMet", "Permissions are not met. The file cannot be downloaded."));
            }

            if (this.IsFileAutoSyncEnabled())
            {
                this.AutoSyncFile(file);
            }

            this.WriteFileToHttpContext(file, contentDisposition);
        }

        internal virtual int CopyContentItem(int contentItemId)
        {
            if (contentItemId == Null.NullInteger)
            {
                return Null.NullInteger;
            }

            var newContentItem = this.CreateFileContentItem();

            // Clone terms
            var termController = new TermController();
            foreach (var term in termController.GetTermsByContent(contentItemId))
            {
                termController.AddTermToContent(term, newContentItem);
            }

            return newContentItem.ContentItemId;
        }

        internal virtual ContentItem CreateFileContentItem()
        {
            var typeController = new ContentTypeController();
            var contentTypeFile = (from t in typeController.GetContentTypes() where t.ContentType == "File" select t).SingleOrDefault();

            if (contentTypeFile == null)
            {
                contentTypeFile = new ContentType { ContentType = "File" };
                contentTypeFile.ContentTypeId = typeController.AddContentType(contentTypeFile);
            }

            var objContent = new ContentItem
            {
                ContentTypeId = contentTypeFile.ContentTypeId,
                Indexed = false,
            };

            objContent.ContentItemId = Util.GetContentController().AddContentItem(objContent);

            return objContent;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void MoveVersions(IFileInfo file, IFolderInfo destinationFolder, FolderProvider sourceFolderProvider, FolderProvider destinationFolderProvider)
        {
            var versions = FileVersionController.Instance.GetFileVersions(file).ToArray();
            if (!versions.Any())
            {
                return;
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            foreach (var version in versions)
            {
                // Get Version File
                using (var fileContent = sourceFolderProvider.GetFileStream(folder, version.FileName))
                {
                    // This scenario is when the file is in the Database Folder Provider
                    if (fileContent == null)
                    {
                        continue;
                    }

                    this.AddFileToFolderProvider(fileContent, version.FileName, destinationFolder, destinationFolderProvider);
                }

                var fileVersion = new FileInfo
                {
                    FileName = version.FileName,
                    Folder = file.Folder,
                    FolderMappingID = file.FolderMappingID,
                    PortalId = folder.PortalID,
                };

                this.DeleteFileFromFolderProvider(fileVersion, sourceFolderProvider);
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void AutoSyncFile(IFileInfo file)
        {
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            if (folderProvider.FileExists(folder, file.FileName))
            {
                var newFileSize = folderProvider.GetFileSize(file);
                if (file.Size != newFileSize)
                {
                    using (var fileContent = this.GetFileContent(file))
                    {
                        this.UpdateFile(file, fileContent);
                    }
                }
            }
            else
            {
                this.DeleteFile(file);
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual int ExtractFiles(IFileInfo file, IFolderInfo destinationFolder, IList<string> invalidFiles)
        {
            var folderManager = FolderManager.Instance;

            ZipInputStream zipInputStream = null;

            if (invalidFiles == null)
            {
                invalidFiles = new List<string>();
            }

            var exactFilesCount = 0;

            try
            {
                using (var fileContent = this.GetFileContent(file))
                {
                    zipInputStream = new ZipInputStream(fileContent);

                    var zipEntry = zipInputStream.GetNextEntry();

                    while (zipEntry != null)
                    {
                        zipEntry.CheckZipEntry();
                        if (!zipEntry.IsDirectory)
                        {
                            exactFilesCount++;
                            var fileName = Path.GetFileName(zipEntry.Name);

                            this.EnsureZipFolder(zipEntry.Name, destinationFolder);

                            IFolderInfo parentFolder;
                            if (zipEntry.Name.IndexOf("/") == -1)
                            {
                                parentFolder = destinationFolder;
                            }
                            else
                            {
                                var folderPath = destinationFolder.FolderPath + zipEntry.Name.Substring(0, zipEntry.Name.LastIndexOf("/") + 1);
                                parentFolder = folderManager.GetFolder(file.PortalId, folderPath);
                            }

                            try
                            {
                                this.AddFile(parentFolder, fileName, zipInputStream, true);
                            }
                            catch (PermissionsNotMetException exc)
                            {
                                Logger.Warn(exc);
                            }
                            catch (NoSpaceAvailableException exc)
                            {
                                Logger.Warn(exc);
                            }
                            catch (InvalidFileExtensionException exc)
                            {
                                invalidFiles.Add(zipEntry.Name);
                                Logger.Warn(exc);
                            }
                            catch (Exception exc)
                            {
                                Logger.Error(exc);
                            }
                        }

                        zipEntry = zipInputStream.GetNextEntry();
                    }
                }
            }
            finally
            {
                if (zipInputStream != null)
                {
                    zipInputStream.Close();
                    zipInputStream.Dispose();
                }
            }

            return exactFilesCount;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal void EnsureZipFolder(string fileName, IFolderInfo destinationFolder)
        {
            var folderManager = FolderManager.Instance;

            var folderMappingController = FolderMappingController.Instance;
            var folderMapping = folderMappingController.GetFolderMapping(destinationFolder.PortalID, destinationFolder.FolderMappingID);

            if (fileName.LastIndexOf('/') == -1)
            {
                return;
            }

            var zipFolder = fileName.Substring(0, fileName.LastIndexOf('/'));

            var folderPath = PathUtils.Instance.RemoveTrailingSlash(zipFolder);

            if (folderPath.IndexOf("/") == -1)
            {
                var newFolderPath = destinationFolder.FolderPath + PathUtils.Instance.FormatFolderPath(folderPath);
                if (!folderManager.FolderExists(destinationFolder.PortalID, newFolderPath))
                {
                    folderManager.AddFolder(folderMapping, newFolderPath);
                }
            }
            else
            {
                var zipFolders = folderPath.Split('/');

                var parentFolder = destinationFolder;

                for (var i = 0; i < zipFolders.Length; i++)
                {
                    var newFolderPath = parentFolder.FolderPath + PathUtils.Instance.FormatFolderPath(zipFolders[i]);
                    if (!folderManager.FolderExists(destinationFolder.PortalID, newFolderPath))
                    {
                        folderManager.AddFolder(folderMappingController.GetFolderMapping(parentFolder.PortalID, parentFolder.FolderMappingID), newFolderPath);
                    }

                    parentFolder = folderManager.GetFolder(destinationFolder.PortalID, newFolderPath);
                }
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual Stream GetAutoDeleteFileStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, BufferSize, FileOptions.DeleteOnClose);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual int GetCurrentUserID()
        {
            return UserController.Instance.GetCurrentUserInfo().UserID;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns>SHA1 hash of the file.</returns>
        internal virtual string GetHash(Stream stream)
        {
            Requires.NotNull("stream", stream);
            var hashText = new StringBuilder();
            using (var hasher = SHA1.Create())
            {
                var hashData = hasher.ComputeHash(stream);
                foreach (var b in hashData)
                {
                    hashText.Append(b.ToString("x2"));
                }
            }

            return hashText.ToString();
        }

        /// <summary>
        /// Gets the hash of a file.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        /// <returns>SHA1 hash of the file.</returns>
        internal virtual string GetHash(IFileInfo fileInfo)
        {
            return FolderProvider.Instance(FolderMappingController.Instance.GetFolderMapping(fileInfo.FolderMappingID).FolderProviderType).GetHashCode(fileInfo);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual string GetHostMapPath()
        {
            return TestableGlobals.Instance.HostMapPath;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual Image GetImageFromStream(Stream stream)
        {
            return Image.FromStream(stream);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual Globals.PerformanceSettings GetPerformanceSetting()
        {
            return Host.PerformanceSetting;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual bool IsAllowedExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            // regex matches a dot followed by 1 or more chars followed by a semi-colon
            // regex is meant to block files like "foo.asp;.png" which can take advantage
            // of a vulnerability in IIS6 which treasts such files as .asp, not .png
            return !string.IsNullOrEmpty(extension)
                   && this.WhiteList.IsAllowedExtension(extension)
                   && !Globals.FileExtensionRegex.IsMatch(fileName);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual bool IsValidFilename(string fileName)
        {
            // regex ensures the file is a valid filename and doesn't include illegal characters
            return Globals.FileValidNameRegex.IsMatch(fileName);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual bool IsFileAutoSyncEnabled()
        {
            return Host.EnableFileAutoSync;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void WriteFileToHttpContext(IFileInfo file, ContentDisposition contentDisposition)
        {
            var scriptTimeOut = HttpContext.Current.Server.ScriptTimeout;

            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;
            var objResponse = HttpContext.Current.Response;

            objResponse.ClearContent();
            objResponse.ClearHeaders();

            switch (contentDisposition)
            {
                case ContentDisposition.Attachment:
                    objResponse.AppendHeader("content-disposition", "attachment; filename=\"" + file.FileName + "\"");
                    break;
                case ContentDisposition.Inline:
                    objResponse.AppendHeader("content-disposition", "inline; filename=\"" + file.FileName + "\"");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("contentDisposition");
            }

            // Do not send negative Content-Length (file.Size could be negative due to integer overflow for files > 2GB)
            if (file.Size >= 0)
            {
                objResponse.AppendHeader("Content-Length", file.Size.ToString(CultureInfo.InvariantCulture));
            }

            objResponse.ContentType = this.GetContentType(file.Extension.Replace(".", string.Empty));

            try
            {
                using (var fileContent = this.GetFileContent(file))
                {
                    this.WriteStream(objResponse, fileContent);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                objResponse.Write("Error : " + ex.Message);
            }

            objResponse.Flush();
            objResponse.End();

            HttpContext.Current.Server.ScriptTimeout = scriptTimeOut;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void WriteStream(HttpResponse objResponse, Stream objStream)
        {
            var bytBuffer = new byte[10000];
            try
            {
                if (objResponse.IsClientConnected)
                {
                    var intLength = objStream.Read(bytBuffer, 0, 10000);

                    while (objResponse.IsClientConnected && intLength > 0)
                    {
                        objResponse.OutputStream.Write(bytBuffer, 0, intLength);
                        objResponse.Flush();

                        intLength = objStream.Read(bytBuffer, 0, 10000);
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

        /// <summary>
        /// Update file info to database.
        /// </summary>
        /// <param name="file">File info.</param>
        /// <param name="updateLazyload">Whether to update the lazy load properties: Width, Height, Sha1Hash.</param>
        /// <returns>The file info.</returns>
        internal virtual IFileInfo UpdateFile(IFileInfo file, bool updateLazyload)
        {
            // By default File Events will be fired
            return this.UpdateFile(file, updateLazyload, true);
        }

        /// <summary>
        /// Update file info to database.
        /// </summary>
        /// <param name="file">File info.</param>
        /// <param name="updateLazyload">Whether to update the lazy load properties: Width, Height, Sha1Hash.</param>
        /// <param name="fireEvent">Whether to fire File events or not.</param>
        /// <returns>The file info.</returns>
        internal virtual IFileInfo UpdateFile(IFileInfo file, bool updateLazyload, bool fireEvent)
        {
            Requires.NotNull("file", file);

            DataProvider.Instance().UpdateFile(
                file.FileId,
                file.VersionGuid,
                file.FileName,
                file.Extension,
                file.Size,
                updateLazyload ? file.Width : Null.NullInteger,
                updateLazyload ? file.Height : Null.NullInteger,
                file.ContentType,
                file.FolderId,
                this.GetCurrentUserID(),
                updateLazyload ? file.SHA1Hash : Null.NullString,
                file.LastModificationTime,
                file.Title,
                file.Description,
                file.StartDate,
                file.EndDate,
                file.EnablePublishPeriod,
                file.ContentItemID);

            DataCache.RemoveCache("GetFileById" + file.FileId);
            this.ClearFolderCache(file.PortalId);
            var updatedFile = this.GetFile(file.FileId);

            if (fireEvent)
            {
                this.OnFileMetadataChanged(updatedFile ?? this.GetFile(file.FileId, true), this.GetCurrentUserID());
            }

            return updatedFile;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void ClearFolderCache(int portalId)
        {
            DataCache.ClearFolderCache(portalId);
        }

        private static ImageFormat GetImageFormat(Image img)
        {
            if (img.RawFormat.Equals(ImageFormat.Jpeg))
            {
                return ImageFormat.Jpeg;
            }

            if (img.RawFormat.Equals(ImageFormat.Bmp))
            {
                return ImageFormat.Bmp;
            }

            if (img.RawFormat.Equals(ImageFormat.Png))
            {
                return ImageFormat.Png;
            }

            if (img.RawFormat.Equals(ImageFormat.Emf))
            {
                return ImageFormat.Emf;
            }

            if (img.RawFormat.Equals(ImageFormat.Exif))
            {
                return ImageFormat.Exif;
            }

            if (img.RawFormat.Equals(ImageFormat.Gif))
            {
                return ImageFormat.Gif;
            }

            if (img.RawFormat.Equals(ImageFormat.Icon))
            {
                return ImageFormat.Icon;
            }

            if (img.RawFormat.Equals(ImageFormat.MemoryBmp))
            {
                return ImageFormat.Jpeg;
            }

            if (img.RawFormat.Equals(ImageFormat.Tiff))
            {
                return ImageFormat.Tiff;
            }
            else
            {
                return ImageFormat.Wmf;
            }
        }

        private static Stream ToStream(Image image, ImageFormat formaw)
        {
            var stream = new MemoryStream();
            image.Save(stream, formaw);
            stream.Position = 0;
            return stream;
        }

        // Match the orientation code to the correct rotation:
        private static RotateFlipType OrientationToFlipType(string orientation)
        {
            switch (int.Parse(orientation))
            {
                case 1:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
                default:
                    return RotateFlipType.RotateNoneFlipNone;
            }
        }

        private static bool ValidMetadata(IFileInfo file, out string exceptionMessage)
        {
            exceptionMessage = string.Empty;

            // TODO check dynamically all required fields from MetadataInfo

            // TODO check dynamically all max lengths from MetadataInfo
            // TODO Use the MaxLength from MetadataInfo
            if (!string.IsNullOrEmpty(file.Title) && file.Title.Length > 256)
            {
                exceptionMessage = Localization.GetExceptionMessage("MaxLengthExceeded", "The maximum length of the field {0} has been exceeded", DefaultMetadataNames.Title);
                return false;
            }

            if (file.StartDate == null || file.StartDate == Null.NullDate)
            {
                exceptionMessage = Localization.GetExceptionMessage("StartDateRequired", "The Start Date is required");
                return false;
            }

            var savedFile = FileManager.Instance.GetFile(file.FileId);
            if (file.StartDate < file.CreatedOnDate.Date && file.StartDate != savedFile.StartDate)
            {
                exceptionMessage = Localization.GetExceptionMessage("StartDateMustNotBeInThePast", "The Start Date must not be in the past");
                return false;
            }

            if (file.EndDate != Null.NullDate && file.StartDate > file.EndDate)
            {
                exceptionMessage = Localization.GetExceptionMessage("InvalidPublishPeriod", "The End Date must be after the Start Date");
                return false;
            }

            return true;
        }

        private void AddFileToFolderProvider(Stream fileContent, string fileName, IFolderInfo destinationFolder, FolderProvider provider)
        {
            try
            {
                if (!fileContent.CanSeek)
                {
                    using (var seekableStream = this.GetSeekableStream(fileContent))
                    {
                        provider.AddFile(destinationFolder, fileName, seekableStream);
                    }
                }
                else
                {
                    provider.AddFile(destinationFolder, fileName, fileContent);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception."), ex);
            }
        }

        private void DeleteFileFromFolderProvider(IFileInfo file, FolderProvider provider)
        {
            try
            {
                // We can't delete the file until the fileContent resource has been released
                provider.DeleteFile(file);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception."), ex);
            }
        }

        private void OnFileDeleted(IFileInfo fileInfo, int userId)
        {
            EventManager.Instance.OnFileDeleted(new FileDeletedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
                IsCascadeDeleting = false,
            });
        }

        private void OnFileRenamed(IFileInfo fileInfo, string oldFileName, int userId)
        {
            EventManager.Instance.OnFileRenamed(new FileRenamedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
                OldFileName = oldFileName,
            });
        }

        private void OnFileMoved(IFileInfo fileInfo, string oldFilePath, int userId)
        {
            EventManager.Instance.OnFileMoved(new FileMovedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
                OldFilePath = oldFilePath,
            });
        }

        private void OnFileOverwritten(IFileInfo fileInfo, int userId)
        {
            EventManager.Instance.OnFileOverwritten(new FileChangedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
            });
        }

        private void OnFileMetadataChanged(IFileInfo fileInfo, int userId)
        {
            EventManager.Instance.OnFileMetadataChanged(new FileChangedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
            });
        }

        private void OnFileAdded(IFileInfo fileInfo, IFolderInfo folderInfo, int userId)
        {
            EventManager.Instance.OnFileAdded(new FileAddedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
                FolderInfo = folderInfo,
            });
        }

        /// <summary>
        /// Rotate/Flip the image as per the metadata and reset the metadata.
        /// </summary>
        /// <param name="content"></param>
        private void RotateFlipImage(ref Stream content)
        {
            try
            {
                using (var image = this.GetImageFromStream(content))
                {
                    if (!image.PropertyIdList.Any(x => x == 274))
                    {
                        return;
                    }

                    var orientation = image.GetPropertyItem(274); // Find rotation/flip meta property
                    if (orientation == null)
                    {
                        return;
                    }

                    var flip = OrientationToFlipType(orientation.Value[0].ToString());
                    if (flip == RotateFlipType.RotateNoneFlipNone)
                    {
                        return; // No rotation or flip required
                    }

                    image.RotateFlip(flip);
                    var newOrientation = new byte[2];
                    newOrientation[0] = 1; // little Endian
                    newOrientation[1] = 0;
                    orientation.Value = newOrientation;
                    image.SetPropertyItem(orientation);
                    content = ToStream(image, GetImageFormat(image));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void CheckFileAddingRestrictions(IFolderInfo folder, string fileName, bool checkPermissions,
            bool ignoreWhiteList)
        {
            if (checkPermissions && !FolderPermissionController.Instance.CanAddFolder(folder))
            {
                throw new PermissionsNotMetException(Localization.GetExceptionMessage(
                    "AddFilePermissionsNotMet",
                    "Permissions are not met. The file has not been added."));
            }

            if (!this.IsAllowedExtension(fileName) && !(UserController.Instance.GetCurrentUserInfo().IsSuperUser && ignoreWhiteList))
            {
                throw new InvalidFileExtensionException(
                    string.Format(
                        Localization.GetExceptionMessage(
                            "AddFileExtensionNotAllowed",
                            "The extension '{0}' is not allowed. The file has not been added."), Path.GetExtension(fileName)));
            }

            if (!this.IsValidFilename(fileName))
            {
                throw new InvalidFilenameException(
                    string.Format(
                        Localization.GetExceptionMessage(
                            "AddFilenameNotAllowed",
                            "The file name '{0}' is not allowed. The file has not been added."), fileName));
            }
        }

        private void NotifyFileAddingEvents(IFolderInfo folder, int createdByUserID, bool fileExists, Workflow folderWorkflow, IFileInfo file)
        {
            // Notify file event
            if (fileExists &&
                (folderWorkflow == null || folderWorkflow.WorkflowID == SystemWorkflowManager.Instance.GetDirectPublishWorkflow(folderWorkflow.PortalID).WorkflowID))
            {
                this.OnFileOverwritten(file, createdByUserID);
            }

            if (!fileExists)
            {
                this.OnFileAdded(file, folder, createdByUserID);
            }
        }

        private void SetContentItem(IFileInfo file)
        {
            // Create Content Item if does not exists
            if (file.ContentItemID == Null.NullInteger)
            {
                file.ContentItemID = this.CreateFileContentItem().ContentItemId;
            }
        }

        private void SetInitialFileMetadata(ref Stream fileContent, FileInfo file, FolderProvider folderProvider)
        {
            file.Size = (int)fileContent.Length;
            var fileHash = folderProvider.GetHashCode(file, fileContent);
            file.SHA1Hash = fileHash;
            fileContent.Position = 0;

            file.Width = 0;
            file.Height = 0;

            if (this.IsImageFile(file))
            {
                this.RotateFlipImage(ref fileContent);
                this.SetImageProperties(file, fileContent);
            }
        }

        private void SetImageProperties(IFileInfo file, Stream fileContent)
        {
            try
            {
                using (var image = this.GetImageFromStream(fileContent))
                {
                    file.Width = image.Width;
                    file.Height = image.Height;
                }
            }
            catch
            {
                file.ContentType = "application/octet-stream";
            }
            finally
            {
                fileContent.Position = 0;
            }
        }

        private void CheckFileWritingRestrictions(IFolderInfo folder, string fileName, Stream fileContent, IFileInfo oldFile, int createdByUserId)
        {
            if (!PortalController.Instance.HasSpaceAvailable(folder.PortalID, fileContent.Length))
            {
                throw new NoSpaceAvailableException(
                    Localization.GetExceptionMessage(
                        "AddFileNoSpaceAvailable",
                        "The portal has no space available to store the specified file. The file has not been added."));
            }

            // Publish Period
            if (oldFile != null && FileLockingController.Instance.IsFileOutOfPublishPeriod(oldFile, folder.PortalID, createdByUserId))
            {
                throw new FileLockedException(
                                Localization.GetExceptionMessage(
                                    "FileLockedOutOfPublishPeriodError",
                                    "File locked. The file cannot be updated because it is out of Publish Period"));
            }

            if (!FileSecurityController.Instance.Validate(fileName, fileContent))
            {
                var defaultMessage = "The content of '{0}' is not valid. The file has not been added.";
                var errorMessage = Localization.GetExceptionMessage("AddFileInvalidContent", defaultMessage);
                throw new InvalidFileContentException(string.Format(errorMessage, fileName));
            }
        }

        private void ManageFileAdding(int createdByUserID, Workflow folderWorkflow, bool fileExists, FileInfo file)
        {
            if (folderWorkflow == null || !fileExists)
            {
                this.AddFile(file, createdByUserID);
            }
            else
            {
                // File Events for updating will not be fired. Only events for adding nust be fired
                this.UpdateFile(file, true, false);
            }

            if (folderWorkflow != null && this.StartWorkflow(createdByUserID, folderWorkflow, fileExists, file.ContentItemID))
            {
                if (!fileExists) // if file exists it could have been published. So We don't have to update the field
                {
                    // Maybe here we can set HasBeenPublished as 0
                    DataProvider.Instance().SetFileHasBeenPublished(file.FileId, false);
                }
            }
        }

        private void AddFile(IFileInfo file, int createdByUserID)
        {
            file.FileId = DataProvider.Instance().AddFile(
                file.PortalId,
                file.UniqueId,
                file.VersionGuid,
                file.FileName,
                file.Extension,
                file.Size,
                file.Width,
                file.Height,
                file.ContentType,
                file.Folder,
                file.FolderId,
                createdByUserID,
                file.SHA1Hash,
                file.LastModificationTime,
                file.Title,
                file.Description,
                file.StartDate,
                file.EndDate,
                file.EnablePublishPeriod,
                file.ContentItemID);
        }

        private string ProcessVersioning(IFolderInfo folder, IFileInfo oldFile, IFileInfo file, int createdByUserID)
        {
            if (oldFile != null && FileVersionController.Instance.IsFolderVersioned(folder) && oldFile.SHA1Hash != file.SHA1Hash)
            {
                return FileVersionController.Instance.AddFileVersion(oldFile, createdByUserID);
            }

            return file.FileName;
        }

        private bool CanUpdateWhenApproving(IFolderInfo folder, ContentItem item, int createdByUserID)
        {
            if (WorkflowEngine.Instance.IsWorkflowOnDraft(item.ContentItemId))
            {
                ////We assume User can add content to folder
                return true;
            }

            return WorkflowSecurity.Instance.HasStateReviewerPermission(folder.PortalID, createdByUserID, item.StateID);
        }

        private bool StartWorkflow(int createdByUserID, Workflow folderWorkflow, bool fileExists, int contentItemID)
        {
            if (WorkflowEngine.Instance.IsWorkflowCompleted(contentItemID))
            {
                WorkflowEngine.Instance.StartWorkflow(folderWorkflow.WorkflowID, contentItemID, createdByUserID);
                return true;
            }

            return false;
        }

        private string UpdateWhileApproving(IFolderInfo folder, int createdByUserID, IFileInfo file, IFileInfo oldFile, Stream content)
        {
            var contentController = new ContentController();
            bool workflowCompleted = WorkflowEngine.Instance.IsWorkflowCompleted(file.ContentItemID);

            var isDatabaseMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID).MappingName == "Database";

            // If the file does not exist, then the field would not has value.
            // Currently, first upload has not version file
            if (oldFile == null || !oldFile.HasBeenPublished)
            {
                return file.FileName;
            }

            if (workflowCompleted) // We assume User can add content to folder
            {
                return isDatabaseMapping ? FileVersionController.Instance.AddFileVersion(file, createdByUserID, false, false, content) : FileVersionController.Instance.AddFileVersion(file, createdByUserID, false);
            }

            if (this.CanUpdateWhenApproving(folder, contentController.GetContentItem(file.ContentItemID), createdByUserID))
            {
                // Update the Unpublished version
                var versions = FileVersionController.Instance.GetFileVersions(file).ToArray();
                if (versions.Any())
                {
                    FileVersionController.Instance.DeleteFileVersion(file, versions.OrderByDescending(f => f.Version).FirstOrDefault().Version);
                }

                return isDatabaseMapping ? FileVersionController.Instance.AddFileVersion(file, createdByUserID, false, false, content) : FileVersionController.Instance.AddFileVersion(file, createdByUserID, false);
            }

            throw new FileLockedException(
                Localization.GetExceptionMessage(
                    "FileLockedRunningWorkflowError",
                    "File locked. The file cannot be updated because it has a running workflow"));
        }
    }
}
