// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components;

using System.IO;

using Dnn.Modules.ResourceManager.Services.Dto;

using DotNetNuke.Services.Assets;
using DotNetNuke.Services.FileSystem;

/// <summary>Manager for the items of Resource Manager.</summary>
public interface IItemsManager
{
    /// <summary>Get the items contained on the folder.</summary>
    /// <param name="folderId">Container folder id.</param>
    /// <param name="startIndex">Index of the first item to be returned.</param>
    /// <param name="numItems">Max number of items to return.</param>
    /// <param name="sorting">Sorting option.</param>
    /// <param name="moduleMode">Current mode of module instance.</param>
    /// <returns>A <see cref="ContentPage"/> with the items in the folder.</returns>
    ContentPage GetFolderContent(int folderId, int startIndex, int numItems, string sorting, int moduleMode);

    /// <summary>Get the content of a file.</summary>
    /// <param name="fileId">Id of the file.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contentType">Type of the file.</param>
    /// <returns>Stream of the file content.</returns>
    Stream GetFileContent(int fileId, out string fileName, out string contentType);

    /// <summary>Create a new folder on Parent Folder.</summary>
    /// <param name="folderName">Name of the new folder.</param>
    /// <param name="parentFolderId">Id of the parent folder.</param>
    /// <param name="folderMappingId">Id of folder mapping.</param>
    /// <param name="mappedName">Mapped name.</param>
    /// <param name="moduleMode">Current mode of the module instance.</param>
    /// <returns>Folder info object of the new created folder.</returns>
    IFolderInfo CreateNewFolder(string folderName, int parentFolderId, int folderMappingId, string mappedName, int moduleMode);

    /// <summary>Update the asset details of a file.</summary>
    /// <param name="file">File.</param>
    /// <param name="fileDetails">New asset details of the file.</param>
    void SaveFileDetails(IFileInfo file, FileDetailsRequest fileDetails);

    /// <summary>Update the asset details of a folder.</summary>
    /// <param name="folder">Folder.</param>
    /// <param name="folderDetails">New asset details of the folder.</param>
    void SaveFolderDetails(IFolderInfo folder, FolderDetailsRequest folderDetails);

    /// <summary>Deletes a file.</summary>
    /// <param name="fileId">id of the file to delete.</param>
    /// <param name="moduleMode">Current mode of the module instance.</param>
    /// <param name="groupId">Id of the group (for group mode).</param>
    void DeleteFile(int fileId, int moduleMode, int groupId);

    /// <summary>Deletes a folder.</summary>
    /// <param name="folderId">Folder to delete Id.</param>
    /// <param name="unlinkAllowedStatus">Unlink allowed status.</param>
    /// <param name="moduleMode">Current mode of the module instance.</param>
    void DeleteFolder(int folderId, bool unlinkAllowedStatus, int moduleMode);

    /// <summary>Moves a file into a folder.</summary>
    /// <param name="sourceFileId">The id of the file to move.</param>
    /// <param name="destinationFolderId">The id of the folder to move the file into.</param>
    /// <param name="moduleMode">Current mode of the module instance.</param>
    /// <param name="groupId">Id of the group (gor group mode).</param>
    void MoveFile(int sourceFileId, int destinationFolderId, int moduleMode, int groupId);

    /// <summary>Moves a folder into another folder.</summary>
    /// <param name="sourceFolderId">The id of the folder to move.</param>
    /// <param name="destinationFolderId">The id of the folder into which to move the source folder.</param>
    /// <param name="moduleMode">Current mode of the module instance.</param>
    /// <param name="groupId">Id of the group (for group mode).</param>
    void MoveFolder(int sourceFolderId, int destinationFolderId, int moduleMode, int groupId);
}
