#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Services.Assets
{
    public interface IAssetManager
    {
        /// <summary>
        /// Gets the page of files and folders contained in the specified folder.
        /// </summary>
        /// <param name="folderId">Folder Identifier</param>
        /// <param name="startIndex">Start index to retrieve items</param>
        /// <param name="numItems">Max Number of items</param>
        /// <param name="sortExpression">The sort expression in a SQL format, e.g. FileName ASC</param>
        /// <param name="subfolderFilter"></param>
        /// <returns>The list of files and folders contained in the specified folder paginated</returns>
        ContentPage GetFolderContent(int folderId, int startIndex, int numItems, string sortExpression = null, SubfolderFilter subfolderFilter = SubfolderFilter.IncludeSubfoldersFolderStructure);

        /// <summary>
        /// Searches the files and folders contained in the specified folder.
        /// </summary>
        /// <param name="folderId">Folder Identifier</param>
        /// <param name="pattern">The pattern to search for</param>
        /// <param name="startIndex">Start index to retrieve items</param>
        /// <param name="numItems">Max Number of items</param>
        /// <param name="sortExpression">The sort expression in a SQL format, e.g. FileName ASC</param>
        /// <returns>The list of files and folders contained in the specified folder paginated</returns>
        ContentPage SearchFolderContent(int folderId, string pattern, int startIndex, int numItems, string sortExpression = null, SubfolderFilter subfolderFilter = SubfolderFilter.IncludeSubfoldersFolderStructure);

        /// <summary>
        /// Gets the list of subfolders for the specified folder.
        /// </summary>        
        /// <param name="parentFolder">The folder from where to get the list of subfolders.</param>
        /// <param name="orderingField">The field to order the list</param>
        /// <param name="asc">True to order ascending, false to order descending</param>
        /// <returns>The list of subfolders for the specified folder.</returns>
        IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder, string orderingField, bool asc);

        /// <summary>
        /// Renames a existing file.
        /// </summary>
        /// <param name="fileId">File reference to rename</param>
        /// <param name="newFileName">The new name to set to the file</param>
        /// <returns>The final renamed file</returns>
        IFileInfo RenameFile(int fileId, string newFileName);

        /// <summary>
        /// Renames a existing folder.
        /// </summary>
        /// <param name="folderId">Folder reference to rename</param>
        /// <param name="newFolderName">The new name to set to the folder</param>
        /// <returns>The final renamed folder</returns>
        IFolderInfo RenameFolder(int folderId, string newFolderName);

        /// <summary>
        /// Creates a new folder.
        /// </summary>
        /// <param name="folderName">folderName is the name of the new folder</param>
        /// <param name="folderParentId">The reference to the parent folder where the new folder will be create</param>
        /// <param name="folderMappingId">folderMappingID is the mapping related with the new folder</param>
        /// <param name="mappedPath">mappedPath used for the mapping to folder in remove provider</param>
        /// <returns>The newly folder created under the specified parent folder</returns> 
        IFolderInfo CreateFolder(string folderName, int folderParentId, int folderMappingId, string mappedPath);

        /// <summary>
        /// Deletes an existing folder 
        /// </summary>
        /// <param name="folderId">The ide of the folder to delete</param>
        /// <param name="onlyUnlink">In case of a remote folder, specifies that the folder should be unlinked, not deleted</param>
        /// <param name="nonDeletedSubfolders">The list of subfolders that could not be deleted, for example due to permissions</param>
        /// <returns>True if the folder has been correctly deleted, false otherwise</returns>  
        bool DeleteFolder(int folderId, bool onlyUnlink, ICollection<IFolderInfo> nonDeletedSubfolders);

        /// <summary>
        /// Deletes an existing file
        /// </summary>
        /// <param name="fileId">The ide of the folder to delete</param>
        /// <returns>True if the file has been correctly deleted, false otherwise</returns>  
        bool DeleteFile(int fileId);
    }
}