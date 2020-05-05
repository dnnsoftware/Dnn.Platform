// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Components
{
    /// <summary>
    /// Manager for permission logic of Resource Manager
    /// </summary>
    public interface IPermissionsManager
    {
        /// <summary>
        /// Return if the current user is allowed to view the content of a folder
        /// </summary>
        /// <param name="folderId">Id of the folder</param>
        /// <param name="moduleMode">Current mode of the module instance</param>
        /// <returns>If the user is allowed or not</returns>
        bool HasFolderContentPermission(int folderId, int moduleMode);

        /// <summary>
        /// Return if the current user is allowed to read the files of a folder
        /// </summary>
        /// <param name="folderId">Id of the folder</param>
        /// <returns>If the user is allowed or not</returns>
        bool HasGetFileContentPermission(int folderId);

        /// <summary>
        /// Return if the current user is allowed to add files on a folder
        /// </summary>
        /// <param name="moduleMode">Current mode of the module instance</param>
        /// <param name="folderId">Id of the folder</param>
        /// <returns>If the user is allowed or not</returns>
        bool HasAddFilesPermission(int moduleMode, int folderId);

        /// <summary>
        /// Return if the current user is allowed to add folders on a folder
        /// </summary>
        /// <param name="moduleMode">Current mode of the module instance</param>
        /// <param name="folderId">Id of the folder</param>
        /// <returns>If the user is allowed or not</returns>
        bool HasAddFoldersPermission(int moduleMode, int folderId);

        /// <summary>
        /// Return if the current user is allowed to delete items from a folder
        /// </summary>
        /// <param name="moduleMode">Current mode of the module instace</param>
        /// <param name="folderId">Id of the folder</param>
        /// <returns>If the user is allowed or not</returns>
        bool HasDeletePermission(int moduleMode, int folderId);

        /// <summary>
        /// Return if the current user is allowed to manage the items of a folder
        /// </summary>
        /// <param name="moduleMode">Current mode of the module instance</param>
        /// <param name="folderId">Id of the folder</param>
        /// <returns>If the user is allowed or not</returns>
        bool HasManagePermission(int moduleMode, int folderId);
    }
}