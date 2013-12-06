#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem.EventArgs;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Services.Log.EventLog;

namespace DotNetNuke.Services.FileSystem
{
    /// <summary>
    /// Exposes methods to manage folders.
    /// </summary>
    public class FolderManager : ComponentBase<IFolderManager, FolderManager>, IFolderManager
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FolderManager));

        private const string DefaultUsersFoldersPath = "Users";
        
        #region Private Events
        private event EventHandler<FolderDeletedEventArgs> FolderDeleted;
        private event EventHandler<FolderChangedEventArgs> FolderAdded;
        private event EventHandler<FolderMovedEventArgs> FolderMoved;
        private event EventHandler<FolderRenamedEventArgs> FolderRenamed;
        private event EventHandler<FileDeletedEventArgs> FileDeleted;
        #endregion

        #region Constructor

        internal FolderManager()
        {
            RegisterEventHandlers();
        }

        #endregion

        #region Public Properties

        public virtual string MyFolderName
        {
            get
            {
                return Localization.Localization.GetString("MyFolderName");
            }
        }

        #endregion

        #region Private Methods

        private void RegisterEventHandlers()
        {
            foreach (var value in FileEventHandlersContainer.Instance.FileEventsHandlers.Select(e => e.Value))
            {
                FolderDeleted += value.FolderDeleted;
                FolderRenamed += value.FolderRenamed;
                FolderMoved += value.FolderMoved;
                FolderAdded += value.FolderAdded;
                FileDeleted += value.FileDeleted;
            }
        }

        private int AddFolderInternal(IFolderInfo folder)
        {
            //Check this is not a duplicate
            var tmpfolder = GetFolder(folder.PortalID, folder.FolderPath);

            if (tmpfolder != null && folder.FolderID == Null.NullInteger)
            {
                folder.FolderID = tmpfolder.FolderID;
            }

            if (folder.FolderID == Null.NullInteger)
            {
                var isVersioned = folder.IsVersioned;
                var workflowId = folder.WorkflowID;

                // Inherit some configuration from its Parent Folder
                var parentFolder = GetParentFolder(folder.PortalID, folder.FolderPath);
                var parentId = Null.NullInteger;
                if (parentFolder != null)
                {
                    isVersioned = parentFolder.IsVersioned;
                    workflowId = parentFolder.WorkflowID;
                    parentId = parentFolder.FolderID;
                }

                folder.FolderPath = PathUtils.Instance.FormatFolderPath(folder.FolderPath);
                folder.FolderID = DataProvider.Instance().AddFolder(folder.PortalID,
                                                                    folder.UniqueId,
                                                                    folder.VersionGuid,
                                                                    folder.FolderPath,
                                                                    folder.MappedPath,
                                                                    folder.StorageLocation,
                                                                    folder.IsProtected,
                                                                    folder.IsCached,
                                                                    folder.LastUpdated,
                                                                    GetCurrentUserId(),
                                                                    folder.FolderMappingID,
                                                                    isVersioned,
                                                                    workflowId,
                                                                    parentId);

                //Refetch folder for logging
                folder = GetFolder(folder.PortalID, folder.FolderPath);

                AddLogEntry(folder, EventLogController.EventLogType.FOLDER_CREATED);

                if (parentFolder != null)
                {
                    UpdateFolderInternal(parentFolder, false);
                }
                else
                {
                    UpdateParentFolder(folder.PortalID, folder.FolderPath);
                }
            }
            else
            {
                var parentFolder = GetParentFolder(folder.PortalID, folder.FolderPath);
                if (parentFolder != null)
                {
                    // Ensure that Parent Id is repaired
                    folder.ParentID = parentFolder.FolderID;
                }
                UpdateFolderInternal(folder, false);
            }

            //Invalidate Cache
            ClearFolderCache(folder.PortalID);

            return folder.FolderID;
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
                    Localization.Localization.GetExceptionMessage("DeleteFolderUnderlyingSystemError",
                                                                  "The underlying system threw an exception. The folder has not been deleted."),
                    ex);
            }

            if (DirectoryWrapper.Instance.Exists(folder.PhysicalPath))
            {
                DirectoryWrapper.Instance.Delete(folder.PhysicalPath, false);
            }
            DeleteFolder(folder.PortalID, folder.FolderPath);

            // Notify folder deleted event
            OnFolderDeleted(folder, GetCurrentUserId(), isCascadeDeleting);
        }

        private IFolderInfo GetParentFolder(int portalId, string folderPath)
        {
            if (!String.IsNullOrEmpty(folderPath))
            {
                var parentFolderPath = folderPath.Substring(0, folderPath.Substring(0, folderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
                return GetFolder(portalId, parentFolderPath);
            }

            return null;
        }

        private IEnumerable<IFileInfo> SearchFiles(IFolderInfo folder, Regex regex, bool recursive)
        {
            var fileCollection = CBOWrapper.Instance.FillCollection<FileInfo>(DataProvider.Instance().GetFiles(folder.FolderID));

            var files = (from f in fileCollection where regex.IsMatch(f.FileName) select f).Cast<IFileInfo>().ToList();

            if (recursive)
            {
                foreach (var subFolder in GetFolders(folder))
                {
                    if (FolderPermissionControllerWrapper.Instance.CanViewFolder(subFolder))
                    {
                        files.AddRange(SearchFiles(subFolder, regex, true));
                    }
                }
            }

            return files;
        }

        private IFolderInfo UpdateFolderInternal(IFolderInfo folder, bool clearCache)
        {
            Requires.NotNull("folder", folder);

            DataProvider.Instance().UpdateFolder(folder.PortalID,
                                                    folder.VersionGuid,
                                                    folder.FolderID,
                                                    PathUtils.Instance.FormatFolderPath(folder.FolderPath),
                                                    folder.StorageLocation,
                                                    folder.MappedPath,
                                                    folder.IsProtected,
                                                    folder.IsCached,
                                                    folder.LastUpdated,
                                                    GetCurrentUserId(),
                                                    folder.FolderMappingID,
                                                    folder.IsVersioned,
                                                    folder.WorkflowID,
                                                    folder.ParentID);

            if (clearCache)
            {
                ClearFolderCache(folder.PortalID);
            }

            return folder;
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

            return new Regex(pattern, RegexOptions.IgnoreCase);
        }
        
        private static bool IsStandardFolderProviderType(FolderMappingInfo folderMappingInfo)
        {
            var compatibleTypes = new[] { "StandardFolderProvider", "SecureFolderProvider", "DatabaseFolderProvider" };
            return compatibleTypes.Contains(folderMappingInfo.FolderProviderType);
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
				var folder = GetFolder(portalId, parentPath);
				if (folder != null)
				{
					return folder.FolderMappingID;
				}
			}

			return FolderMappingController.Instance.GetDefaultFolderMapping(portalId).FolderMappingID;
		}

        private bool DeleteFolderRecursive(IFolderInfo folder, ICollection<IFolderInfo> notDeletedSubfolders, bool isRecursiveDeletionFolder)
        {
            Requires.NotNull("folder", folder);

            if (UserSecurityController.Instance.HasFolderPermission(folder, "DELETE"))
            {
                var subfolders = GetFolders(folder);

                var allSubFoldersHasBeenDeleted = true;

                foreach (var subfolder in subfolders)
                {
                    if (!DeleteFolderRecursive(subfolder, notDeletedSubfolders, false))
                    {
                        allSubFoldersHasBeenDeleted = false;
                    }
                }

                var files = GetFiles(folder, false, true);
                foreach (var file in files)
                {
                    FileDeletionController.Instance.DeleteFile(file);
                    OnFileDeleted(file, GetCurrentUserId(), true);
                }

                if (allSubFoldersHasBeenDeleted)
                {
                    DeleteFolderInternal(folder, !isRecursiveDeletionFolder);                    
                    return true;
                }
            }

            notDeletedSubfolders.Add(folder);
            return false;
        }

        #region On Folder Events
        private void OnFolderMoved(IFolderInfo folderInfo, int userId, string oldFolderPath)
        {
            if (FolderMoved != null)
            {
                FolderMoved(this, new FolderMovedEventArgs
                {
                    FolderInfo = folderInfo,
                    UserId = userId,
                    OldFolderPath = oldFolderPath
                });
            }
        }

        private void OnFolderRenamed(IFolderInfo folderInfo, int userId, string oldFolderName)
        {
            if (FolderRenamed != null)
            {
                FolderRenamed(this, new FolderRenamedEventArgs
                {
                    FolderInfo = folderInfo,
                    UserId = userId,
                    OldFolderName = oldFolderName
                });
            }
        }

        private void OnFolderDeleted(IFolderInfo folderInfo, int userId, bool isCascadeDeleting)
        {
            if (FolderDeleted != null)
            {
                FolderDeleted(this, new FolderDeletedEventArgs
                {
                    FolderInfo = folderInfo,
                    UserId = userId,
                    IsCascadeDeletng = isCascadeDeleting
                });
            }
        }

        private void OnFolderAdded(IFolderInfo folderInfo, int userId)
        {
            if (FolderAdded != null)
            {
                FolderAdded(this, new FolderChangedEventArgs
                {
                    FolderInfo = folderInfo,
                    UserId = userId
                });
            }
        }

        private void OnFileDeleted(IFileInfo fileInfo, int userId, bool isCascadeDeleting)
        {
            if (FileDeleted != null)
            {
                FileDeleted(this, new FileDeletedEventArgs
                {
                    FileInfo = fileInfo,
                    UserId = userId,
                    IsCascadeDeleting = isCascadeDeleting
                });
            }
        }
        #endregion

        #endregion

        #region Public Methods

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
            return AddFolder(folderMapping, folderPath, folderPath);
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
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            if (FolderExists(folderMapping.PortalID, folderPath))
            {
                throw new FolderAlreadyExistsException(Localization.Localization.GetExceptionMessage("AddFolderAlreadyExists", "The provided folder path already exists. The folder has not been added."));
            }

            var parentFolder = GetParentFolder(folderMapping.PortalID, folderPath);
            if (parentFolder != null)
            {
                var parentFolderMapping = FolderMappingController.Instance.GetFolderMapping(parentFolder.PortalID, parentFolder.FolderMappingID);
                if (FolderProvider.Instance(parentFolderMapping.FolderProviderType).SupportsMappedPaths)
                {
                    folderMapping = parentFolderMapping;
                    mappedPath =
                        PathUtils.Instance.FormatFolderPath(parentFolder.MappedPath +
                                                            folderPath.Replace(parentFolder.FolderPath, string.Empty));
                }
                else if (!FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMappedPaths)
                {
                    mappedPath = folderPath;
                }
            }

            try
            {
                FolderProvider.Instance(folderMapping.FolderProviderType).AddFolder(folderPath, folderMapping, mappedPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(Localization.Localization.GetExceptionMessage("AddFolderUnderlyingSystemError", "The underlying system threw an exception. The folder has not been added."), ex);
            }

            CreateFolderInFileSystem(PathUtils.Instance.GetPhysicalPath(folderMapping.PortalID, folderPath));
            var folderId = CreateFolderInDatabase(folderMapping.PortalID, folderPath, folderMapping.FolderMappingID, mappedPath);

            var folder = GetFolder(folderId);

            // Notify add folder event
            OnFolderAdded(folder, GetCurrentUserId());

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
            var parentFolder = GetFolder(portalId, parentFolderPath) ?? AddFolder(portalId, parentFolderPath);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, parentFolder.FolderMappingID);

            return AddFolder(folderMapping, folderPath);
        }

        /// <summary>
        /// Deletes the specified folder.
        /// </summary>
        /// <param name="folder">The folder to delete.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual void DeleteFolder(IFolderInfo folder)
        {
            DeleteFolderInternal(folder, false);
        }

        /// <summary>
        /// Deletes the specified folder.
        /// </summary>
        /// <param name="folderId">The folder identifier.</param>
        public virtual void DeleteFolder(int folderId)
        {
            var folder = GetFolder(folderId);

            DeleteFolder(folder);
        }

        /// <summary>
        /// Deletes the specified folder and all its content
        /// </summary>
        /// <param name="folder">The folder to delete</param>
        /// <param name="notDeletedSubfolders">A collection with all not deleted subfolders after processiong the action</param>
        /// <returns></returns>
        public void DeleteFolder(IFolderInfo folder, ICollection<IFolderInfo> notDeletedSubfolders)
        {
            DeleteFolderRecursive(folder, notDeletedSubfolders, true);
        }

       /// <summary>
        /// Checks the existence of the specified folder in the specified portal.
        /// </summary>
        /// <param name="portalId">The portal where to check the existence of the folder.</param>
        /// <param name="folderPath">The path of folder to check the existence of.</param>
        /// <returns>A bool value indicating whether the folder exists or not in the specified portal.</returns>
        public virtual bool FolderExists(int portalId, string folderPath)
        {
            Requires.NotNull("folderPath", folderPath);

            return GetFolder(portalId, folderPath) != null;
        }

        /// <summary>
        /// Gets the files contained in the specified folder.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> GetFiles(IFolderInfo folder)
        {
            return GetFiles(folder, false);
        }

        /// <summary>
        /// Gets the files contained in the specified folder.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="recursive">Whether or not to include all the subfolders</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, bool recursive)
        {
            return GetFiles(folder, recursive, false);
        }

        /// <summary>
        /// Gets the files contained in the specified folder.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="recursive">Whether or not to include all the subfolders</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, bool recursive, bool retrieveUnpublishedFiles)
        {
            Requires.NotNull("folder", folder);

            var fileCollection = CBOWrapper.Instance.FillCollection<FileInfo>(DataProvider.Instance().GetFiles(folder.FolderID, retrieveUnpublishedFiles));

            var files = fileCollection.Cast<IFileInfo>().ToList();

            if (recursive)
            {
                foreach (var subFolder in GetFolders(folder))
                {
                    files.AddRange(GetFiles(subFolder, true, retrieveUnpublishedFiles));
                }
            }

            return files;
        }

        /// <summary>
        /// Gets the list of Standard folders the specified user has the provided permissions.
        /// </summary>
        /// <param name="user">The user info</param>
        /// <param name="permissions">The permissions the folders have to met.</param>
        /// <returns>The list of Standard folders the specified user has the provided permissions.</returns>
        /// <remarks>This method is used to support legacy behaviours and situations where we know the file/folder is in the file system.</remarks>
        public virtual IEnumerable<IFolderInfo> GetFileSystemFolders(UserInfo user, string permissions)
        {
            var userFolders = new List<IFolderInfo>();

            var portalId = user.PortalID;

            var userFolder = GetUserFolder(user);

            var defaultFolderMaping = FolderMappingController.Instance.GetDefaultFolderMapping(portalId);

            var folders = GetFolders(portalId, permissions, user.UserID).Where(f => f.FolderPath != null && f.FolderMappingID == defaultFolderMaping.FolderMappingID);

            foreach (var folder in folders)
            {
                if (folder.FolderPath.StartsWith(DefaultUsersFoldersPath + "/", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (folder.FolderID == userFolder.FolderID)
                    {
                        folder.DisplayPath = MyFolderName + "/";
                        folder.DisplayName = MyFolderName;
                    }
                    else if (!folder.FolderPath.StartsWith(userFolder.FolderPath, StringComparison.InvariantCultureIgnoreCase)) //Allow UserFolder children
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
            //Try and get the folder from the portal cache
            IFolderInfo folder = null;
            var portalSettings = PortalController.GetCurrentPortalSettings();
            if (portalSettings != null)
            {
                var folders = GetFolders(portalSettings.PortalId);
                folder = folders.SingleOrDefault(f => f.FolderID == folderId) ?? CBOWrapper.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolder(folderId));
            }

            return folder ?? (CBOWrapper.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolder(folderId)));
        }

        /// <summary>
        /// Gets a folder entity by providing a portal identifier and folder path.
        /// </summary>
        /// <param name="portalId">The portal where the folder exists.</param>
        /// <param name="folderPath">The path of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        public virtual IFolderInfo GetFolder(int portalId, string folderPath)
        {
            Requires.NotNull("folderPath", folderPath);

            folderPath = PathUtils.Instance.FormatFolderPath(folderPath);

            var folders = GetFolders(portalId);
            return folders.SingleOrDefault(f => f.FolderPath == folderPath) ?? CBOWrapper.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolder(portalId, folderPath));
        }

        /// <summary>
        /// Gets a folder entity by providing its unique id.
        /// </summary>
        /// <param name="uniqueId">The unique id of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        public virtual IFolderInfo GetFolder(Guid uniqueId)
        {
            return CBOWrapper.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolderByUniqueID(uniqueId));
        }

        /// <summary>
        /// Gets the list of subfolders for the specified folder.
        /// </summary>
        /// <param name="parentFolder">The folder to get the list of subfolders.</param>
        /// <returns>The list of subfolders for the specified folder.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when parentFolder is null.</exception>
        public virtual IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder)
        {
            Requires.NotNull("parentFolder", parentFolder);

            return GetFolders(parentFolder.PortalID).Where(f => f.ParentID == parentFolder.FolderID);
        }

        /// <summary>
        /// Gets the sorted list of folders of the provided portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="useCache">True = Read from Cache, False = Read from DB </param>
        /// <returns>The sorted list of folders of the provided portal.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(int portalId, bool useCache)
        {
            if (!useCache)
            {
                ClearFolderCache(portalId);
            }

            return GetFolders(portalId);
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
            CBOWrapper.Instance.GetCachedObject<List<FolderInfo>>(new CacheItemArgs(cacheKey, DataCache.FolderCacheTimeOut, DataCache.FolderCachePriority, portalId), GetFoldersSortedCallBack).ForEach(folders.Add);

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
            CBOWrapper.Instance.GetCachedObject<List<FolderInfo>>(cacheItemArgs, GetFoldersByPermissionSortedCallBack).ForEach(folders.Add);

            return folders;
        }

        /// <summary>
        /// Gets the list of folders the specified user has read permissions
        /// </summary>
        /// <param name="user">The user info</param>
        /// <returns>The list of folders the specified user has read permissions.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(UserInfo user)
        {
            return GetFolders(user, "READ");
        }

        /// <summary>
        /// Gets the list of folders the specified user has the provided permissions
        /// </summary>
        /// <param name="user">The user info</param>
        /// <param name="permissions">The permissions the folders have to met.</param>
        /// <returns>The list of folders the specified user has the provided permissions.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(UserInfo user, string permissions)
        {
            var userFolders = new List<IFolderInfo>();

            var portalId = user.PortalID;

            var userFolder = GetUserFolder(user);

            foreach (var folder in GetFolders(portalId, permissions, user.UserID).Where(folder => folder.FolderPath != null))
            {
                if (folder.FolderPath.StartsWith(DefaultUsersFoldersPath + "/", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (folder.FolderID == userFolder.FolderID)
                    {
                        folder.DisplayPath = Localization.Localization.GetString("MyFolderName") + "/";
                        folder.DisplayName = Localization.Localization.GetString("MyFolderName");
                    }
                    else if (!folder.FolderPath.StartsWith(userFolder.FolderPath, StringComparison.InvariantCultureIgnoreCase)) //Allow UserFolder children
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
            //always use _default portal for a super user
            int portalId = userInfo.IsSuperUser ? -1 : userInfo.PortalID;

            string userFolderPath = ((PathUtils)PathUtils.Instance).GetUserFolderPathInternal(userInfo);
            return GetFolder(portalId, userFolderPath) ?? AddUserFolder(userInfo);
        }

        public virtual IFolderInfo MoveFolder(IFolderInfo folder, IFolderInfo destinationFolder)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNull("destinationFolder", destinationFolder);

            var newFolderPath = PathUtils.Instance.FormatFolderPath(destinationFolder.FolderPath + folder.FolderName + "/");
            
            if (folder.FolderPath == destinationFolder.FolderPath) return folder;

            if(FolderExists(folder.PortalID, newFolderPath))
            {
                throw new InvalidOperationException(string.Format(
                    Localization.Localization.GetExceptionMessage("CannotMoveFolderAlreadyExists", 
                    "The folder with name '{0}' cannot be moved. A folder with that name already exists under the folder '{1}'.", folder.FolderName, destinationFolder.FolderName)));
            }
            
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var destinationFolderMapping = FolderMappingController.Instance.GetFolderMapping(destinationFolder.PortalID, destinationFolder.FolderMappingID);

            if(!CanMoveBetweenFolderMappings(folderMapping, destinationFolderMapping))
            {
                throw new InvalidOperationException(string.Format(
                    Localization.Localization.GetExceptionMessage("CannotMoveFolderBetweenFolderType", 
                    "The folder with name '{0}' cannot be moved. Move Folder operation between this two folder types is not allowed", folder.FolderName)));
            }

            if (!IsMoveOperationValid(folder, destinationFolder, newFolderPath))
            {
                throw new InvalidOperationException(Localization.Localization.GetExceptionMessage("MoveFolderCannotComplete", "The operation cannot be completed."));
            }

            var currentFolderPath = folder.FolderPath;

            if ((folder.FolderMappingID == destinationFolder.FolderMappingID && FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMoveFolder) ||
                (IsStandardFolderProviderType(folderMapping) && IsStandardFolderProviderType(destinationFolderMapping)))
            {
                MoveFolderWithinProvider(folder, destinationFolder);
            }
            else
            {
                MoveFolderBetweenProviders(folder, newFolderPath);
            }

            //Files in cache are obsolete because their physical path is not correct after moving
            DeleteFilesFromCache(folder.PortalID, newFolderPath);
            var movedFolder = GetFolder(folder.FolderID);

            // Notify folder moved event
            OnFolderMoved(folder, GetCurrentUserId(), currentFolderPath);

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

            if (folder.FolderName.Equals(newFolderName)) return;

            var currentFolderName = folder.FolderName;

            var newFolderPath = folder.FolderPath.Substring(0, folder.FolderPath.LastIndexOf(folder.FolderName, StringComparison.Ordinal)) + PathUtils.Instance.FormatFolderPath(newFolderName);

            if (FolderExists(folder.PortalID, newFolderPath))
            {
                throw new FolderAlreadyExistsException(Localization.Localization.GetExceptionMessage("RenameFolderAlreadyExists", "The destination folder already exists. The folder has not been renamed."));
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, GetParentFolder(folder.PortalID, folder.FolderPath).FolderMappingID);
            var provider = FolderProvider.Instance(folderMapping.FolderProviderType);

            RenameFolderInFileSystem(folder, newFolderPath);

            //Update Provider
            provider.RenameFolder(folder, newFolderName);

            //Update database
            UpdateChildFolders(folder, newFolderPath);

            // Notify folder renamed event
            OnFolderRenamed(folder, GetCurrentUserId(), currentFolderName);
        }

        /// <summary>
        /// Search the files contained in the specified folder, for a matching pattern
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="pattern">The patter to search for</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> SearchFiles(IFolderInfo folder, string pattern, bool recursive)
        {
            Requires.NotNull("folder", folder);

            if (!FolderPermissionControllerWrapper.Instance.CanViewFolder(folder))
            {
                throw new FolderProviderException("No permission to view the folder");
            }

            return SearchFiles(folder, WildcardToRegex(pattern), recursive);
        }

        /// <summary>
        /// Synchronizes the entire folder tree for the specified portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The number of folder collisions.</returns>
        public virtual int Synchronize(int portalId)
        {
            var folderCollisions = Synchronize(portalId, "", true, true);

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
            return Synchronize(portalId, relativePath, true, true);
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
            Requires.NotNull("relativePath", relativePath);

            if (AreThereFolderMappingsRequiringNetworkConnectivity(portalId, relativePath, isRecursive) && !IsNetworkAvailable())
            {
                throw new NoNetworkAvailableException(Localization.Localization.GetExceptionMessage("NoNetworkAvailableError", "Network connectivity is needed but there is no network available."));
            }

            int? scriptTimeOut = null;

            try
            {
                if (HttpContext.Current != null)
                {
                    scriptTimeOut = GetCurrentScriptTimeout();

                    // Synchronization could be a time-consuming process. To not get a time-out, we need to modify the request time-out value
                    SetScriptTimeout(int.MaxValue);
                }

                var mergedTree = GetMergedTree(portalId, relativePath, isRecursive);

                // Step 1: Add Folders
                for (var i = 0; i < mergedTree.Count; i++)
                {
                    var item = mergedTree.Values[i];
                    ProcessMergedTreeItemInAddMode(item, portalId);
                }

                // Step 2: Delete Files and Folders
                for (var i = mergedTree.Count - 1; i >= 0; i--)
                {
                    var item = mergedTree.Values[i];

                    if (syncFiles)
                    {
                        SynchronizeFiles(item, portalId);
                    }

                    ProcessMergedTreeItemInDeleteMode(item, portalId);
                }
            }
            finally
            {
                // Restore original time-out
                if (HttpContext.Current != null && scriptTimeOut != null)
                {
                    SetScriptTimeout(scriptTimeOut.Value);
                }
            }

            return 0;
        }

        /// <summary>
        /// Updates metadata of the specified folder.
        /// </summary>
        /// <param name="folder">The folder to update.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        public virtual IFolderInfo UpdateFolder(IFolderInfo folder)
        {
            var updatedFolder = UpdateFolderInternal(folder, true);

            AddLogEntry(updatedFolder, EventLogController.EventLogType.FOLDER_UPDATED);
            
            SaveFolderPermissions(updatedFolder);

            return updatedFolder;
        }

        #endregion

        #region Permission Methods

        /// <summary>
        /// Adds read permissions for all users to the specified folder.
        /// </summary>
        /// <param name="folder">The folder to add the permission to.</param>
        /// <param name="permission">Used as base class for FolderPermissionInfo when there is no read permission already defined.</param>
        public virtual void AddAllUserReadPermission(IFolderInfo folder, PermissionInfo permission)
        {
            var roleId = Int32.Parse(Globals.glbRoleAllUsers);

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
                    AllowAccess = true
                };

                folder.FolderPermissions.Add(folderPermission);
            }
        }

        /// <summary>
        /// Sets folder permissions to the given folder by copying parent folder permissions.
        /// </summary>
        /// <param name="folder">The folder to copy permissions to</param>
        public virtual void CopyParentFolderPermissions(IFolderInfo folder)
        {
            Requires.NotNull("folder", folder);

            if (String.IsNullOrEmpty(folder.FolderPath)) return;

            var parentFolderPath = folder.FolderPath.Substring(0, folder.FolderPath.Substring(0, folder.FolderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);

            foreach (FolderPermissionInfo objPermission in
                FolderPermissionController.GetFolderPermissionsCollectionByFolder(folder.PortalID, parentFolderPath))
            {
                var folderPermission = new FolderPermissionInfo(objPermission)
                {
                    FolderID = folder.FolderID,
                    RoleID = objPermission.RoleID,
                    UserID = objPermission.UserID,
                    AllowAccess = objPermission.AllowAccess
                };
                folder.FolderPermissions.Add(folderPermission, true);
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }

        /// <summary>
        /// Sets specific folder permissions for the given role to the given folder.        
        /// </summary>
        /// <param name="folder">The folder to set permission to</param>
        /// <param name="permissionId">The id of the permission to assign</param>
        /// <param name="roleId">The role to assign the permission to</param>
        public virtual void SetFolderPermission(IFolderInfo folder, int permissionId, int roleId)
        {
            SetFolderPermission(folder, permissionId, roleId, Null.NullInteger);
        }

        /// <summary>
        /// Sets specific folder permissions for the given role/user to the given folder.
        /// </summary>
        /// <param name="folder">The folder to set permission to</param>
        /// <param name="permissionId">The id of the permission to assign</param>
        /// <param name="roleId">The role to assign the permission to</param>
        /// <param name="userId">The user to assign the permission to</param>
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
                AllowAccess = true
            };

            folder.FolderPermissions.Add(objFolderPermissionInfo, true);
            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }

        /// <summary>
        /// Sets folder permissions for administrator role to the given folder.
        /// </summary>
        /// <param name="folder">The folder to set permission to</param>
        /// <param name="administratorRoleId">The administrator role id to assign the permission to</param>  
        public virtual void SetFolderPermissions(IFolderInfo folder, int administratorRoleId)
        {
            Requires.NotNull("folder", folder);

            foreach (PermissionInfo objPermission in PermissionController.GetPermissionsByFolder())
            {
                var folderPermission = new FolderPermissionInfo(objPermission)
                {
                    FolderID = folder.FolderID,
                    RoleID = administratorRoleId
                };

                folder.FolderPermissions.Add(folderPermission, true);
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }

        #endregion

        #region Internal Methods

        internal virtual void AddLogEntry(IFolderInfo folder, EventLogController.EventLogType eventLogType)
        {
            var eventLogController = new EventLogController();
            eventLogController.AddLog(folder, PortalController.GetCurrentPortalSettings(), GetCurrentUserId(), "", eventLogType);
        }

        internal virtual void AddLogEntry(string propertyName, string propertyValue, EventLogController.EventLogType eventLogType)
        {
            var eventLogController = new EventLogController();
            eventLogController.AddLog(propertyName, propertyValue, PortalController.GetCurrentPortalSettings(), GetCurrentUserId(), eventLogType);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal void DeleteFilesFromCache(int portalId, string newFolderPath)
        {
            var folders = GetFolders(portalId).Where(f => f.FolderPath.StartsWith(newFolderPath));
            foreach (var folderInfo in folders)
            {
                var fileIds = GetFiles(folderInfo).Select(f => f.FileId);
                foreach (var fileId in fileIds)
                {
                    DataCache.RemoveCache("GetFileById" + fileId);
                }    
            }            
        }


        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual IFolderInfo AddUserFolder(UserInfo user)
        {
            //user _default portal for all super users
            var portalId = user.IsSuperUser ? -1 : user.PortalID;

            var defaultFolderMapping = FolderMappingController.Instance.GetDefaultFolderMapping(portalId);

            if (!FolderExists(portalId, DefaultUsersFoldersPath))
            {
                AddFolder(defaultFolderMapping, DefaultUsersFoldersPath);
            }

#pragma warning disable 612,618
            var rootFolder = PathUtils.Instance.GetUserFolderPathElement(user.UserID, PathUtils.UserFolderElement.Root);
#pragma warning restore 612,618

            var folderPath = PathUtils.Instance.FormatFolderPath(String.Format(DefaultUsersFoldersPath + "/{0}", rootFolder));

            if (!FolderExists(portalId, folderPath))
            {
                AddFolder(defaultFolderMapping, folderPath);
            }

#pragma warning disable 612,618
            folderPath = PathUtils.Instance.FormatFolderPath(String.Concat(folderPath, PathUtils.Instance.GetUserFolderPathElement(user.UserID, PathUtils.UserFolderElement.SubFolder)));
#pragma warning restore 612,618

            if (!FolderExists(portalId, folderPath))
            {
                AddFolder(defaultFolderMapping, folderPath);
            }

            folderPath = PathUtils.Instance.FormatFolderPath(String.Concat(folderPath, user.UserID.ToString(CultureInfo.InvariantCulture)));

            if (!FolderExists(portalId, folderPath))
            {
                AddFolder(defaultFolderMapping, folderPath);

                var folder = GetFolder(portalId, folderPath);

                foreach (PermissionInfo permission in PermissionController.GetPermissionsByFolder())
                {
                    if (permission.PermissionKey.ToUpper() == "READ" || permission.PermissionKey.ToUpper() == "WRITE" || permission.PermissionKey.ToUpper() == "BROWSE")
                    {
                        var folderPermission = new FolderPermissionInfo(permission)
                        {
                            FolderID = folder.FolderID,
                            UserID = user.UserID,
                            RoleID = Null.NullInteger,
                            AllowAccess = true
                        };

                        folder.FolderPermissions.Add(folderPermission);

                        if (permission.PermissionKey.ToUpper() == "READ")
                        {
                            AddAllUserReadPermission(folder, permission);
                        }
                    }
                }

                FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
            }

            return GetFolder(portalId, folderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual bool AreThereFolderMappingsRequiringNetworkConnectivity(int portalId, string relativePath, bool isRecursive)
        {
            var folder = GetFolder(portalId, relativePath);

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
            return CreateFolderInDatabase(portalId, folderPath, folderMappingId, folderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
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
            var folder = new FolderInfo
                                {
                                    PortalID = portalId,
                                    FolderPath = folderPath,
                                    MappedPath = mappedPath,
                                    StorageLocation = storageLocation,
                                    IsProtected = isProtected,
                                    IsCached = false,
                                    FolderMappingID = folderMappingId,
                                    LastUpdated = Null.NullDate
                                };

            folder.FolderID = AddFolderInternal(folder);

            if (portalId != Null.NullInteger)
            {
                //Set Folder Permissions to inherit from parent
                CopyParentFolderPermissions(folder);
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
            AddLogEntry("FolderPath", folderPath, EventLogController.EventLogType.FOLDER_DELETED);
            UpdateParentFolder(portalId, folderPath);
            DataCache.ClearFolderCache(portalId);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void DeleteFoldersFromExternalStorageLocations(Dictionary<int, FolderMappingInfo> folderMappings, IEnumerable<IFolderInfo> foldersToDelete)
        {
            foreach (var folderToDelete in foldersToDelete)
            {
                // Delete source folder from its storage location
                var folderMapping = GetFolderMapping(folderMappings, folderToDelete.FolderMappingID);

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
        internal virtual int GetCurrentScriptTimeout()
        {
            return HttpContext.Current.Server.ScriptTimeout;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual int GetCurrentUserId()
        {
            return UserController.GetCurrentUserInfo().UserID;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual SortedList<string, MergedTreeItem> GetDatabaseFolders(int portalId, string relativePath, bool isRecursive)
        {
            var databaseFolders = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());

            var folder = GetFolder(portalId, relativePath);

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
                        MappedPath = folder.MappedPath
                    };

                    databaseFolders.Add(relativePath, item);
                }
                else
                {
                    databaseFolders = GetDatabaseFoldersRecursive(folder);
                }
            }

            return databaseFolders;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
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
                                    MappedPath = folderInfo.MappedPath
                                };

                if (!result.ContainsKey(item.FolderPath))
                {
                    result.Add(item.FolderPath, item);
                }

                foreach (var subfolder in GetFolders(folderInfo))
                {
                    stack.Push(subfolder);
                }
            }

            return result;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
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
                        MappedPath = String.Empty
                    };

                    fileSystemFolders.Add(relativePath, item);
                }
                else
                {
                    fileSystemFolders = GetFileSystemFoldersRecursive(portalId, physicalPath);
                }
            }

            return fileSystemFolders;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
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
                        MappedPath = String.Empty
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
        internal virtual FolderMappingInfo GetFolderMapping(Dictionary<int, FolderMappingInfo> folderMappings, int folderMappingId)
        {
            if (!folderMappings.ContainsKey(folderMappingId))
            {
                folderMappings.Add(folderMappingId, FolderMappingController.Instance.GetFolderMapping(folderMappingId));
            }

            return folderMappings[folderMappingId];
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
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
                var relativePath = (String.IsNullOrEmpty(mappedPath))
                                        ? String.Empty
                                        : (String.IsNullOrEmpty(baseMappedPath))
                                            ? mappedPath
                                            : mappedPath.Replace(baseMappedPath, "");

                var folderPath = baseFolderPath + relativePath;

                try
                {
                    if (folderProvider.FolderExists(mappedPath, folderMapping))
                    {
                        var item = new MergedTreeItem
                        {
                            FolderID = -1,
                            FolderMappingID = folderMapping.FolderMappingID,
                            FolderPath = folderPath,
                            ExistsInFolderMapping = true,
                            MappedPath = mappedPath
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
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return result;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual object GetFoldersByPermissionSortedCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var permissions = (string)cacheItemArgs.ParamList[1];
            var userId = (int)cacheItemArgs.ParamList[2];
            return CBOWrapper.Instance.FillCollection<FolderInfo>(DataProvider.Instance().GetFoldersByPortalAndPermissions(portalId, permissions, userId));
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual object GetFoldersSortedCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            return CBOWrapper.Instance.FillCollection<FolderInfo>(DataProvider.Instance().GetFoldersByPortal(portalId));
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual SortedList<string, MergedTreeItem> GetMergedTree(int portalId, string relativePath, bool isRecursive)
        {
            var fileSystemFolders = GetFileSystemFolders(portalId, relativePath, isRecursive);
            var databaseFolders = GetDatabaseFolders(portalId, relativePath, isRecursive);

            var mergedTree = MergeFolderLists(fileSystemFolders, databaseFolders);
            var mappedFolders = new SortedList<string, MergedTreeItem>();

            //Some providers cache the list of objects for performance
            ClearFolderProviderCachedLists(portalId);

            foreach (var mergedItem in mergedTree.Values)
            {
                if (mergedItem.FolderMappingID == Null.NullInteger) continue;

                var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, mergedItem.FolderMappingID);

                //Add any folders from non-core providers
                if (folderMapping.MappingName != "Standard" && folderMapping.MappingName != "Secure" && folderMapping.MappingName != "Database")
                {
                    if (!isRecursive)
                    {
                        mergedItem.ExistsInFolderMapping = true;
                    }
                    else
                    {
                        var folder = GetFolder(portalId, mergedItem.FolderPath);
                        mappedFolders = MergeFolderLists(mappedFolders, GetFolderMappingFoldersRecursive(folderMapping, folder));
                    }
                }
                else
                {
                    mergedItem.ExistsInFolderMapping = folderMapping.MappingName == "Database" ? mergedItem.ExistsInDatabase : mergedItem.ExistsInFileSystem;
                }
            }

            mergedTree = MergeFolderLists(mergedTree, mappedFolders);

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
        internal virtual bool IsFolderMappingEditable(FolderMappingInfo folderMapping)
        {
            return folderMapping.IsEditable;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual bool IsMoveOperationValid(IFolderInfo folderToMove, IFolderInfo destinationFolder, string newFolderPath)
        {
            //FolderMapping cases
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folderToMove.PortalID, folderToMove.FolderMappingID);
            if (folderToMove.FolderMappingID == destinationFolder.FolderMappingID && FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMappedPaths)
            {
                //Root mapped folder cannot be move, when folder mappings are equal
                if (folderToMove.MappedPath == string.Empty)
                {
                    return false;
                }

                //Destination folder cannot be a child mapped folder from the folder to move
                if (destinationFolder.MappedPath.StartsWith(folderToMove.MappedPath))
                {
                    return false;
                }
            }

            return IsMoveOperationValid(folderToMove, newFolderPath);
        }
        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
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
        internal virtual bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
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
                    if (String.IsNullOrEmpty(existingItem.MappedPath))
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
                    if (File.Exists(targetFile)) File.Delete(targetFile);
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
            RenameFolderInFileSystem(folder, newFolderPath);

            //Update provider
            var newMappedPath = destinationFolder.MappedPath + folder.FolderName + "/";
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var provider = FolderProvider.Instance(folderMapping.FolderProviderType);
            provider.MoveFolder(folder.MappedPath, newMappedPath, folderMapping);

            //Update database
            UpdateChildFolders(folder, Path.Combine(destinationFolder.FolderPath, folder.FolderName));
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void MoveFolderBetweenProviders(IFolderInfo folder, string newFolderPath)
        {
            RenameFolderInFileSystem(folder, newFolderPath);

            var folderInfos = GetFolders(folder.PortalID).Where(f => f.FolderPath != string.Empty && f.FolderPath.StartsWith(folder.FolderPath)).ToArray();
            var tmpFolderPath = folder.FolderPath;

            foreach (var folderInfo in folderInfos)
            {
                var folderPath = newFolderPath + folderInfo.FolderPath.Substring(tmpFolderPath.Length);
                
                var parentFolder = GetParentFolder(folder.PortalID, folderPath);
                folderInfo.ParentID = parentFolder.FolderID;
                folderInfo.FolderPath = folderPath;
                UpdateFolderInternal(folderInfo, true);
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void OverwriteFolder(IFolderInfo sourceFolder, IFolderInfo destinationFolder, Dictionary<int, FolderMappingInfo> folderMappings, SortedList<string, IFolderInfo> foldersToDelete)
        {
            var fileManager = FileManager.Instance;
            var files = GetFiles(sourceFolder, true);

            foreach (var file in files)
            {
                fileManager.MoveFile(file, destinationFolder);
            }

            // Delete source folder in database
            DeleteFolder(sourceFolder.PortalID, sourceFolder.FolderPath);

            var folderMapping = GetFolderMapping(folderMappings, sourceFolder.FolderMappingID);

            if (IsFolderMappingEditable(folderMapping))
            {
                foldersToDelete.Add(sourceFolder.FolderPath, sourceFolder);
            }
        }
        
        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void ProcessMergedTreeItemInAddMode(MergedTreeItem item, int portalId)
        {
            if (item.ExistsInFileSystem)
            {
                if (!item.ExistsInDatabase)
                {
	                var folderMappingId = FindFolderMappingId(item, portalId);
                    CreateFolderInDatabase(portalId, item.FolderPath, folderMappingId);
                }
            }
            else
            {
                if (item.ExistsInDatabase)
                {
                    if (item.ExistsInFolderMapping)
                    {
                        CreateFolderInFileSystem(PathUtils.Instance.GetPhysicalPath(portalId, item.FolderPath));
                    }
                }
                else // by exclusion it exists in the Folder Mapping
                {
                    CreateFolderInFileSystem(PathUtils.Instance.GetPhysicalPath(portalId, item.FolderPath));
                    CreateFolderInDatabase(portalId, item.FolderPath, item.FolderMappingID, item.MappedPath);
                }
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void ProcessMergedTreeItemInDeleteMode(MergedTreeItem item, int portalId)
        {
            if (item.ExistsInFileSystem)
            {
                if (item.ExistsInDatabase)
                {
                    if (item.FolderPath == "") return; // Do not process root folder

                    if (!item.ExistsInFolderMapping)
                    {
                        var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, item.FolderMappingID);

                        if (folderMapping.IsEditable)
                        {
                            DirectoryWrapper.Instance.Delete(PathUtils.Instance.GetPhysicalPath(portalId, item.FolderPath), false);
                            DeleteFolder(portalId, item.FolderPath);
                        }
                    }
                }
            }
            else
            {
                if (item.ExistsInDatabase && !item.ExistsInFolderMapping)
                {
                    DeleteFolder(portalId, item.FolderPath);
                }
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void RemoveOrphanedFiles(IFolderInfo folder)
        {
            var files = GetFiles(folder, false, true);

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
                            FileManager.Instance.DeleteFile(file);
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
            if (!di.Exists) return;

            var target = PathUtils.Instance.GetPhysicalPath(folder.PortalID, newFolderPath);
            MoveDirectory(source, target);
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
            var folder = GetFolder(portalId, item.FolderPath);

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

                RemoveOrphanedFiles(folder);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void UpdateParentFolder(int portalId, string folderPath)
        {
            if (!String.IsNullOrEmpty(folderPath))
            {
                var parentFolderPath = folderPath.Substring(0, folderPath.Substring(0, folderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
                var objFolder = GetFolder(portalId, parentFolderPath);
                if (objFolder != null)
                {
                    //UpdateFolder(objFolder);
                    UpdateFolderInternal(objFolder, false);
                }
            }
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual void UpdateChildFolders(IFolderInfo folder, string newFolderPath)
        {
            var originalFolderPath = folder.FolderPath;

            var folderInfos = GetFolders(folder.PortalID).Where(f => f.FolderPath != string.Empty && f.FolderPath.StartsWith(originalFolderPath)).ToArray();
            
            foreach (var folderInfo in folderInfos)
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folderInfo.FolderMappingID);
                var provider = FolderProvider.Instance(folderMapping.FolderProviderType);

                var folderPath = newFolderPath + (newFolderPath.EndsWith("/") ? "" : "/") + folderInfo.FolderPath.Substring(originalFolderPath.Length);

                var parentFolder = GetParentFolder(folder.PortalID, folderPath);
                folderInfo.ParentID = parentFolder.FolderID;
                folderInfo.FolderPath = folderPath;

                var parentProvider = FolderProvider.Instance(FolderMappingController.Instance.GetFolderMapping(parentFolder.PortalID, parentFolder.FolderMappingID).FolderProviderType);
                if (parentProvider.SupportsMappedPaths || !provider.SupportsMappedPaths)
                {
                    if (provider.SupportsMappedPaths)
                    {
                        var mappedPath = parentFolder.FolderPath == "" ? "" : folderPath.Replace(parentFolder.FolderPath, string.Empty);
                        folderInfo.MappedPath = PathUtils.Instance.FormatFolderPath(parentFolder.MappedPath + mappedPath);
                    }
                    else
                    {
                        folderInfo.MappedPath = folderPath;
                    }
                }

                UpdateFolderInternal(folderInfo, false);
            }
            ClearFolderCache(folder.PortalID);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        internal virtual bool CanMoveBetweenFolderMappings(FolderMappingInfo sourceFolderMapping, FolderMappingInfo destinationFolderMapping)
        {
            //If Folder Mappings are exactly the same
            if (sourceFolderMapping.FolderMappingID == destinationFolderMapping.FolderMappingID)
            {
                return true;
            }

            return IsStandardFolderProviderType(sourceFolderMapping) && IsStandardFolderProviderType(destinationFolderMapping);
        }
        #endregion

        #region Internal Classes

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
                return String.Compare(x.ToLowerInvariant(), y.ToLowerInvariant(), StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// This class and its members are reserved for internal use and are not intended to be used in your code.
        /// </summary>
        internal class MoveFoldersInfo
        {
            public string Source { get; private set; }
            public string Target { get; private set; }

            public MoveFoldersInfo(string source, string target)
            {
                Source = source;
                Target = target;
            }
        }

        #endregion

        #region Obsolete Methods

        /// <summary>
        /// Moves the specified folder and its contents to a new location.
        /// </summary>
        /// <param name="folder">The folder to move.</param>
        /// <param name="newFolderPath">The new folder path.</param>
        /// <returns>The moved folder.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.1.  It has been replaced by FolderManager.Instance.MoveFolder(IFolderInfo folder, IFolderInfo destinationFolder) ")]
        public virtual IFolderInfo MoveFolder(IFolderInfo folder, string newFolderPath)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("newFolderPath", newFolderPath);

            var parentFolderPath = newFolderPath.Substring(0, newFolderPath.Substring(0, newFolderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
            return MoveFolder(folder, GetFolder(folder.PortalID, parentFolderPath));
        }

        #endregion
    }
}
