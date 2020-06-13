// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Assets
{
    using System.Collections.Generic;

    using DotNetNuke.Services.FileSystem;

    public interface IAssetManager
    {
        /// <summary>
        /// Gets the page of files and folders contained in the specified folder.
        /// </summary>
        /// <param name="folderId">Folder Identifier.</param>
        /// <param name="startIndex">Start index to retrieve items.</param>
        /// <param name="numItems">Max Number of items.</param>
        /// <param name="sortExpression">The sort expression in a SQL format, e.g. FileName ASC.</param>
        /// <param name="subfolderFilter"></param>
        /// <returns>The list of files and folders contained in the specified folder paginated.</returns>
        ContentPage GetFolderContent(int folderId, int startIndex, int numItems, string sortExpression = null, SubfolderFilter subfolderFilter = SubfolderFilter.IncludeSubfoldersFolderStructure);

        /// <summary>
        /// Searches the files and folders contained in the specified folder.
        /// </summary>
        /// <param name="folderId">Folder Identifier.</param>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="startIndex">Start index to retrieve items.</param>
        /// <param name="numItems">Max Number of items.</param>
        /// <param name="sortExpression">The sort expression in a SQL format, e.g. FileName ASC.</param>
        /// <returns>The list of files and folders contained in the specified folder paginated.</returns>
        ContentPage SearchFolderContent(int folderId, string pattern, int startIndex, int numItems, string sortExpression = null, SubfolderFilter subfolderFilter = SubfolderFilter.IncludeSubfoldersFolderStructure);

        /// <summary>
        /// Gets the list of subfolders for the specified folder.
        /// </summary>
        /// <param name="parentFolder">The folder from where to get the list of subfolders.</param>
        /// <param name="orderingField">The field to order the list.</param>
        /// <param name="asc">True to order ascending, false to order descending.</param>
        /// <returns>The list of subfolders for the specified folder.</returns>
        IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder, string orderingField, bool asc);

        /// <summary>
        /// Renames a existing file.
        /// </summary>
        /// <param name="fileId">File reference to rename.</param>
        /// <param name="newFileName">The new name to set to the file.</param>
        /// <returns>The final renamed file.</returns>
        IFileInfo RenameFile(int fileId, string newFileName);

        /// <summary>
        /// Renames a existing folder.
        /// </summary>
        /// <param name="folderId">Folder reference to rename.</param>
        /// <param name="newFolderName">The new name to set to the folder.</param>
        /// <returns>The final renamed folder.</returns>
        IFolderInfo RenameFolder(int folderId, string newFolderName);

        /// <summary>
        /// Creates a new folder.
        /// </summary>
        /// <param name="folderName">folderName is the name of the new folder.</param>
        /// <param name="folderParentId">The reference to the parent folder where the new folder will be create.</param>
        /// <param name="folderMappingId">folderMappingID is the mapping related with the new folder.</param>
        /// <param name="mappedPath">mappedPath used for the mapping to folder in remove provider.</param>
        /// <returns>The newly folder created under the specified parent folder.</returns>
        IFolderInfo CreateFolder(string folderName, int folderParentId, int folderMappingId, string mappedPath);

        /// <summary>
        /// Deletes an existing folder.
        /// </summary>
        /// <param name="folderId">The ide of the folder to delete.</param>
        /// <param name="onlyUnlink">In case of a remote folder, specifies that the folder should be unlinked, not deleted.</param>
        /// <param name="nonDeletedSubfolders">The list of subfolders that could not be deleted, for example due to permissions.</param>
        /// <returns>True if the folder has been correctly deleted, false otherwise.</returns>
        bool DeleteFolder(int folderId, bool onlyUnlink, ICollection<IFolderInfo> nonDeletedSubfolders);

        /// <summary>
        /// Deletes an existing file.
        /// </summary>
        /// <param name="fileId">The ide of the folder to delete.</param>
        /// <returns>True if the file has been correctly deleted, false otherwise.</returns>
        bool DeleteFile(int fileId);
    }
}
