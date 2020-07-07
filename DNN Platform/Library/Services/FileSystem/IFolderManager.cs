// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;

    /// <summary>
    /// Do not implement.  This interface is meant for reference and unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IFolderManager
    {
        /// <summary>
        /// Gets the alias name of the personal User Folder.
        /// </summary>
        string MyFolderName { get; }

        /// <summary>
        /// Adds read permissions for all users to the specified folder.
        /// </summary>
        /// <param name="folder">The folder to add the permission to.</param>
        /// <param name="permission">Used as base class for FolderPermissionInfo when there is no read permission already defined.</param>
        void AddAllUserReadPermission(IFolderInfo folder, PermissionInfo permission);

        /// <summary>
        /// Creates a new folder using the provided folder path and mapping.
        /// </summary>
        /// <param name="folderMapping">The folder mapping to use.</param>
        /// <param name="folderPath">The path of the new folder.</param>
        /// <returns>The added folder.</returns>
        IFolderInfo AddFolder(FolderMappingInfo folderMapping, string folderPath);

        /// <summary>
        /// Creates a new folder using the provided folder path and mapping.
        /// </summary>
        /// <param name="folderMapping">The folder mapping to use.</param>
        /// <param name="folderPath">The path of the new folder.</param>
        /// <param name="mappedPath">The path of the new folder in the provider.</param>
        /// <returns>The added folder.</returns>
        IFolderInfo AddFolder(FolderMappingInfo folderMapping, string folderPath, string mappedPath);

        /// <summary>
        /// Creates a new folder in the given portal using the provided folder path.
        /// The same mapping than the parent folder will be used to create this folder. So this method have to be used only to create subfolders.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="folderPath">The path of the new folder.</param>
        /// <returns>The added folder.</returns>
        IFolderInfo AddFolder(int portalId, string folderPath);

        /// <summary>
        /// Sets folder permissions to the given folder by copying parent folder permissions.
        /// </summary>
        /// <param name="folder">The folder to copy permissions to.</param>
        void CopyParentFolderPermissions(IFolderInfo folder);

        /// <summary>
        /// Deletes the specified folder.
        /// </summary>
        /// <param name="folder">The folder to delete.</param>
        void DeleteFolder(IFolderInfo folder);

        /// <summary>
        /// Deletes the specified folder.
        /// </summary>
        /// <param name="folderId">The folder identifier.</param>
        void DeleteFolder(int folderId);

        /// <summary>
        /// Unlink the specified folder.
        /// </summary>
        /// <param name="folder">The folder to unlink.</param>
        void UnlinkFolder(IFolderInfo folder);

        /// <summary>
        /// Delete the specified folder and all its content.
        /// </summary>
        /// <param name="folder"> The folder to delete>.</param>
        /// <param name="notDeletedSubfolders">A collection with all not deleted subfolders.</param>
        void DeleteFolder(IFolderInfo folder, ICollection<IFolderInfo> notDeletedSubfolders);

        /// <summary>
        /// Checks the existence of the specified folder in the specified portal.
        /// </summary>
        /// <param name="portalId">The portal where to check the existence of the folder.</param>
        /// <param name="folderPath">The path of folder to check the existence of.</param>
        /// <returns>A boolean value indicating whether the folder exists or not in the specified portal.</returns>
        bool FolderExists(int portalId, string folderPath);

        /// <summary>
        /// Gets the files contained in the specified folder.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        IEnumerable<IFileInfo> GetFiles(IFolderInfo folder);

        /// <summary>
        /// Gets the files contained in the specified folder.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="recursive">Whether or not to include all the subfolders.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, bool recursive);

        /// <summary>
        /// Gets the files contained in the specified folder.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="recursive">Whether or not to include all the subfolders.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, bool recursive, bool retrieveUnpublishedFiles);

        /// <summary>
        /// Search the files contained in the specified folder, for a matching pattern.
        /// </summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="pattern">The patter to search for.</param>
        /// <param name="recursive">Whether or not to include all the subfolders.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        IEnumerable<IFileInfo> SearchFiles(IFolderInfo folder, string pattern, bool recursive = false);

        /// <summary>
        /// Gets the list of Standard folders the specified user has the provided permissions.
        /// </summary>
        /// <param name="user">The user info.</param>
        /// <param name="permissions">The permissions the folders have to met.</param>
        /// <returns>The list of Standard folders the specified user has the provided permissions.</returns>
        /// <remarks>This method is used to support legacy behaviours and situations where we know the file/folder is in the file system.</remarks>
        IEnumerable<IFolderInfo> GetFileSystemFolders(UserInfo user, string permissions);

        /// <summary>
        /// Gets a folder entity by providing a portal identifier and folder identifier.
        /// </summary>
        /// <param name="folderId">The identifier of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        IFolderInfo GetFolder(int folderId);

        /// <summary>
        /// Gets a folder entity by providing a portal identifier and folder path.
        /// </summary>
        /// <param name="portalId">The portal where the folder exists.</param>
        /// <param name="folderPath">The path of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        IFolderInfo GetFolder(int portalId, string folderPath);

        /// <summary>
        /// Gets a folder entity by providing its unique id.
        /// </summary>
        /// <param name="uniqueId">The unique id of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        IFolderInfo GetFolder(Guid uniqueId);

        /// <summary>
        /// Get the users folder.
        /// </summary>
        /// <param name="userInfo">the user.</param>
        /// <returns>FolderInfo for the users folder.</returns>
        IFolderInfo GetUserFolder(UserInfo userInfo);

        /// <summary>
        /// Gets the list of subfolders for the specified folder.
        /// </summary>
        /// <param name="parentFolder">The folder to get the list of subfolders.</param>
        /// <returns>The list of subfolders for the specified folder.</returns>
        IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder);

        /// <summary>
        /// Gets the sorted list of folders of the provided portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The sorted list of folders of the provided portal.</returns>
        IEnumerable<IFolderInfo> GetFolders(int portalId);

        /// <summary>
        /// Gets the sorted list of folders of the provided portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="useCache">True = Read from Cache, False = Read from DB. </param>
        /// <returns>The sorted list of folders of the provided portal.</returns>
        IEnumerable<IFolderInfo> GetFolders(int portalId, bool useCache);

        /// <summary>
        /// Gets the sorted list of folders that match the provided permissions in the specified portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="permissions">The permissions to match.</param>
        /// <param name="userId">The user identifier to be used to check permissions.</param>
        /// <returns>The list of folders that match the provided permissions in the specified portal.</returns>
        IEnumerable<IFolderInfo> GetFolders(int portalId, string permissions, int userId);

        /// <summary>
        /// Gets the list of folders the specified user has read permissions.
        /// </summary>
        /// <param name="user">The user info.</param>
        /// <returns>The list of folders the specified user has read permissions.</returns>
        IEnumerable<IFolderInfo> GetFolders(UserInfo user);

        /// <summary>
        /// Gets the list of folders the specified user has the provided permissions.
        /// </summary>
        /// <param name="user">The user info.</param>
        /// <param name="permissions">The permissions the folders have to met.</param>
        /// <returns>The list of folders the specified user has the provided permissions.</returns>
        IEnumerable<IFolderInfo> GetFolders(UserInfo user, string permissions);

        /// <summary>
        /// Moves the specified folder and its contents to a new location.
        /// </summary>
        /// <param name="folder">The folder to move.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <returns>The moved folder.</returns>
        IFolderInfo MoveFolder(IFolderInfo folder, IFolderInfo destinationFolder);

        /// <summary>
        /// Renames the specified folder by setting the new provided folder name.
        /// </summary>
        /// <param name="folder">The folder to rename.</param>
        /// <param name="newFolderName">The new name to apply to the folder.</param>
        void RenameFolder(IFolderInfo folder, string newFolderName);

        /// <summary>
        /// Sets specific folder permissions for the given role to the given folder.
        /// </summary>
        /// <param name="folder">The folder to set permission to.</param>
        /// <param name="permissionId">The id of the permission to assign.</param>
        /// <param name="roleId">The role to assign the permission to.</param>
        void SetFolderPermission(IFolderInfo folder, int permissionId, int roleId);

        /// <summary>
        /// Sets specific folder permissions for the given role/user to the given folder.
        /// </summary>
        /// <param name="folder">The folder to set permission to.</param>
        /// <param name="permissionId">The id of the permission to assign.</param>
        /// <param name="roleId">The role to assign the permission to.</param>
        /// <param name="userId">The user to assign the permission to.</param>
        void SetFolderPermission(IFolderInfo folder, int permissionId, int roleId, int userId);

        /// <summary>
        /// Sets folder permissions for administrator role to the given folder.
        /// </summary>
        /// <param name="folder">The folder to set permission to.</param>
        /// <param name="administratorRoleId">The administrator role id to assign the permission to.</param>
        void SetFolderPermissions(IFolderInfo folder, int administratorRoleId);

        /// <summary>
        /// Synchronizes the entire folder tree for the specified portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The number of folder collisions.</returns>
        int Synchronize(int portalId);

        /// <summary>
        /// Synchronizes the specified folder, its files and its subfolders.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="relativePath">The relative path of the folder.</param>
        /// <returns>The number of folder collisions.</returns>
        int Synchronize(int portalId, string relativePath);

        /// <summary>
        /// Synchronizes the specified folder, its files and, optionally, its subfolders.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="relativePath">The relative path of the folder.</param>
        /// <param name="isRecursive">Indicates if the synchronization has to be recursive.</param>
        /// <param name="syncFiles">Indicates if files need to be synchronized.</param>
        /// <returns>The number of folder collisions.</returns>
        int Synchronize(int portalId, string relativePath, bool isRecursive, bool syncFiles);

        /// <summary>
        /// Updates metadata of the specified folder.
        /// </summary>
        /// <param name="folder">The folder to update.</param>
        /// <returns>The updated folder.</returns>
        IFolderInfo UpdateFolder(IFolderInfo folder);

        /// <summary>
        /// Moves the specified folder and its contents to a new location.
        /// </summary>
        /// <param name="folder">The folder to move.</param>
        /// <param name="newFolderPath">The new folder path.</param>
        /// <returns>The moved folder.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.1.  It has been replaced by FolderManager.Instance.MoveFolder(IFolderInfo folder, IFolderInfo destinationFolder) . Scheduled removal in v10.0.0.")]
        IFolderInfo MoveFolder(IFolderInfo folder, string newFolderPath);
    }
}
