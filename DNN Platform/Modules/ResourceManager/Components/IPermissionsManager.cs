// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components;

/// <summary>Manager for permission logic of Resource Manager.</summary>
public interface IPermissionsManager
{
    /// <summary>Checks if the current user is allowed to view the content of a folder.</summary>
    /// <param name="folderId">Id of the folder.</param>
    /// <param name="moduleMode">Current mode of the module instance.</param>
    /// <returns>A value indicating whether the user is allowed to view the content of the folder.</returns>
    bool HasFolderContentPermission(int folderId, int moduleMode);

    /// <summary>Checks if the current user is allowed to read the files in a folder.</summary>
    /// <param name="folderId">Id of the folder.</param>
    /// <returns>A value indicating whether the user is allowed to view the content of the folder.</returns>
    bool HasGetFileContentPermission(int folderId);

    /// <summary>Checks if the current user is allowed to add files to a folder.</summary>
    /// <param name="moduleMode">Current mode of the module instance.</param>
    /// <param name="folderId">Id of the folder.</param>
    /// <returns>A value indicating whether the user is allowed to add files to the folder.</returns>
    bool HasAddFilesPermission(int moduleMode, int folderId);

    /// <summary>Checks if the current user is allowed to add folders to a folder.</summary>
    /// <param name="moduleMode">Current mode of the module instance.</param>
    /// <param name="folderId">Id of the folder.</param>
    /// <returns>A value indicating whether the user is allowed to add folders to the folder.</returns>
    bool HasAddFoldersPermission(int moduleMode, int folderId);

    /// <summary>Cehcks if the current user is allowed to delete items from a folder.</summary>
    /// <param name="moduleMode">Current mode of the module instace.</param>
    /// <param name="folderId">Id of the folder.</param>
    /// <returns>A value indicating whether the user is allowed to delete items from the folder.</returns>
    bool HasDeletePermission(int moduleMode, int folderId);

    /// <summary>Checks if the current user is allowed to manage the items in a folder.</summary>
    /// <param name="moduleMode">Current mode of the module instance.</param>
    /// <param name="folderId">Id of the folder.</param>
    /// <returns>A value indicating whether the user is allowed to manage the items in the folder.</returns>
    bool HasManagePermission(int moduleMode, int folderId);
}
