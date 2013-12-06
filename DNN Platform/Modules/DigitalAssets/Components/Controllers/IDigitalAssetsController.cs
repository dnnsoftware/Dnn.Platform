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

using System.Collections.Generic;
using System.IO;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers
{
    public interface IDigitalAssetsController
    {
        /// <summary>
        /// Gets the list of the Folder Mappings
        /// </summary>
        /// <returns>The list of Folder Mappings</returns>
        IEnumerable<FolderMappingViewModel> GetFolderMappings();

        /// <summary>
        /// Gets the list of subfolders for the specified folder.
        /// </summary>
        /// <param name="parentFolderId">The folderItemId of the folder to get the list of subfolders.</param>
        /// <returns>The list of subfolders for the specified folder.</returns>
        IEnumerable<FolderViewModel> GetFolders(int parentFolderId);

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
        /// Gets the root folder of the current Portal.
        /// </summary>
        /// <returns>The root folderItem entity.</returns>
        FolderViewModel GetRootFolder();

        /// <summary>
        /// Gets the group folder
        /// </summary>
        /// <param name="groupId">The identifier of the group.</param>
        /// <param name="portalSettings" >The current portal settings.</param>
        /// <returns>The root folderItem entity.</returns>
        FolderViewModel GetGroupFolder(int groupId, PortalSettings portalSettings);

        /// <summary>
        /// Gets the files and folders contained in the specified folder.
        /// </summary>
        /// <param name="folderId">Folder Identifier</param>
        /// <param name="startIndex">Start index to retrieve items</param>
        /// <param name="numItems">Max Number of items</param>
        /// <param name="sortExpression">The sort expression in a SQL format, e.g. FileName ASC</param>
        /// <returns>The list of files and folders contained in the specified folder paginated</returns>
        PageViewModel GetFolderContent(int folderId, int startIndex, int numItems, string sortExpression);

        /// <summary>
        /// Searches the files and folders contained in the specified folder.
        /// </summary>
        /// <param name="folderId">Folder Identifier</param>
        /// <param name="pattern">The pattern to search for</param>
        /// <param name="startIndex">Start index to retrieve items</param>
        /// <param name="numItems">Max Number of items</param>
        /// <param name="sortExpression">The sort expression in a SQL format, e.g. FileName ASC</param>
        /// <returns>The list of files and folders contained in the specified folder paginated</returns>
        PageViewModel SearchFolderContent(int folderId, string pattern, int startIndex, int numItems, string sortExpression);

        /// <summary>
        /// Synchronize a folder within the File System
        /// </summary>
        /// <param name="folderId">Reference to the folder is going to be synchronized</param>        
        /// <param name="recursive">Indicates if subfolders are going to be synchronized</param>
        void SyncFolderContent(int folderId, bool recursive);

        /// <summary>
        /// Gets a newly created folder.
        /// </summary>
        /// <param name="folderName">folderName is the name of the new folder</param>
        /// <param name="folderParentID">The reference to the parent folder where the new folder will be create</param>
        /// <param name="folderMappingID">folderMappingID is the mapping related with the new folder</param>
        /// <param name="mappedPath">mappedPath used for the mapping to folder in remove provider</param>
        /// <returns>The newly folder created under the specified parent folder</returns>
        FolderViewModel CreateFolder(string folderName, int folderParentID, int folderMappingID, string mappedPath);

        /// <summary>
        /// Renames a existing folder.
        /// </summary>
        /// <param name="folderID">Folder reference to rename</param>
        /// <param name="newFolderName">The new name to set to the folder</param>
        /// <returns>The final moved folder</returns>
        FolderViewModel RenameFolder(int folderID, string newFolderName);

        /// <summary>
        /// Deletes a collection of items (folder and/or files)
        /// </summary>
        /// <param name="items">Items list</param>
        /// <remarks>all the items belong at the same Folder</remarks>
        /// <returns>The non deleted item list. The files / subfolders for which the user has no permissions to delete</returns>
        IEnumerable<ItemPathViewModel> DeleteItems(IEnumerable<ItemBaseViewModel> items);

        /// <summary>
        /// Renames a existing file.
        /// </summary>
        /// <param name="fileID">File reference to rename</param>
        /// <param name="newFileName">The new name to set to the file</param>
        /// <returns>The final renamed file</returns>
        ItemViewModel RenameFile(int fileID, string newFileName);

        /// <summary>
        /// Get the content of a file, ready to download
        /// </summary>
        /// <param name="fileId">File reference to the source file</param>
        /// <param name="fileName">Returns the name of the file</param>
        /// <param name="contentType">Returns the content type of the file</param>
        /// <returns>The file content</returns>
        Stream GetFileContent(int fileId, out string fileName, out string contentType);

        /// <summary>
        /// Copies a file to the destination folder
        /// </summary>
        /// <param name="fileId">File reference to the source file</param>
        /// <param name="destinationFolderId">Folder reference to the destination folder</param>
        /// <param name="overwrite">Overwrite destination if a file with the same name already exists</param>
        /// <returns>The response object with the result of the action</returns>
        CopyMoveItemViewModel CopyFile(int fileId, int destinationFolderId, bool overwrite);

        /// <summary>
        /// Moves a file to the destination folder
        /// </summary>
        /// <param name="fileId">File reference to the source file</param>
        /// <param name="destinationFolderId">Folder reference to the destination folder</param>
        /// <param name="overwrite">Overwrite destination if a file with the same name already exists</param>
        /// <returns>The response object with the result of the action</returns>
        CopyMoveItemViewModel MoveFile(int fileId, int destinationFolderId, bool overwrite);

        /// <summary>
        /// Moves a Folder to the destination folder
        /// </summary>
        /// <param name="folderId">Folder reference to the source file</param>
        /// <param name="destinationFolderId">Folder reference to the destination folder</param>
        /// <param name="overwrite">Overwrite destination if a file with the same name already exists</param>
        /// <returns>The response object with the result of the action</returns>
        CopyMoveItemViewModel MoveFolder(int folderId, int destinationFolderId, bool overwrite);

        /// <summary>
        /// Extracts the files and folders contained in the specified zip file to the specified folder.
        /// </summary>
        /// <param name="fileId">File reference to the source file</param>
        /// <param name="overwrite">Overwrite destination if a file with the same name already exists</param>
        /// <returns>The response object with the result of the action</returns>
        ZipExtractViewModel UnzipFile(int fileId, bool overwrite);        
        
        /// <summary>
        /// Returns all invalid chars for folder and file names
        /// </summary>
        /// <returns>A string that includes all invalid chars</returns>
        string GetInvalidChars();

        /// <summary>
        /// Returns the error text when a name contains an invalid character
        /// </summary>
        /// <returns>The error text to show when a name contains an invalid character</returns>
        string GetInvalidCharsErrorText();
        
        /// <summary>
        /// Get the URL of a file
        /// </summary>
        /// <param name="fileId">File reference to the source file</param>
        /// <returns>The URL of the file</returns>
        string GetUrl(int fileId);

        /// <summary>
        /// Returns a fields set. These fields define the Folder preview info
        /// </summary>
        /// <param name="folder">The folder model</param>
        /// <returns>The Preview info object</returns>
        PreviewInfoViewModel GetFolderPreviewInfo(IFolderInfo folder);

        /// <summary>
        /// Returns a fields set. These fields define the File preview info
        /// </summary>
        /// <param name="file">The file</param>
        /// <param name="item">The file model</param>
        /// <returns>The Preview info object</returns>
        PreviewInfoViewModel GetFilePreviewInfo(IFileInfo file, ItemViewModel item);

        /// <summary>
        /// Gets the current Portal Id
        /// </summary>
        int CurrentPortalId { get; }
    }
}
