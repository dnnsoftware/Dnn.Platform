// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
    using DotNetNuke.Modules.DigitalAssets.Services.Models;
    using DotNetNuke.Services.FileSystem;

    public interface IDigitalAssetsController
    {
        /// <summary>
        /// Gets the list of the Folder Mappings. This list depends on the configuration of the module instanced.
        /// </summary>
        /// <param name="moduleId">The Id of the module.</param>
        /// <returns>The list of Folder Mappings.</returns>
        IEnumerable<FolderMappingViewModel> GetFolderMappings(int moduleId);

        /// <summary>
        /// Gets the list of subfolders for the specified folder.
        /// </summary>
        /// <param name="moduleId">The Id of the module.</param>
        /// <param name="parentFolderId">The folderItemId of the folder to get the list of subfolders.</param>
        /// <returns>The list of subfolders for the specified folder.</returns>
        IEnumerable<FolderViewModel> GetFolders(int moduleId, int parentFolderId);

        /// <summary>
        /// Gets a file entity by providing a file identifier.
        /// </summary>
        /// <param name="fileID">The identifier of the file.</param>
        /// <returns>The fileItem entity.</returns>
        ItemViewModel GetFile(int fileID);

        /// <summary>
        /// Gets a folder entity by providing a folder identifier.
        /// </summary>
        /// <param name="folderID">The identifier of the folder.</param>
        /// <returns>The folderItem entity or null if the folder cannot be located.</returns>
        FolderViewModel GetFolder(int folderID);

        /// <summary>
        /// Gets the root folder of the current Portal. This folder depends on the configuration of the module.
        /// </summary>
        /// <param name="moduleId">The Id of the module.</param>
        /// <returns>The root folderItem entity.</returns>
        FolderViewModel GetRootFolder(int moduleId);

        /// <summary>
        /// Gets the group folder.
        /// </summary>
        /// <param name="groupId">The identifier of the group.</param>
        /// <param name="portalSettings" >The current portal settings.</param>
        /// <returns>The group folderItem entity.</returns>
        FolderViewModel GetGroupFolder(int groupId, PortalSettings portalSettings);

        /// <summary>
        /// Gets the user folder.
        /// </summary>
        /// <param name="userInfo" >The current user.</param>
        /// <returns>The user folderItem entity.</returns>
        FolderViewModel GetUserFolder(UserInfo userInfo);

        /// <summary>
        /// Gets the files and folders contained in the specified folder.
        /// </summary>
        /// <param name="moduleId">The id of the Module.</param>
        /// <param name="folderId">Folder Identifier.</param>
        /// <param name="startIndex">Start index to retrieve items.</param>
        /// <param name="numItems">Max Number of items.</param>
        /// <param name="sortExpression">The sort expression in a SQL format, e.g. FileName ASC.</param>
        /// <returns>The list of files and folders contained in the specified folder paginated.</returns>
        PageViewModel GetFolderContent(int moduleId, int folderId, int startIndex, int numItems, string sortExpression);

        /// <summary>
        /// Searches the files and folders contained in the specified folder.
        /// </summary>
        /// <param name="moduleId">The id of the Module.</param>
        /// <param name="folderId">Folder Identifier.</param>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="startIndex">Start index to retrieve items.</param>
        /// <param name="numItems">Max Number of items.</param>
        /// <param name="sortExpression">The sort expression in a SQL format, e.g. FileName ASC.</param>
        /// <returns>The list of files and folders contained in the specified folder paginated.</returns>
        PageViewModel SearchFolderContent(int moduleId, int folderId, string pattern, int startIndex, int numItems, string sortExpression);

        /// <summary>
        /// Synchronize a folder within the File System.
        /// </summary>
        /// <param name="folderId">Reference to the folder is going to be synchronized.</param>
        /// <param name="recursive">Indicates if subfolders are going to be synchronized.</param>
        void SyncFolderContent(int folderId, bool recursive);

        /// <summary>
        /// Gets a newly created folder.
        /// </summary>
        /// <param name="folderName">folderName is the name of the new folder.</param>
        /// <param name="folderParentID">The reference to the parent folder where the new folder will be create.</param>
        /// <param name="folderMappingID">folderMappingID is the mapping related with the new folder.</param>
        /// <param name="mappedPath">mappedPath used for the mapping to folder in remove provider.</param>
        /// <returns>The newly folder created under the specified parent folder.</returns>
        FolderViewModel CreateFolder(string folderName, int folderParentID, int folderMappingID, string mappedPath);

        /// <summary>
        /// Renames a existing folder.
        /// </summary>
        /// <param name="folderID">Folder reference to rename.</param>
        /// <param name="newFolderName">The new name to set to the folder.</param>
        /// <returns>The final moved folder.</returns>
        FolderViewModel RenameFolder(int folderID, string newFolderName);

        /// <summary>
        /// Deletes a collection of items (folder and/or files).
        /// </summary>
        /// <param name="items">Items list.</param>
        /// <remarks>all the items belong at the same Folder.</remarks>
        /// <returns>The non deleted item list. The files / subfolders for which the user has no permissions to delete.</returns>
        IEnumerable<ItemPathViewModel> DeleteItems(IEnumerable<DeleteItem> items);

        /// <summary>
        /// Unlinks a specified folder.
        /// </summary>
        /// <param name="folderID">The folder ID to be unlinked.</param>
        void UnlinkFolder(int folderID);

        /// <summary>
        /// Get the number of subfolders which support Mapped Path.
        /// </summary>
        /// <param name="items">Items list.</param>
        /// <param name="portalID">Portal ID.</param>
        /// <returns></returns>
        int GetMappedSubFoldersCount(IEnumerable<ItemBaseViewModel> items, int portalID);

        /// <summary>
        /// Renames a existing file.
        /// </summary>
        /// <param name="fileID">File reference to rename.</param>
        /// <param name="newFileName">The new name to set to the file.</param>
        /// <returns>The final renamed file.</returns>
        ItemViewModel RenameFile(int fileID, string newFileName);

        /// <summary>
        /// Get the content of a file, ready to download.
        /// </summary>
        /// <param name="fileId">File reference to the source file.</param>
        /// <param name="fileName">Returns the name of the file.</param>
        /// <param name="contentType">Returns the content type of the file.</param>
        /// <returns>The file content.</returns>
        Stream GetFileContent(int fileId, out string fileName, out string contentType);

        /// <summary>
        /// Copies a file to the destination folder.
        /// </summary>
        /// <param name="fileId">File reference to the source file.</param>
        /// <param name="destinationFolderId">Folder reference to the destination folder.</param>
        /// <param name="overwrite">Overwrite destination if a file with the same name already exists.</param>
        /// <returns>The response object with the result of the action.</returns>
        CopyMoveItemViewModel CopyFile(int fileId, int destinationFolderId, bool overwrite);

        /// <summary>
        /// Moves a file to the destination folder.
        /// </summary>
        /// <param name="fileId">File reference to the source file.</param>
        /// <param name="destinationFolderId">Folder reference to the destination folder.</param>
        /// <param name="overwrite">Overwrite destination if a file with the same name already exists.</param>
        /// <returns>The response object with the result of the action.</returns>
        CopyMoveItemViewModel MoveFile(int fileId, int destinationFolderId, bool overwrite);

        /// <summary>
        /// Moves a Folder to the destination folder.
        /// </summary>
        /// <param name="folderId">Folder reference to the source file.</param>
        /// <param name="destinationFolderId">Folder reference to the destination folder.</param>
        /// <param name="overwrite">Overwrite destination if a file with the same name already exists.</param>
        /// <returns>The response object with the result of the action.</returns>
        CopyMoveItemViewModel MoveFolder(int folderId, int destinationFolderId, bool overwrite);

        /// <summary>
        /// Extracts the files and folders contained in the specified zip file to the specified folder.
        /// </summary>
        /// <param name="fileId">File reference to the source file.</param>
        /// <param name="overwrite">Overwrite destination if a file with the same name already exists.</param>
        /// <returns>The response object with the result of the action.</returns>
        ZipExtractViewModel UnzipFile(int fileId, bool overwrite);

        /// <summary>
        /// Returns all invalid chars for folder and file names.
        /// </summary>
        /// <returns>A string that includes all invalid chars.</returns>
        string GetInvalidChars();

        /// <summary>
        /// Returns the error text when a name contains an invalid character.
        /// </summary>
        /// <returns>The error text to show when a name contains an invalid character.</returns>
        string GetInvalidCharsErrorText();

        /// <summary>
        /// Get the URL of a file.
        /// </summary>
        /// <param name="fileId">File reference to the source file.</param>
        /// <returns>The URL of the file.</returns>
        string GetUrl(int fileId);

        /// <summary>
        /// Returns a fields set. These fields define the Folder preview info.
        /// </summary>
        /// <param name="folder">The folder model.</param>
        /// <returns>The Preview info object.</returns>
        PreviewInfoViewModel GetFolderPreviewInfo(IFolderInfo folder);

        /// <summary>
        /// Returns a fields set. These fields define the File preview info.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="item">The file model.</param>
        /// <returns>The Preview info object.</returns>
        PreviewInfoViewModel GetFilePreviewInfo(IFileInfo file, ItemViewModel item);

        /// <summary>
        /// Get the list of the default FolderMappings, i.e.: Standard/Database/Secure, associated to the module instance.
        /// This depends on the configuration of the module.
        /// </summary>
        /// <param name="moduleId">The id of the Module.</param>
        /// <returns>The list of default FolderMappingInfo associated.</returns>
        IEnumerable<FolderMappingInfo> GetDefaultFolderProviderValues(int moduleId);

        /// <summary>
        /// Get the default FolderTypeId to use when creating new folders under the root folder.
        /// This depends on the module configuration.
        /// </summary>
        /// <param name="moduleId">The Id of the module.</param>
        /// <returns>The default FolderTypeId.</returns>
        int? GetDefaultFolderTypeId(int moduleId);

        /// <summary>
        /// Gets the current Portal Id. This id depends on the configuration of the module.
        /// </summary>
        /// <param name="moduleId">The Id of the module.</param>
        /// <returns>The id of the current portal.</returns>
        int GetCurrentPortalId(int moduleId);

        /// <summary>
        /// Check if the current user has the specified permission over the specified folder.
        /// </summary>
        /// <param name="folder">The folder to check.</param>
        /// <param name="permissionKey">The permission to check.</param>
        /// <returns>Returns TRUE if the current user has the specified permission over the specified folder. FALSE otherwise.</returns>
        bool HasPermission(IFolderInfo folder, string permissionKey);

        /// <summary>
        /// Get the index of the inital tab to be shown when module is loaded.
        /// </summary>
        /// <param name="requestParams">Request parameters collection.</param>
        /// <param name="damState">Module State values collection.</param>
        /// <returns>The index to the tab to be shown.</returns>
        int GetInitialTab(NameValueCollection requestParams, NameValueCollection damState);
    }
}
