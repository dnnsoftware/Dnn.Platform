// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem.EventArgs;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Log.EventLog;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// Exposes methods to manage folders.
    /// </summary>
    public class FolderManager : ComponentBase<IFolderManager, FolderManager>, IFolderManager
    {
        private const string DefaultUsersFoldersPath = "Users";
        private const string DefaultMappedPathSetting = "DefaultMappedPath";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FolderManager));
        private static readonly Dictionary<int, SyncFolderData> SyncFoldersData = new Dictionary<int, SyncFolderData>();
        private static readonly object _threadLocker = new object();

        public virtual string MyFolderName
        {
            get
            {
                return Localization.GetString("MyFolderName");
            }
        }

        /// <summary>
        /// Creates a new folder using the provided folder path.
        /// </summary>
        /// <param name="folderMapping">The folder mapping to use.</param>
        /// <param name="folderPath">The path of the new folder.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folderPath or folderMapping are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <returns>The added folder.</returns>
        public virtual IFolderInfo AddFolder(FolderMappingInfo folderMapping, string folderPath)
        {
            return this.AddFolder(folderMapping, folderPath, folderPath);
        }

        /// <summary>
        /// Creates a new folder using the provided folder path and mapping.
        /// </summary>
        /// <param name="folderMapping">The folder mapping to use.</param>
        /// <param name="folderPath">The path of the new folder.</param>
        /// <param name="mappedPath">The mapped path of the new folder.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folderPath or folderMapping are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <returns>The added folder.</returns>
        public virtual IFolderInfo AddFolder(FolderMappingInfo folderMapping, string folderPath, string mappedPath)
        {
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            folderPath = folderPath.Trim();

            if (this.FolderExists(folderMapping.PortalID, folderPath))
            {
                throw new FolderAlreadyExistsException(Localization.GetExceptionMessage("AddFolderAlreadyExists", "The provided folder path already exists. The folder has not been added."));
            }

            if (!this.IsValidFolderPath(folderPath))
            {
                throw new InvalidFolderPathException(Localization.GetExceptionMessage("AddFolderNotAllowed", "The folder path '{0}' is not allowed. The folder has not been added.", folderPath));
            }

            var parentFolder = this.GetParentFolder(folderMapping.PortalID, folderPath);
            if (parentFolder != null)
            {
                var parentFolderMapping = FolderMappingController.Instance.GetFolderMapping(
                    parentFolder.PortalID,
                    parentFolder.FolderMappingID);
                if (FolderProvider.Instance(parentFolderMapping.FolderProviderType).SupportsMappedPaths)
                {
                    folderMapping = parentFolderMapping;
                    mappedPath = string.IsNullOrEmpty(parentFolder.FolderPath) ? PathUtils.Instance.FormatFolderPath(parentFolder.MappedPath + folderPath)
                                                                            : PathUtils.Instance.FormatFolderPath(parentFolder.MappedPath + folderPath.Replace(parentFolder.FolderPath, string.Empty));
                }
                else if (!FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMappedPaths)
                {
                    mappedPath = folderPath;
                }
                else
                {
                    // Parent foldermapping DOESN'T support mapped path
                    // abd current foldermapping YES support mapped path
                    mappedPath = PathUtils.Instance.FormatFolderPath(this.GetDefaultMappedPath(folderMapping) + mappedPath);
                }
            }
            else if (FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMappedPaths)
            {
                mappedPath = PathUtils.Instance.FormatFolderPath(this.GetDefaultMappedPath(folderMapping) + mappedPath);
            }

            try
            {
                FolderProvider.Instance(folderMapping.FolderProviderType).AddFolder(folderPath, folderMapping, mappedPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(Localization.GetExceptionMessage("AddFolderUnderlyingSystemError", "The underlying system threw an exception. The folder has not been added."), ex);
            }

            this.CreateFolderInFileSystem(PathUtils.Instance.GetPhysicalPath(folderMapping.PortalID, folderPath));
            var folderId = this.CreateFolderInDatabase(folderMapping.PortalID, folderPath, folderMapping.FolderMappingID, mappedPath);

            var folder = this.GetFolder(folderId);

            // Notify add folder event
            this.OnFolderAdded(folder, this.GetCurrentUserId());

            return folder;
        }

        /// <summary>
        /// Creates a new folder in the given portal using the provided folder path.
        /// The same mapping than the parent folder will be used to create this folder. So this method have to be used only to create subfolders.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="folderPath">The path of the new folder.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folderPath is null or empty.</exception>
        /// <returns>The added folder.</returns>
        public virtual IFolderInfo AddFolder(int portalId, string folderPath)
        {
            Requires.NotNullOrEmpty("folderPath", folderPath);

            folderPath = PathUtils.Instance.FormatFolderPath(folderPath);

            var parentFolderPath = folderPath.Substring(0, folderPath.Substring(0, folderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
            var parentFolder = this.GetFolder(portalId, parentFolderPath) ?? this.AddFolder(portalId, parentFolderPath);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, parentFolder.FolderMappingID);

            return this.AddFolder(folderMapping, folderPath);
        }

        /// <summary>
        /// Deletes the specified folder.
        /// </summary>
        /// <param name="folder">The folder to delete.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual void DeleteFolder(IFolderInfo folder)
        {
            this.DeleteFolderInternal(folder, false);
        }

        public virtual void UnlinkFolder(IFolderInfo folder)
        {
            this.DeleteFolderRecursive(folder, new Collection<IFolderInfo>(), true, true);
        }

        /// <summary>
        /// Deletes the specified folder.
        /// </summary>
        /// <param name="folderId">The folder identifier.</param>
        public virtual void DeleteFolder(int folderId)
        {
            var folder = this.GetFolder(folderId);

            this.DeleteFolder(folder);
        }

        /// <summary>
        /// Deletes the specified folder and all its content.
        /// </summary>
        /// <param name="folder">The folder to delete.</param>
        /// <param name="notDeletedSubfolders">A collection with all not deleted subfolders after processiong the action.</param>
        public void DeleteFolder(IFolderInfo folder, ICollection<IFolderInfo> notDeletedSubfolders)
        {
            this.DeleteFolderRecursive(folder, notDeletedSubfolders, true, this.GetOnlyUnmap(folder));
        }

        /// <summary>
        /// Checks the existence of the specified folder in the specified portal.
        /// </summary>
        /// <param name="portalId">The portal where to check the existence of the folder.</param>
        /// <param name="folderPath">The path of folder to check the existence of.</param>
        /// <returns>A bool value indicating whether the folder exists or not in the specified portal.</returns>
        public virtual bool FolderExists(int portalId, string folderPath)
        {
            Requires.PropertyNotNull("folderPath", folderPath);

            return this.GetFolder(portalId, folderPath) != null;
        }

        /// <summary>
        /// Gets the files contained in the specified folder.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> GetFiles(IFolderInfo folder)
        {
            return this.GetFiles(folder, false);
        }

        /// <summary>
        /// Gets the files contained in the specified folder.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="recursive">Whether or not to include all the subfolders.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, bool recursive)
        {
            return this.GetFiles(folder, recursive, false);
        }

        /// <summary>
        /// Gets the files contained in the specified folder.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="recursive">Whether or not to include all the subfolders.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, bool recursive, bool retrieveUnpublishedFiles)
        {
            Requires.NotNull("folder", folder);

            return CBO.Instance.FillCollection<FileInfo>(DataProvider.Instance().GetFiles(folder.FolderID, retrieveUnpublishedFiles, recursive));
        }

        /// <summary>
        /// Gets the list of Standard folders the specified user has the provided permissions.
        /// </summary>
        /// <param name="user">The user info.</param>
        /// <param name="permissions">The permissions the folders have to met.</param>
        /// <returns>The list of Standard folders the specified user has the provided permissions.</returns>
        /// <remarks>This method is used to support legacy behaviours and situations where we know the file/folder is in the file system.</remarks>
        public virtual IEnumerable<IFolderInfo> GetFileSystemFolders(UserInfo user, string permissions)
        {
            var userFolders = new List<IFolderInfo>();

            var portalId = user.PortalID;

            var userFolder = this.GetUserFolder(user);

            var defaultFolderMaping = FolderMappingController.Instance.GetDefaultFolderMapping(portalId);

            var folders = this.GetFolders(portalId, permissions, user.UserID).Where(f => f.FolderPath != null && f.FolderMappingID == defaultFolderMaping.FolderMappingID);

            foreach (var folder in folders)
            {
                if (folder.FolderPath.StartsWith(DefaultUsersFoldersPath + "/", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (folder.FolderID == userFolder.FolderID)
                    {
                        folder.DisplayPath = this.MyFolderName + "/";
                        folder.DisplayName = this.MyFolderName;
                    }
                    else if (!folder.FolderPath.StartsWith(userFolder.FolderPath, StringComparison.InvariantCultureIgnoreCase)) // Allow UserFolder children
                    {
                        continue;
                    }
                }

                userFolders.Add(folder);
            }

            return userFolders;
        }

        /// <summary>
        /// Gets a folder entity by providing a folder identifier.
        /// </summary>
        /// <param name="folderId">The identifier of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        public virtual IFolderInfo GetFolder(int folderId)
        {
            // Try and get the folder from the portal cache
            IFolderInfo folder = null;
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings != null)
            {
                var folders = this.GetFolders(portalSettings.PortalId);
                folder = folders.SingleOrDefault(f => f.FolderID == folderId) ?? CBO.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolder(folderId));
            }

            return folder ?? CBO.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolder(folderId));
        }

        /// <summary>
        /// Gets a folder entity by providing a portal identifier and folder path.
        /// </summary>
        /// <param name="portalId">The portal where the folder exists.</param>
        /// <param name="folderPath">The path of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        public virtual IFolderInfo GetFolder(int portalId, string folderPath)
        {
            Requires.PropertyNotNull("folderPath", folderPath);

            folderPath = PathUtils.Instance.FormatFolderPath(folderPath);

            var folders = this.GetFolders(portalId);
            return folders.SingleOrDefault(f => f.FolderPath == folderPath) ?? CBO.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolder(portalId, folderPath));
        }

        /// <summary>
        /// Gets a folder entity by providing its unique id.
        /// </summary>
        /// <param name="uniqueId">The unique id of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        public virtual IFolderInfo GetFolder(Guid uniqueId)
        {
            return CBO.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolderByUniqueID(uniqueId));
        }

        /// <summary>
        /// Gets the list of subfolders for the specified folder.
        /// </summary>
        /// <param name="parentFolder">The folder to get the list of subfolders.</param>
        /// <returns>The list of subfolders for the specified folder.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when parentFolder is null.</exception>
        public virtual IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder)
        {
            return this.GetFolders(parentFolder, false);
        }

        /// <summary>
        /// Gets the sorted list of folders of the provided portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="useCache">True = Read from Cache, False = Read from DB. </param>
        /// <returns>The sorted list of folders of the provided portal.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(int portalId, bool useCache)
        {
            if (!useCache)
            {
                this.ClearFolderCache(portalId);
            }

            return this.GetFolders(portalId);
        }

        /// <summary>
        /// Gets the sorted list of folders of the provided portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The sorted list of folders of the provided portal.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(int portalId)
        {
            var folders = new List<IFolderInfo>();

            var cacheKey = string.Format(DataCache.FolderCacheKey, portalId);
            CBO.Instance.GetCachedObject<List<FolderInfo>>(new CacheItemArgs(cacheKey, DataCache.FolderCacheTimeOut, DataCache.FolderCachePriority, portalId), this.GetFoldersSortedCallBack, false).ForEach(folders.Add);

            return folders;
        }

        /// <summary>
        /// Gets the sorted list of folders that match the provided permissions in the specified portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="permissions">The permissions to match.</param>
        /// <param name="userId">The user identifier to be used to check permissions.</param>
        /// <returns>The list of folders that match the provided permissions in the specified portal.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(int portalId, string permissions, int userId)
        {
            var folders = new List<IFolderInfo>();

            var cacheKey = string.Format(DataCache.FolderUserCacheKey, portalId, permissions, userId);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.FolderUserCacheTimeOut, DataCache.FolderUserCachePriority, portalId, permissions, userId);
            CBO.Instance.GetCachedObject<List<FolderInfo>>(cacheItemArgs, this.GetFoldersByPermissionSortedCallBack, false).ForEach(folders.Add);

            return folders;
        }

        /// <summary>
        /// Gets the list of folders the specified user has read permissions.
        /// </summary>
        /// <param name="user">The user info.</param>
        /// <returns>The list of folders the specified user has read permissions.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(UserInfo user)
        {
            return this.GetFolders(user, "READ");
        }

        /// <summary>
        /// Gets the list of folders the specified user has the provided permissions.
        /// </summary>
        /// <param name="user">The user info.</param>
        /// <param name="permissions">The permissions the folders have to met.</param>
        /// <returns>The list of folders the specified user has the provided permissions.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(UserInfo user, string permissions)
        {
            var userFolders = new List<IFolderInfo>();

            var portalId = user.PortalID;

            var userFolder = this.GetUserFolder(user);

            foreach (var folder in this.GetFolders(portalId, permissions, user.UserID).Where(folder => folder.FolderPath != null))
            {
                if (folder.FolderPath.StartsWith(DefaultUsersFoldersPath + "/", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (folder.FolderID == userFolder.FolderID)
                    {
                        folder.DisplayPath = Localization.GetString("MyFolderName") + "/";
                        folder.DisplayName = Localization.GetString("MyFolderName");
                    }
                    else if (!folder.FolderPath.StartsWith(userFolder.FolderPath, StringComparison.InvariantCultureIgnoreCase)) // Allow UserFolder children
                    {
                        continue;
                    }
                }

                userFolders.Add(folder);
            }

            return userFolders;
        }

        public virtual IFolderInfo GetUserFolder(UserInfo userInfo)
        {
            // always use _default portal for a super user
            int portalId = userInfo.IsSuperUser ? -1 : userInfo.PortalID;

            string userFolderPath = ((PathUtils)PathUtils.Instance).GetUserFolderPathInternal(userInfo);
            return this.GetFolder(portalId, userFolderPath) ?? this.AddUserFolder(userInfo);
        }

        public virtual IFolderInfo MoveFolder(IFolderInfo folder, IFolderInfo destinationFolder)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNull("destinationFolder", destinationFolder);

            var newFolderPath = PathUtils.Instance.FormatFolderPath(destinationFolder.FolderPath + folder.FolderName + "/");

            if (folder.FolderPath == destinationFolder.FolderPath)
            {
                return folder;
            }

            if (this.FolderExists(folder.PortalID, newFolderPath))
            {
                throw new InvalidOperationException(string.Format(
                    Localization.GetExceptionMessage(
                        "CannotMoveFolderAlreadyExists",
                        "The folder with name '{0}' cannot be moved. A folder with that name already exists under the folder '{1}'.", folder.FolderName, destinationFolder.FolderName)));
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var destinationFolderMapping = FolderMappingController.Instance.GetFolderMapping(destinationFolder.PortalID, destinationFolder.FolderMappingID);

            if (!this.CanMoveBetweenFolderMappings(folderMapping, destinationFolderMapping))
            {
                throw new InvalidOperationException(string.Format(
                    Localization.GetExceptionMessage(
                        "CannotMoveFolderBetweenFolderType",
                        "The folder with name '{0}' cannot be moved. Move Folder operation between this two folder types is not allowed", folder.FolderName)));
            }

            if (!this.IsMoveOperationValid(folder, destinationFolder, newFolderPath))
            {
                throw new InvalidOperationException(Localization.GetExceptionMessage("MoveFolderCannotComplete", "The operation cannot be completed."));
            }

            var currentFolderPath = folder.FolderPath;

            if ((folder.FolderMappingID == destinationFolder.FolderMappingID && FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMoveFolder) ||
                (IsStandardFolderProviderType(folderMapping) && IsStandardFolderProviderType(destinationFolderMapping)))
            {
                this.MoveFolderWithinProvider(folder, destinationFolder);
            }
            else
            {
                this.MoveFolderBetweenProviders(folder, newFolderPath);
            }

            // log the folder moved event.
            var log = new LogInfo();
            log.AddProperty("Old Folder Path", currentFolderPath);
            log.AddProperty("New Folder Path", newFolderPath);
            log.AddProperty("Home Directory", folder.PortalID == Null.NullInteger ? Globals.HostPath : PortalSettings.Current.HomeDirectory);
            log.LogTypeKey = EventLogController.EventLogType.FOLDER_MOVED.ToString();
            LogController.Instance.AddLog(log);

            // Files in cache are obsolete because their physical path is not correct after moving
            this.DeleteFilesFromCache(folder.PortalID, newFolderPath);
            var movedFolder = this.GetFolder(folder.FolderID);

            // Notify folder moved event
            this.OnFolderMoved(folder, this.GetCurrentUserId(), currentFolderPath);

            return movedFolder;
        }

        /// <summary>
        /// Renames the specified folder by setting the new provided folder name.
        /// </summary>
        /// <param name="folder">The folder to rename.</param>
        /// <param name="newFolderName">The new name to apply to the folder.</param>
        /// <exception cref="System.ArgumentException">Thrown when newFolderName is null or empty.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual void RenameFolder(IFolderInfo folder, string newFolderName)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("newFolderName", newFolderName);

            if (folder.FolderName.Equals(newFolderName))
            {
                return;
            }

            var currentFolderName = folder.FolderName;

            var newFolderPath = folder.FolderPath.Substring(0, folder.FolderPath.LastIndexOf(folder.FolderName, StringComparison.Ordinal)) + PathUtils.Instance.FormatFolderPath(newFolderName);

            if (this.FolderExists(folder.PortalID, newFolderPath))
            {
                throw new FolderAlreadyExistsException(Localization.GetExceptionMessage("RenameFolderAlreadyExists", "The destination folder already exists. The folder has not been renamed."));
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var provider = FolderProvider.Instance(folderMapping.FolderProviderType);

            this.RenameFolderInFileSystem(folder, newFolderPath);

            // Update Provider
            provider.RenameFolder(folder, newFolderName);

            // Update database
            this.UpdateChildFolders(folder, newFolderPath);

            // Files in cache are obsolete because their physical path is not correct after rename
            this.DeleteFilesFromCache(folder.PortalID, newFolderPath);

            // Notify folder renamed event
            this.OnFolderRenamed(folder, this.GetCurrentUserId(), currentFolderName);
        }

        /// <summary>
        /// Search the files contained in the specified folder, for a matching pattern.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="pattern">The patter to search for.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> SearchFiles(IFolderInfo folder, string pattern, bool recursive)
        {
            Requires.NotNull("folder", folder);

            if (!FolderPermissionController.Instance.CanViewFolder(folder))
            {
                throw new FolderProviderException("No permission to view the folder");
            }

            return this.SearchFiles(folder, WildcardToRegex(pattern), recursive);
        }

        /// <summary>
        /// Synchronizes the entire folder tree for the specified portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The number of folder collisions.</returns>
        public virtual int Synchronize(int portalId)
        {
            var folderCollisions = this.Synchronize(portalId, string.Empty, true, true);

            DataCache.ClearFolderCache(portalId);

            return folderCollisions;
        }

        /// <summary>
        /// Syncrhonizes the specified folder, its files and its subfolders.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="relativePath">The relative path of the folder.</param>
        /// <returns>The number of folder collisions.</returns>
        public virtual int Synchronize(int portalId, string relativePath)
        {
            return this.Synchronize(portalId, relativePath, true, true);
        }

        /// <summary>
        /// Syncrhonizes the specified folder, its files and, optionally, its subfolders.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="relativePath">The relative path of the folder.</param>
        /// <param name="isRecursive">Indicates if the synchronization has to be recursive.</param>
        /// <param name="syncFiles">Indicates if files need to be synchronized.</param>
        /// <exception cref="DotNetNuke.Services.FileSystem.NoNetworkAvailableException">Thrown when there are folder mappings requiring network connection but there is no network available.</exception>
        /// <returns>The number of folder collisions.</returns>
        public virtual int Synchronize(int portalId, string relativePath, bool isRecursive, bool syncFiles)
        {
            Requires.PropertyNotNull("relativePath", relativePath);

            if (this.AreThereFolderMappingsRequiringNetworkConnectivity(portalId, relativePath, isRecursive) && !this.IsNetworkAvailable())
            {
                throw new NoNetworkAvailableException(Localization.GetExceptionMessage("NoNetworkAvailableError", "Network connectivity is needed but there is no network available."));
            }

            int? scriptTimeOut = null;

            Monitor.Enter(_threadLocker);
            try
            {
                if (HttpContext.Current != null)
                {
                    scriptTimeOut = this.GetCurrentScriptTimeout();

                    // Synchronization could be a time-consuming process. To not get a time-out, we need to modify the request time-out value
                    this.SetScriptTimeout(int.MaxValue);
                }

                var mergedTree = this.GetMergedTree(portalId, relativePath, isRecursive);

                // Step 1: Add Folders
                this.InitialiseSyncFoldersData(portalId, relativePath);
                for (var i = 0; i < mergedTree.Count; i++)
                {
                    var item = mergedTree.Values[i];
                    this.ProcessMergedTreeItemInAddMode(item, portalId);
                }

                this.RemoveSyncFoldersData(relativePath);

                // Step 2: Delete Files and Folders
                for (var i = mergedTree.Count - 1; i >= 0; i--)
                {
                    var item = mergedTree.Values[i];

                    if (syncFiles)
                    {
                        this.SynchronizeFiles(item, portalId);
                    }

                    this.ProcessMergedTreeItemInDeleteMode(item, portalId);
                }
            }
            finally
            {
                Monitor.Exit(_threadLocker);

                // Restore original time-out
                if (HttpContext.Current != null && scriptTimeOut != null)
                {
                    this.SetScriptTimeout(scriptTimeOut.Value);
                }
            }

            return 0;
        }

        /// <summary>
        /// Updates metadata of the specified folder.
        /// </summary>
        /// <param name="folder">The folder to update.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <returns></returns>
        public virtual IFolderInfo UpdateFolder(IFolderInfo folder)
        {
            var updatedFolder = this.UpdateFolderInternal(folder, true);

            this.AddLogEntry(updatedFolder, EventLogController.EventLogType.FOLDER_UPDATED);

            this.SaveFolderPermissions(updatedFolder);

            return updatedFolder;
        }

        /// <summary>
        /// Adds read permissions for all users to the specified folder.
        /// </summary>
        /// <param name="folder">The folder to add the permission to.</param>
        /// <param name="permission">Used as base class for FolderPermissionInfo when there is no read permission already defined.</param>
        public virtual void AddAllUserReadPermission(IFolderInfo folder, PermissionInfo permission)
        {
            var roleId = int.Parse(Globals.glbRoleAllUsers);

            var folderPermission =
                (from FolderPermissionInfo p in folder.FolderPermissions
                 where p.PermissionKey == "READ" && p.FolderID == folder.FolderID && p.RoleID == roleId && p.UserID == Null.NullInteger
                 select p).SingleOrDefault();

            if (folderPermission != null)
            {
                folderPermission.AllowAccess = true;
            }
            else
            {
                folderPermission = new FolderPermissionInfo(permission)
                {
                    FolderID = folder.FolderID,
                    UserID = Null.NullInteger,
                    RoleID = roleId,
                    AllowAccess = true,
                };

                folder.FolderPermissions.Add(folderPermission);
            }
        }

        /// <summary>
        /// Sets folder permissions to the given folder by copying parent folder permissions.
        /// </summary>
        /// <param name="folder">The folder to copy permissions to.</param>
        public virtual void CopyParentFolderPermissions(IFolderInfo folder)
        {
            Requires.NotNull("folder", folder);

            if (string.IsNullOrEmpty(folder.FolderPath))
            {
                return;
            }

            var parentFolderPath = folder.FolderPath.Substring(0, folder.FolderPath.Substring(0, folder.FolderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);

            foreach (FolderPermissionInfo objPermission in
                this.GetFolderPermissionsFromSyncData(folder.PortalID, parentFolderPath))
            {
                var folderPermission = new FolderPermissionInfo(objPermission)
                {
                    FolderID = folder.FolderID,
                    RoleID = objPermission.RoleID,
                    UserID = objPermission.UserID,
                    AllowAccess = objPermission.AllowAccess,
                };
                folder.FolderPermissions.Add(folderPermission, true);
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }

        /// <summary>
        /// Sets specific folder permissions for the given role to the given folder.
        /// </summary>
        /// <param name="folder">The folder to set permission to.</param>
        /// <param name="permissionId">The id of the permission to assign.</param>
        /// <param name="roleId">The role to assign the permission to.</param>
        public virtual void SetFolderPermission(IFolderInfo folder, int permissionId, int roleId)
        {
            this.SetFolderPermission(folder, permissionId, roleId, Null.NullInteger);
        }

        /// <summary>
        /// Sets specific folder permissions for the given role/user to the given folder.
        /// </summary>
        /// <param name="folder">The folder to set permission to.</param>
        /// <param name="permissionId">The id of the permission to assign.</param>
        /// <param name="roleId">The role to assign the permission to.</param>
        /// <param name="userId">The user to assign the permission to.</param>
        public virtual void SetFolderPermission(IFolderInfo folder, int permissionId, int roleId, int userId)
        {
            if (folder.FolderPermissions.Cast<FolderPermissionInfo>()
                .Any(fpi => fpi.FolderID == folder.FolderID && fpi.PermissionID == permissionId && fpi.RoleID == roleId && fpi.UserID == userId && fpi.AllowAccess))
            {
                return;
            }

            var objFolderPermissionInfo = new FolderPermissionInfo
            {
                FolderID = folder.FolderID,
                PermissionID = permissionId,
                RoleID = roleId,
                UserID = userId,
                AllowAccess = true,
            };

            folder.FolderPermissions.Add(objFolderPermissionInfo, true);
            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }

        /// <summary>
        /// Sets folder permissions for administrator role to the given folder.
        /// </summary>
        /// <param name="folder">The folder to set permission to.</param>
        /// <param name="administratorRoleId">The administrator role id to assign the permission to.</param>
        public virtual void SetFolderPermissions(IFolderInfo folder, int administratorRoleId)
        {
            Requires.NotNull("folder", folder);

            foreach (PermissionInfo objPermission in PermissionController.GetPermissionsByFolder())
            {
                var folderPermission = new FolderPermissionInfo(objPermission)
                {
                    FolderID = folder.FolderID,
                    RoleID = administratorRoleId,
                };

                folder.FolderPermissions.Add(folderPermission, true);
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }

        /// <summary>
        /// Moves the specified folder and its contents to a new location.
        /// </summary>
        /// <param name="folder">The folder to move.</param>
        /// <param name="newFolderPath">The new folder path.</param>
        /// <returns>The moved folder.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.1.  It has been replaced by FolderManager.Instance.MoveFolder(IFolderInfo folder, IFolderInfo destinationFolder) . Scheduled removal in v10.0.0.")]
        public virtual IFolderInfo MoveFolder(IFolderInfo folder, string newFolderPath)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("newFolderPath", newFolderPath);

            var nameCharIndex = newFolderPath.Substring(0, newFolderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1;
            var parentFolder = this.GetFolder(folder.PortalID, newFolderPath.Substring(0, nameCharIndex));
            if (parentFolder.FolderID == folder.ParentID)
            {
                var newFolderName = newFolderPath.Substring(nameCharIndex, newFolderPath.Length - nameCharIndex - 1);
                this.RenameFolder(folder, newFolderName);
                return folder;
            }

            return this.MoveFolder(folder, parentFolder);
        }

        internal virtual bool IsValidFolderPath(string folderPath)
        {
            var illegalInFolderPath = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidPathChars()))), RegexOptions.Compiled);
            return !illegalInFolderPath.IsMatch(folderPath) && !folderPath.TrimEnd('/', '\\').EndsWith(".");
        }

        internal virtual void AddLogEntry(IFolderInfo folder, EventLogController.EventLogType eventLogType)
        {
            EventLogController.Instance.AddLog(folder, PortalController.Instance.GetCurrentPortalSettings(), this.GetCurrentUserId(), string.Empty, eventLogType);
        }

        internal virtual void AddLogEntry(string propertyName, string propertyValue, EventLogController.EventLogType eventLogType)
        {
            EventLogController.Instance.AddLog(propertyName, propertyValue, PortalController.Instance.GetCurrentPortalSettings(), this.GetCurrentUserId(), eventLogType);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal void DeleteFilesFromCache(int portalId, string newFolderPath)
        {
            var folders = this.GetFolders(portalId).Where(f => f.FolderPath.StartsWith(newFolderPath));
            foreach (var folderInfo in folders)
            {
                var fileIds = this.GetFiles(folderInfo).Select(f => f.FileId);
                foreach (var fileId in fileIds)
                {
                    DataCache.RemoveCache("GetFileById" + fileId);
                }
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual IFolderInfo AddUserFolder(UserInfo user)
        {
            // user _default portal for all super users
            var portalId = user.IsSuperUser ? Null.NullInteger : user.PortalID;

            var folderMapping = FolderMappingsConfigController.Instance.GetFolderMapping(portalId, DefaultUsersFoldersPath) ?? FolderMappingController.Instance.GetDefaultFolderMapping(portalId);

            if (!this.FolderExists(portalId, DefaultUsersFoldersPath))
            {
                this.AddFolder(folderMapping, DefaultUsersFoldersPath);
            }

#pragma warning disable 612,618
            var rootFolder = PathUtils.Instance.GetUserFolderPathElement(user.UserID, PathUtils.UserFolderElement.Root);
#pragma warning restore 612,618

            var folderPath = PathUtils.Instance.FormatFolderPath(string.Format(DefaultUsersFoldersPath + "/{0}", rootFolder));

            if (!this.FolderExists(portalId, folderPath))
            {
                this.AddFolder(folderMapping, folderPath);
            }

#pragma warning disable 612,618
            folderPath = PathUtils.Instance.FormatFolderPath(string.Concat(folderPath, PathUtils.Instance.GetUserFolderPathElement(user.UserID, PathUtils.UserFolderElement.SubFolder)));
#pragma warning restore 612,618

            if (!this.FolderExists(portalId, folderPath))
            {
                this.AddFolder(folderMapping, folderPath);
            }

            folderPath = PathUtils.Instance.FormatFolderPath(string.Concat(folderPath, user.UserID.ToString(CultureInfo.InvariantCulture)));

            if (!this.FolderExists(portalId, folderPath))
            {
                this.AddFolder(folderMapping, folderPath);

                var folder = this.GetFolder(portalId, folderPath);

                foreach (PermissionInfo permission in PermissionController.GetPermissionsByFolder())
                {
                    if (permission.PermissionKey.Equals("READ", StringComparison.InvariantCultureIgnoreCase) || permission.PermissionKey.Equals("WRITE", StringComparison.InvariantCultureIgnoreCase) || permission.PermissionKey.Equals("BROWSE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var folderPermission = new FolderPermissionInfo(permission)
                        {
                            FolderID = folder.FolderID,
                            UserID = user.UserID,
                            RoleID = int.Parse(Globals.glbRoleNothing),
                            AllowAccess = true,
                        };

                        folder.FolderPermissions.Add(folderPermission);

                        if (permission.PermissionKey.Equals("READ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.AddAllUserReadPermission(folder, permission);
                        }
                    }
                }

                FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
            }

            return this.GetFolder(portalId, folderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual bool AreThereFolderMappingsRequiringNetworkConnectivity(int portalId, string relativePath, bool isRecursive)
        {
            var folder = this.GetFolder(portalId, relativePath);

            if (folder != null)
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
                var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

                if (folderProvider.RequiresNetworkConnectivity)
                {
                    return true;
                }
            }

            if (isRecursive)
            {
                var folderMappingsRequiringNetworkConnectivity = from fm in FolderMappingController.Instance.GetFolderMappings(portalId)
                                                                 where
                                                                      fm.IsEditable &&
                                                                      FolderProvider.Instance(fm.FolderProviderType).RequiresNetworkConnectivity
                                                                 select fm;

                return folderMappingsRequiringNetworkConnectivity.Any();
            }

            return false;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal void ClearFolderProviderCachedLists(int portalId)
        {
            foreach (var folderMapping in FolderMappingController.Instance.GetFolderMappings(portalId))
            {
                var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

                if (folderMapping.MappingName != "Standard" && folderMapping.MappingName != "Secure" && folderMapping.MappingName != "Database")
                {
                    var type = folderProvider.GetType();
                    MethodInfo method = type.GetMethod("ClearCache");
                    if (method != null)
                    {
                        method.Invoke(folderProvider, new object[] { folderMapping.FolderMappingID });
                    }
                }
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void ClearFolderCache(int portalId)
        {
            DataCache.ClearFolderCache(portalId);
        }

        internal virtual int CreateFolderInDatabase(int portalId, string folderPath, int folderMappingId)
        {
            return this.CreateFolderInDatabase(portalId, folderPath, folderMappingId, folderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual int CreateFolderInDatabase(int portalId, string folderPath, int folderMappingId, string mappedPath)
        {
            var isProtected = PathUtils.Instance.IsDefaultProtectedPath(folderPath);
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folderMappingId);
            var storageLocation = (int)FolderController.StorageLocationTypes.DatabaseSecure;
            if (!folderMapping.IsEditable)
            {
                switch (folderMapping.MappingName)
                {
                    case "Standard":
                        storageLocation = (int)FolderController.StorageLocationTypes.InsecureFileSystem;
                        break;
                    case "Secure":
                        storageLocation = (int)FolderController.StorageLocationTypes.SecureFileSystem;
                        break;
                    default:
                        storageLocation = (int)FolderController.StorageLocationTypes.DatabaseSecure;
                        break;
                }
            }

            var folder = new FolderInfo(true)
            {
                PortalID = portalId,
                FolderPath = folderPath,
                MappedPath = mappedPath,
                StorageLocation = storageLocation,
                IsProtected = isProtected,
                IsCached = false,
                FolderMappingID = folderMappingId,
                LastUpdated = Null.NullDate,
            };

            folder.FolderID = this.AddFolderInternal(folder);

            if (portalId != Null.NullInteger)
            {
                // Set Folder Permissions to inherit from parent
                this.CopyParentFolderPermissions(folder);
            }

            return folder.FolderID;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void CreateFolderInFileSystem(string physicalPath)
        {
            var di = new DirectoryInfo(physicalPath);

            if (!di.Exists)
            {
                di.Create();
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void DeleteFolder(int portalId, string folderPath)
        {
            DataProvider.Instance().DeleteFolder(portalId, PathUtils.Instance.FormatFolderPath(folderPath));
            this.AddLogEntry("FolderPath", folderPath, EventLogController.EventLogType.FOLDER_DELETED);
            this.UpdateParentFolder(portalId, folderPath);
            DataCache.ClearFolderCache(portalId);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void DeleteFoldersFromExternalStorageLocations(Dictionary<int, FolderMappingInfo> folderMappings, IEnumerable<IFolderInfo> foldersToDelete)
        {
            foreach (var folderToDelete in foldersToDelete)
            {
                // Delete source folder from its storage location
                var folderMapping = this.GetFolderMapping(folderMappings, folderToDelete.FolderMappingID);

                try
                {
                    var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

                    // IMPORTANT: We cannot delete the folder from its storage location when it contains other subfolders
                    if (!folderProvider.GetSubFolders(folderToDelete.MappedPath, folderMapping).Any())
                    {
                        folderProvider.DeleteFolder(folderToDelete);
                    }
                }
                catch (Exception ex)
                {
                    // The folders that cannot be deleted from its storage location will be handled during the next sync
                    Logger.Error(ex);
                }
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual int GetCurrentScriptTimeout()
        {
            return HttpContext.Current.Server.ScriptTimeout;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual int GetCurrentUserId()
        {
            return UserController.Instance.GetCurrentUserInfo().UserID;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual SortedList<string, MergedTreeItem> GetDatabaseFolders(int portalId, string relativePath, bool isRecursive)
        {
            var databaseFolders = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());

            var folder = this.GetFolder(portalId, relativePath);

            if (folder != null)
            {
                if (!isRecursive)
                {
                    var item = new MergedTreeItem
                    {
                        FolderID = folder.FolderID,
                        FolderMappingID = folder.FolderMappingID,
                        FolderPath = folder.FolderPath,
                        ExistsInDatabase = true,
                        MappedPath = folder.MappedPath,
                    };

                    databaseFolders.Add(relativePath, item);
                }
                else
                {
                    databaseFolders = this.GetDatabaseFoldersRecursive(folder);
                }
            }

            return databaseFolders;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual SortedList<string, MergedTreeItem> GetDatabaseFoldersRecursive(IFolderInfo folder)
        {
            var result = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());
            var stack = new Stack<IFolderInfo>();

            stack.Push(folder);

            while (stack.Count > 0)
            {
                var folderInfo = stack.Pop();

                var item = new MergedTreeItem
                {
                    FolderID = folderInfo.FolderID,
                    FolderMappingID = folderInfo.FolderMappingID,
                    FolderPath = folderInfo.FolderPath,
                    ExistsInDatabase = true,
                    MappedPath = folderInfo.MappedPath,
                };

                if (!result.ContainsKey(item.FolderPath))
                {
                    result.Add(item.FolderPath, item);
                }

                foreach (var subfolder in this.GetFolders(folderInfo))
                {
                    stack.Push(subfolder);
                }
            }

            return result;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual SortedList<string, MergedTreeItem> GetFileSystemFolders(int portalId, string relativePath, bool isRecursive)
        {
            var fileSystemFolders = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());

            var physicalPath = PathUtils.Instance.GetPhysicalPath(portalId, relativePath);
            var hideFoldersEnabled = PortalController.GetPortalSettingAsBoolean("HideFoldersEnabled", portalId, true);

            if (DirectoryWrapper.Instance.Exists(physicalPath))
            {
                if (((FileWrapper.Instance.GetAttributes(physicalPath) & FileAttributes.Hidden) == FileAttributes.Hidden || physicalPath.StartsWith("_")) && hideFoldersEnabled)
                {
                    return fileSystemFolders;
                }

                if (!isRecursive)
                {
                    var item = new MergedTreeItem
                    {
                        FolderID = -1,
                        FolderMappingID = -1,
                        FolderPath = relativePath,
                        ExistsInFileSystem = true,
                        MappedPath = string.Empty,
                    };

                    fileSystemFolders.Add(relativePath, item);
                }
                else
                {
                    fileSystemFolders = this.GetFileSystemFoldersRecursive(portalId, physicalPath);
                }
            }

            return fileSystemFolders;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual SortedList<string, MergedTreeItem> GetFileSystemFoldersRecursive(int portalId, string physicalPath)
        {
            var result = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());
            var stack = new Stack<string>();

            stack.Push(physicalPath);

            var hideFoldersEnabled = PortalController.GetPortalSettingAsBoolean("HideFoldersEnabled", portalId, true);

            while (stack.Count > 0)
            {
                var dir = stack.Pop();

                try
                {
                    var item = new MergedTreeItem
                    {
                        FolderID = -1,
                        FolderMappingID = -1,
                        FolderPath = PathUtils.Instance.GetRelativePath(portalId, dir),
                        ExistsInFileSystem = true,
                        MappedPath = string.Empty,
                    };

                    result.Add(item.FolderPath, item);

                    foreach (var dn in DirectoryWrapper.Instance.GetDirectories(dir))
                    {
                        if (((FileWrapper.Instance.GetAttributes(dn) & FileAttributes.Hidden) == FileAttributes.Hidden || dn.StartsWith("_")) && hideFoldersEnabled)
                        {
                            continue;
                        }

                        stack.Push(dn);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return result;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual FolderMappingInfo GetFolderMapping(Dictionary<int, FolderMappingInfo> folderMappings, int folderMappingId)
        {
            if (!folderMappings.ContainsKey(folderMappingId))
            {
                folderMappings.Add(folderMappingId, FolderMappingController.Instance.GetFolderMapping(folderMappingId));
            }

            return folderMappings[folderMappingId];
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual SortedList<string, MergedTreeItem> GetFolderMappingFoldersRecursive(FolderMappingInfo folderMapping, IFolderInfo folder)
        {
            var result = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());
            var stack = new Stack<string>();
            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

            var baseMappedPath = folder.MappedPath;
            var baseFolderPath = folder.FolderPath;

            stack.Push(baseMappedPath);

            while (stack.Count > 0)
            {
                var mappedPath = stack.Pop();
                var relativePath = string.IsNullOrEmpty(mappedPath)
                                        ? string.Empty
                                        : string.IsNullOrEmpty(baseMappedPath)
                                            ? mappedPath
                                            : RegexUtils.GetCachedRegex(Regex.Escape(baseMappedPath)).Replace(mappedPath, string.Empty, 1);

                var folderPath = baseFolderPath + relativePath;

                if (folderProvider.FolderExists(mappedPath, folderMapping))
                {
                    var item = new MergedTreeItem
                    {
                        FolderID = -1,
                        FolderMappingID = folderMapping.FolderMappingID,
                        FolderPath = folderPath,
                        ExistsInFolderMapping = true,
                        MappedPath = mappedPath,
                    };

                    if (!result.ContainsKey(item.FolderPath))
                    {
                        result.Add(item.FolderPath, item);
                    }

                    foreach (var subfolderPath in folderProvider.GetSubFolders(mappedPath, folderMapping))
                    {
                        if (folderMapping.SyncAllSubFolders || folderProvider.FolderExists(subfolderPath, folderMapping))
                        {
                            stack.Push(subfolderPath);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual object GetFoldersByPermissionSortedCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var permissions = (string)cacheItemArgs.ParamList[1];
            var userId = (int)cacheItemArgs.ParamList[2];
            return CBO.Instance.FillCollection<FolderInfo>(DataProvider.Instance().GetFoldersByPortalAndPermissions(portalId, permissions, userId));
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual object GetFoldersSortedCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            return CBO.Instance.FillCollection<FolderInfo>(DataProvider.Instance().GetFoldersByPortal(portalId));
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual SortedList<string, MergedTreeItem> GetMergedTree(int portalId, string relativePath, bool isRecursive)
        {
            var fileSystemFolders = this.GetFileSystemFolders(portalId, relativePath, isRecursive);
            var databaseFolders = this.GetDatabaseFolders(portalId, relativePath, isRecursive);

            var mergedTree = this.MergeFolderLists(fileSystemFolders, databaseFolders);
            var mappedFolders = new SortedList<string, MergedTreeItem>();

            // Some providers cache the list of objects for performance
            this.ClearFolderProviderCachedLists(portalId);

            foreach (var mergedItem in mergedTree.Values)
            {
                if (mergedItem.FolderMappingID == Null.NullInteger)
                {
                    continue;
                }

                var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, mergedItem.FolderMappingID);

                // Add any folders from non-core providers
                if (folderMapping.MappingName != "Standard" && folderMapping.MappingName != "Secure" && folderMapping.MappingName != "Database")
                {
                    if (!isRecursive)
                    {
                        mergedItem.ExistsInFolderMapping = true;
                    }
                    else
                    {
                        var folder = this.GetFolder(portalId, mergedItem.FolderPath);
                        mappedFolders = this.MergeFolderLists(mappedFolders, this.GetFolderMappingFoldersRecursive(folderMapping, folder));
                    }
                }
                else
                {
                    mergedItem.ExistsInFolderMapping = folderMapping.MappingName == "Database" ? mergedItem.ExistsInDatabase : mergedItem.ExistsInFileSystem;
                }
            }

            mergedTree = this.MergeFolderLists(mergedTree, mappedFolders);

            // Update ExistsInFolderMapping if the Parent Does Not ExistsInFolderMapping
            var margedTreeItems = mergedTree.Values;
            foreach (var mergedItem in margedTreeItems.Where(m => m.ExistsInFolderMapping
                                       && margedTreeItems.Any(mt2 => !mt2.ExistsInFolderMapping && m.FolderPath.StartsWith(mt2.FolderPath))))
            {
                mergedItem.ExistsInFolderMapping = false;
            }

            return mergedTree;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual bool IsFolderMappingEditable(FolderMappingInfo folderMapping)
        {
            return folderMapping.IsEditable;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual bool IsMoveOperationValid(IFolderInfo folderToMove, IFolderInfo destinationFolder, string newFolderPath)
        {
            // FolderMapping cases
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folderToMove.PortalID, folderToMove.FolderMappingID);
            if (folderToMove.FolderMappingID == destinationFolder.FolderMappingID && FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMappedPaths)
            {
                // Root mapped folder cannot be move, when folder mappings are equal
                if (folderToMove.MappedPath == string.Empty)
                {
                    return false;
                }

                // Destination folder cannot be a child mapped folder from the folder to move
                if (destinationFolder.MappedPath.StartsWith(folderToMove.MappedPath))
                {
                    return false;
                }
            }

            return this.IsMoveOperationValid(folderToMove, newFolderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual bool IsMoveOperationValid(IFolderInfo folderToMove, string newFolderPath)
        {
            // Root folder cannot be moved
            if (folderToMove.FolderPath == string.Empty)
            {
                return false;
            }

            // newParentFolder cannot be a child of folderToMove
            if (newFolderPath.StartsWith(folderToMove.FolderPath))
            {
                return false;
            }

            return true;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual SortedList<string, MergedTreeItem> MergeFolderLists(SortedList<string, MergedTreeItem> list1, SortedList<string, MergedTreeItem> list2)
        {
            foreach (var item in list2.Values)
            {
                if (list1.ContainsKey(item.FolderPath))
                {
                    var existingItem = list1[item.FolderPath];
                    if (existingItem.FolderID < 0)
                    {
                        existingItem.FolderID = item.FolderID;
                    }

                    if (existingItem.FolderMappingID < 0)
                    {
                        existingItem.FolderMappingID = item.FolderMappingID;
                    }

                    if (string.IsNullOrEmpty(existingItem.MappedPath))
                    {
                        existingItem.MappedPath = item.MappedPath;
                    }

                    existingItem.ExistsInFileSystem = existingItem.ExistsInFileSystem || item.ExistsInFileSystem;
                    existingItem.ExistsInDatabase = existingItem.ExistsInDatabase || item.ExistsInDatabase;
                    existingItem.ExistsInFolderMapping = existingItem.ExistsInFolderMapping || item.ExistsInFolderMapping;
                }
                else
                {
                    list1.Add(item.FolderPath, item);
                }
            }

            return list1;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void MoveDirectory(string source, string target)
        {
            var stack = new Stack<MoveFoldersInfo>();
            stack.Push(new MoveFoldersInfo(source, target));

            // ReSharper disable AssignNullToNotNullAttribute
            while (stack.Count > 0)
            {
                var folders = stack.Pop();
                Directory.CreateDirectory(folders.Target);
                foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                {
                    var targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }

                    File.Move(file, targetFile);
                }

                foreach (var folder in Directory.GetDirectories(folders.Source))
                {
                    stack.Push(new MoveFoldersInfo(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                }
            }

            // ReSharper restore AssignNullToNotNullAttribute
            Directory.Delete(source, true);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void MoveFolderWithinProvider(IFolderInfo folder, IFolderInfo destinationFolder)
        {
            var newFolderPath = destinationFolder.FolderPath + folder.FolderName + "/";
            this.RenameFolderInFileSystem(folder, newFolderPath);

            // Update provider
            var newMappedPath = destinationFolder.MappedPath + folder.FolderName + "/";
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var provider = FolderProvider.Instance(folderMapping.FolderProviderType);
            provider.MoveFolder(folder.MappedPath, newMappedPath, folderMapping);

            // Update database
            this.UpdateChildFolders(folder, Path.Combine(destinationFolder.FolderPath, folder.FolderName));
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void MoveFolderBetweenProviders(IFolderInfo folder, string newFolderPath)
        {
            this.RenameFolderInFileSystem(folder, newFolderPath);

            var folderInfos = this.GetFolders(folder.PortalID).Where(f => f.FolderPath != string.Empty && f.FolderPath.StartsWith(folder.FolderPath)).ToArray();
            var tmpFolderPath = folder.FolderPath;

            foreach (var folderInfo in folderInfos)
            {
                var folderPath = newFolderPath + folderInfo.FolderPath.Substring(tmpFolderPath.Length);

                var parentFolder = this.GetParentFolder(folder.PortalID, folderPath);
                folderInfo.ParentID = parentFolder.FolderID;
                folderInfo.FolderPath = folderPath;
                this.UpdateFolderInternal(folderInfo, true);
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void OverwriteFolder(IFolderInfo sourceFolder, IFolderInfo destinationFolder, Dictionary<int, FolderMappingInfo> folderMappings, SortedList<string, IFolderInfo> foldersToDelete)
        {
            var fileManager = FileManager.Instance;
            var files = this.GetFiles(sourceFolder, true);

            foreach (var file in files)
            {
                fileManager.MoveFile(file, destinationFolder);
            }

            // Delete source folder in database
            this.DeleteFolder(sourceFolder.PortalID, sourceFolder.FolderPath);

            var folderMapping = this.GetFolderMapping(folderMappings, sourceFolder.FolderMappingID);

            if (this.IsFolderMappingEditable(folderMapping))
            {
                foldersToDelete.Add(sourceFolder.FolderPath, sourceFolder);
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void ProcessMergedTreeItemInAddMode(MergedTreeItem item, int portalId)
        {
            try
            {
                if (item.ExistsInFileSystem)
                {
                    if (!item.ExistsInDatabase)
                    {
                        var folderMappingId = this.FindFolderMappingId(item, portalId);
                        this.CreateFolderInDatabase(portalId, item.FolderPath, folderMappingId);
                    }
                }
                else
                {
                    if (item.ExistsInDatabase)
                    {
                        if (item.ExistsInFolderMapping)
                        {
                            this.CreateFolderInFileSystem(PathUtils.Instance.GetPhysicalPath(portalId, item.FolderPath));
                        }
                    }
                    else // by exclusion it exists in the Folder Mapping
                    {
                        this.CreateFolderInFileSystem(PathUtils.Instance.GetPhysicalPath(portalId, item.FolderPath));
                        this.CreateFolderInDatabase(portalId, item.FolderPath, item.FolderMappingID, item.MappedPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Could not create folder {0}. EXCEPTION: {1}", item.FolderPath, ex.Message), ex);
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void ProcessMergedTreeItemInDeleteMode(MergedTreeItem item, int portalId)
        {
            if (item.ExistsInFileSystem)
            {
                if (item.ExistsInDatabase)
                {
                    if (item.FolderPath == string.Empty)
                    {
                        return; // Do not process root folder
                    }

                    if (!item.ExistsInFolderMapping)
                    {
                        var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, item.FolderMappingID);

                        if (folderMapping.IsEditable)
                        {
                            DirectoryWrapper.Instance.Delete(PathUtils.Instance.GetPhysicalPath(portalId, item.FolderPath), false);
                            this.DeleteFolder(portalId, item.FolderPath);
                        }
                    }
                }
            }
            else
            {
                if (item.ExistsInDatabase && !item.ExistsInFolderMapping)
                {
                    this.DeleteFolder(portalId, item.FolderPath);
                }
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void RemoveOrphanedFiles(IFolderInfo folder)
        {
            var files = this.GetFiles(folder, false, true);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            if (folderMapping != null)
            {
                var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

                foreach (var file in files)
                {
                    try
                    {
                        if (!folderProvider.FileExists(folder, file.FileName))
                        {
                            FileDeletionController.Instance.DeleteFileData(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void RenameFolderInFileSystem(IFolderInfo folder, string newFolderPath)
        {
            var source = folder.PhysicalPath;

            var di = new DirectoryInfo(source);
            if (!di.Exists)
            {
                return;
            }

            var target = PathUtils.Instance.GetPhysicalPath(folder.PortalID, newFolderPath);
            this.MoveDirectory(source, target);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void SaveFolderPermissions(IFolderInfo folder)
        {
            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void SetScriptTimeout(int timeout)
        {
            HttpContext.Current.Server.ScriptTimeout = timeout;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void SynchronizeFiles(MergedTreeItem item, int portalId)
        {
            var folder = this.GetFolder(portalId, item.FolderPath);

            if (folder == null)
            {
                return;
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folder.FolderMappingID);

            if (folderMapping == null)
            {
                return;
            }

            try
            {
                var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
                var fileManager = FileManager.Instance;

                if (folderProvider.FolderExists(folder.MappedPath, folderMapping))
                {
                    var files = folderProvider.GetFiles(folder);

                    files = files.Except(FileVersionController.Instance.GetFileVersionsInFolder(folder.FolderID).Select(f => f.FileName)).ToArray();

                    foreach (var fileName in files)
                    {
                        try
                        {
                            var file = fileManager.GetFile(folder, fileName, true);

                            if (file == null)
                            {
                                fileManager.AddFile(folder, fileName, null, false);
                            }
                            else if (!folderProvider.IsInSync(file))
                            {
                                fileManager.UpdateFile(file, null);
                            }
                        }
                        catch (InvalidFileExtensionException ex)
                        {
                            Logger.Info(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }

                this.RemoveOrphanedFiles(folder);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void UpdateParentFolder(int portalId, string folderPath)
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                var parentFolderPath = folderPath.Substring(0, folderPath.Substring(0, folderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
                var objFolder = this.GetFolder(portalId, parentFolderPath);
                if (objFolder != null)
                {
                    // UpdateFolder(objFolder);
                    this.UpdateFolderInternal(objFolder, false);
                }
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void UpdateChildFolders(IFolderInfo folder, string newFolderPath)
        {
            var originalFolderPath = folder.FolderPath;

            var folderInfos = this.GetFolders(folder.PortalID).Where(f => f.FolderPath != string.Empty && f.FolderPath.StartsWith(originalFolderPath)).ToArray();

            foreach (var folderInfo in folderInfos)
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folderInfo.FolderMappingID);
                var provider = FolderProvider.Instance(folderMapping.FolderProviderType);

                var folderPath = newFolderPath + (newFolderPath.EndsWith("/") ? string.Empty : "/") + folderInfo.FolderPath.Substring(originalFolderPath.Length);

                var parentFolder = this.GetParentFolder(folder.PortalID, folderPath);
                folderInfo.ParentID = parentFolder.FolderID;
                folderInfo.FolderPath = folderPath;

                var parentProvider = FolderProvider.Instance(FolderMappingController.Instance.GetFolderMapping(parentFolder.PortalID, parentFolder.FolderMappingID).FolderProviderType);
                if (parentProvider.SupportsMappedPaths || !provider.SupportsMappedPaths)
                {
                    if (provider.SupportsMappedPaths)
                    {
                        var mappedPath = parentFolder.FolderPath == string.Empty ? string.Empty : folderPath.Replace(parentFolder.FolderPath, string.Empty);
                        folderInfo.MappedPath = PathUtils.Instance.FormatFolderPath(parentFolder.MappedPath + mappedPath);
                    }
                    else
                    {
                        folderInfo.MappedPath = folderPath;
                    }
                }
                else if (provider.SupportsMappedPaths)
                {
                    if (originalFolderPath == folderInfo.MappedPath)
                    {
                        folderInfo.MappedPath = folderPath;
                    }
                    else if (folderInfo.MappedPath.EndsWith("/" + originalFolderPath, StringComparison.Ordinal))
                    {
                        var newMappedPath = PathUtils.Instance.FormatFolderPath(
                        folderInfo.MappedPath.Substring(0, folderInfo.MappedPath.LastIndexOf("/" + originalFolderPath, StringComparison.Ordinal)) + "/" + folderPath);
                        folderInfo.MappedPath = newMappedPath;
                    }
                }

                this.UpdateFolderInternal(folderInfo, false);
            }

            this.ClearFolderCache(folder.PortalID);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns></returns>
        internal virtual bool CanMoveBetweenFolderMappings(FolderMappingInfo sourceFolderMapping, FolderMappingInfo destinationFolderMapping)
        {
            // If Folder Mappings are exactly the same
            if (sourceFolderMapping.FolderMappingID == destinationFolderMapping.FolderMappingID)
            {
                return true;
            }

            return IsStandardFolderProviderType(sourceFolderMapping) && IsStandardFolderProviderType(destinationFolderMapping);
        }

        private static Regex WildcardToRegex(string pattern)
        {
            if (!pattern.Contains("*") && !pattern.Contains("?"))
            {
                pattern = "^" + pattern + ".*$";
            }
            else
            {
                pattern = "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            }

            return RegexUtils.GetCachedRegex(pattern, RegexOptions.IgnoreCase);
        }

        private static bool IsStandardFolderProviderType(FolderMappingInfo folderMappingInfo)
        {
            var compatibleTypes = new[] { "StandardFolderProvider", "SecureFolderProvider", "DatabaseFolderProvider" };
            return compatibleTypes.Contains(folderMappingInfo.FolderProviderType);
        }

        private int AddFolderInternal(IFolderInfo folder)
        {
            // Check this is not a duplicate
            var tmpfolder = this.GetFolder(folder.PortalID, folder.FolderPath);

            if (tmpfolder != null && folder.FolderID == Null.NullInteger)
            {
                folder.FolderID = tmpfolder.FolderID;
            }

            if (folder.FolderID == Null.NullInteger)
            {
                var isVersioned = folder.IsVersioned;
                var workflowId = folder.WorkflowID;

                // Inherit some configuration from its Parent Folder
                var parentFolder = this.GetParentFolder(folder.PortalID, folder.FolderPath);
                var parentId = Null.NullInteger;
                if (parentFolder != null)
                {
                    isVersioned = parentFolder.IsVersioned;
                    workflowId = parentFolder.WorkflowID;
                    parentId = parentFolder.FolderID;
                }

                folder.FolderPath = PathUtils.Instance.FormatFolderPath(folder.FolderPath);
                folder.FolderID = DataProvider.Instance().AddFolder(
                    folder.PortalID,
                    folder.UniqueId,
                    folder.VersionGuid,
                    folder.FolderPath,
                    folder.MappedPath,
                    folder.StorageLocation,
                    folder.IsProtected,
                    folder.IsCached,
                    folder.LastUpdated,
                    this.GetCurrentUserId(),
                    folder.FolderMappingID,
                    isVersioned,
                    workflowId,
                    parentId);

                // Refetch folder for logging
                folder = this.GetFolder(folder.PortalID, folder.FolderPath);

                this.AddLogEntry(folder, EventLogController.EventLogType.FOLDER_CREATED);

                if (parentFolder != null)
                {
                    this.UpdateFolderInternal(parentFolder, false);
                }
                else
                {
                    this.UpdateParentFolder(folder.PortalID, folder.FolderPath);
                }
            }
            else
            {
                var parentFolder = this.GetParentFolder(folder.PortalID, folder.FolderPath);
                if (parentFolder != null)
                {
                    // Ensure that Parent Id is repaired
                    folder.ParentID = parentFolder.FolderID;
                }

                this.UpdateFolderInternal(folder, false);
            }

            // Invalidate Cache
            this.ClearFolderCache(folder.PortalID);

            return folder.FolderID;
        }

        private bool GetOnlyUnmap(IFolderInfo folder)
        {
            if (folder == null || folder.ParentID == Null.NullInteger)
            {
                return true;
            }

            return FolderProvider.Instance(FolderMappingController.Instance.GetFolderMapping(folder.FolderMappingID).FolderProviderType).SupportsMappedPaths &&
                this.GetFolder(folder.ParentID).FolderMappingID != folder.FolderMappingID;
        }

        private void UnmapFolderInternal(IFolderInfo folder, bool isCascadeDeleting)
        {
            Requires.NotNull("folder", folder);

            if (DirectoryWrapper.Instance.Exists(folder.PhysicalPath))
            {
                DirectoryWrapper.Instance.Delete(folder.PhysicalPath, true);
            }

            this.DeleteFolder(folder.PortalID, folder.FolderPath);

            // Notify folder deleted event
            this.OnFolderDeleted(folder, this.GetCurrentUserId(), isCascadeDeleting);
        }

        private void DeleteFolderInternal(IFolderInfo folder, bool isCascadeDeleting)
        {
            Requires.NotNull("folder", folder);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            try
            {
                FolderProvider.Instance(folderMapping.FolderProviderType).DeleteFolder(folder);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(
                    Localization.GetExceptionMessage(
                        "DeleteFolderUnderlyingSystemError",
                        "The underlying system threw an exception. The folder has not been deleted."),
                    ex);
            }

            if (DirectoryWrapper.Instance.Exists(folder.PhysicalPath))
            {
                DirectoryWrapper.Instance.Delete(folder.PhysicalPath, true);
            }

            this.DeleteFolder(folder.PortalID, folder.FolderPath);

            // Notify folder deleted event
            this.OnFolderDeleted(folder, this.GetCurrentUserId(), isCascadeDeleting);
        }

        private IFolderInfo GetParentFolder(int portalId, string folderPath)
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                var parentFolderPath = folderPath.Substring(0, folderPath.Substring(0, folderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
                return this.GetFolder(portalId, parentFolderPath);
            }

            return null;
        }

        private IEnumerable<IFileInfo> SearchFiles(IFolderInfo folder, Regex regex, bool recursive)
        {
            var fileCollection =
                CBO.Instance.FillCollection<FileInfo>(DataProvider.Instance().GetFiles(folder.FolderID, false, false));

            var files = (from f in fileCollection where regex.IsMatch(f.FileName) select f).Cast<IFileInfo>().ToList();

            if (recursive)
            {
                foreach (var subFolder in this.GetFolders(folder))
                {
                    if (FolderPermissionController.Instance.CanViewFolder(subFolder))
                    {
                        files.AddRange(this.SearchFiles(subFolder, regex, true));
                    }
                }
            }

            return files;
        }

        private IFolderInfo UpdateFolderInternal(IFolderInfo folder, bool clearCache)
        {
            Requires.NotNull("folder", folder);

            DataProvider.Instance().UpdateFolder(
                folder.PortalID,
                folder.VersionGuid,
                folder.FolderID,
                PathUtils.Instance.FormatFolderPath(folder.FolderPath),
                folder.StorageLocation,
                folder.MappedPath,
                folder.IsProtected,
                folder.IsCached,
                folder.LastUpdated,
                this.GetCurrentUserId(),
                folder.FolderMappingID,
                folder.IsVersioned,
                folder.WorkflowID,
                folder.ParentID);

            if (clearCache)
            {
                this.ClearFolderCache(folder.PortalID);
            }

            return folder;
        }

        private int FindFolderMappingId(MergedTreeItem item, int portalId)
        {
            if (item.ExistsInFolderMapping)
            {
                return item.FolderMappingID;
            }

            if (item.FolderPath.IndexOf('/') != item.FolderPath.LastIndexOf('/'))
            {
                var parentPath = item.FolderPath.Substring(0, item.FolderPath.TrimEnd('/').LastIndexOf('/') + 1);
                var folder = this.GetFolder(portalId, parentPath);
                if (folder != null)
                {
                    return folder.FolderMappingID;
                }
            }

            return FolderMappingController.Instance.GetDefaultFolderMapping(portalId).FolderMappingID;
        }

        private bool DeleteFolderRecursive(IFolderInfo folder, ICollection<IFolderInfo> notDeletedSubfolders, bool isRecursiveDeletionFolder, bool unmap)
        {
            Requires.NotNull("folder", folder);

            if (UserSecurityController.Instance.HasFolderPermission(folder, "DELETE"))
            {
                var subfolders = this.GetFolders(folder);

                var allSubFoldersHasBeenDeleted = true;

                foreach (var subfolder in subfolders)
                {
                    if (!this.DeleteFolderRecursive(subfolder, notDeletedSubfolders, false, unmap || this.GetOnlyUnmap(subfolder)))
                    {
                        allSubFoldersHasBeenDeleted = false;
                    }
                }

                var files = this.GetFiles(folder, false, true);
                foreach (var file in files)
                {
                    if (unmap)
                    {
                        FileDeletionController.Instance.UnlinkFile(file);
                    }
                    else
                    {
                        FileDeletionController.Instance.DeleteFile(file);
                    }

                    this.OnFileDeleted(file, this.GetCurrentUserId(), true);
                }

                if (allSubFoldersHasBeenDeleted)
                {
                    if (unmap)
                    {
                        this.UnmapFolderInternal(folder, !isRecursiveDeletionFolder);
                    }
                    else
                    {
                        this.DeleteFolderInternal(folder, !isRecursiveDeletionFolder);
                    }

                    return true;
                }
            }

            notDeletedSubfolders.Add(folder);
            return false;
        }

        private string GetDefaultMappedPath(FolderMappingInfo folderMapping)
        {
            var defaultMappedPath = folderMapping.FolderMappingSettings[DefaultMappedPathSetting];
            if (defaultMappedPath == null)
            {
                return string.Empty;
            }

            return defaultMappedPath.ToString();
        }

        private IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder, bool allSubFolders)
        {
            Requires.NotNull("parentFolder", parentFolder);

            if (allSubFolders)
            {
                var subFolders =
                    this.GetFolders(parentFolder.PortalID)
                        .Where(
                            f =>
                                f.FolderPath.StartsWith(
                                    parentFolder.FolderPath,
                                    StringComparison.InvariantCultureIgnoreCase));

                return subFolders.Where(f => f.FolderID != parentFolder.FolderID);
            }

            return this.GetFolders(parentFolder.PortalID).Where(f => f.ParentID == parentFolder.FolderID);
        }

        private void OnFolderMoved(IFolderInfo folderInfo, int userId, string oldFolderPath)
        {
            EventManager.Instance.OnFolderMoved(new FolderMovedEventArgs
            {
                FolderInfo = folderInfo,
                UserId = userId,
                OldFolderPath = oldFolderPath,
            });
        }

        private void OnFolderRenamed(IFolderInfo folderInfo, int userId, string oldFolderName)
        {
            EventManager.Instance.OnFolderRenamed(new FolderRenamedEventArgs
            {
                FolderInfo = folderInfo,
                UserId = userId,
                OldFolderName = oldFolderName,
            });
        }

        private void OnFolderDeleted(IFolderInfo folderInfo, int userId, bool isCascadeDeleting)
        {
            EventManager.Instance.OnFolderDeleted(new FolderDeletedEventArgs
            {
                FolderInfo = folderInfo,
                UserId = userId,
                IsCascadeDeletng = isCascadeDeleting,
            });
        }

        private void OnFolderAdded(IFolderInfo folderInfo, int userId)
        {
            EventManager.Instance.OnFolderAdded(new FolderChangedEventArgs
            {
                FolderInfo = folderInfo,
                UserId = userId,
            });
        }

        private void OnFileDeleted(IFileInfo fileInfo, int userId, bool isCascadeDeleting)
        {
            EventManager.Instance.OnFileDeleted(new FileDeletedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
                IsCascadeDeleting = isCascadeDeleting,
            });
        }

        private FolderPermissionCollection GetFolderPermissionsFromSyncData(int portalId, string relativePath)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            FolderPermissionCollection permissions = null;
            if (SyncFoldersData.ContainsKey(threadId))
            {
                if (SyncFoldersData[threadId].FolderPath == relativePath && SyncFoldersData[threadId].PortalId == portalId)
                {
                    return SyncFoldersData[threadId].Permissions;
                }

                permissions = FolderPermissionController.GetFolderPermissionsCollectionByFolder(portalId, relativePath);
                SyncFoldersData[threadId] = new SyncFolderData { PortalId = portalId, FolderPath = relativePath, Permissions = permissions };
                return permissions;
            }

            permissions = FolderPermissionController.GetFolderPermissionsCollectionByFolder(portalId, relativePath);
            SyncFoldersData.Add(threadId, new SyncFolderData { PortalId = portalId, FolderPath = relativePath, Permissions = permissions });

            return permissions;
        }

        private void InitialiseSyncFoldersData(int portalId, string relativePath)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var permissions = FolderPermissionController.GetFolderPermissionsCollectionByFolder(portalId, relativePath);
            if (SyncFoldersData.ContainsKey(threadId))
            {
                if (SyncFoldersData[threadId].FolderPath == relativePath && SyncFoldersData[threadId].PortalId == portalId)
                {
                    SyncFoldersData[threadId].Permissions = permissions;
                }
                else
                {
                    SyncFoldersData[threadId] = new SyncFolderData { PortalId = portalId, FolderPath = relativePath, Permissions = permissions };
                }
            }
            else
            {
                SyncFoldersData.Add(threadId, new SyncFolderData { PortalId = portalId, FolderPath = relativePath, Permissions = permissions });
            }
        }

        private void RemoveSyncFoldersData(string relativePath)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            if (SyncFoldersData.ContainsKey(threadId))
            {
                SyncFoldersData.Remove(threadId);
            }
        }

        /// <summary>
        /// This class and its members are reserved for internal use and are not intended to be used in your code.
        /// </summary>
        internal class MergedTreeItem
        {
            public bool ExistsInFileSystem { get; set; }

            public bool ExistsInDatabase { get; set; }

            public bool ExistsInFolderMapping { get; set; }

            public int FolderID { get; set; }

            public int FolderMappingID { get; set; }

            public string FolderPath { get; set; }

            public string MappedPath { get; set; }
        }

        /// <summary>
        /// This class and its members are reserved for internal use and are not intended to be used in your code.
        /// </summary>
        internal class IgnoreCaseStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.Compare(x.ToLowerInvariant(), y.ToLowerInvariant(), StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// This class and its members are reserved for internal use and are not intended to be used in your code.
        /// </summary>
        internal class MoveFoldersInfo
        {
            public MoveFoldersInfo(string source, string target)
            {
                this.Source = source;
                this.Target = target;
            }

            public string Source { get; private set; }

            public string Target { get; private set; }
        }
    }

    internal class SyncFolderData
    {
        public int PortalId { get; set; }

        public string FolderPath { get; set; }

        public FolderPermissionCollection Permissions { get; set; }
    }
}
