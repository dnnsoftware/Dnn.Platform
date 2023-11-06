// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Security.Permissions;

using System.Collections.Generic;

/// <summary>Handles the Business Control Layer for Permissions.</summary>
public interface IPermissionDefinitionService
{
    /// <summary>Gets the permissions.</summary>
    /// <returns>The permissions.</returns>
    IEnumerable<IPermissionDefinitionInfo> GetDefinitions();

    /// <summary>Gets the permissions by folder.</summary>
    /// <returns>The permissions by folder.</returns>
    IEnumerable<IPermissionDefinitionInfo> GetDefinitionsByFolder();

    /// <summary>Gets the permissions by desktop module.</summary>
    /// <returns>The permissions by desktop module.</returns>
    IEnumerable<IPermissionDefinitionInfo> GetDefinitionsByPortalDesktopModule();

    /// <summary>Gets the permissions by tab.</summary>
    /// <returns>The permissions by tab.</returns>
    IEnumerable<IPermissionDefinitionInfo> GetDefinitionsByTab();

    /// <summary>Gets the permissions by <see cref="IPermissionDefinitionInfo.PermissionCode"/> and <see cref="IPermissionDefinitionInfo.PermissionKey"/>.</summary>
    /// <param name="permissionCode">The permission code.</param>
    /// <param name="permissionKey">The permission key.</param>
    /// <returns>The permissions by tab.</returns>
    IEnumerable<IPermissionDefinitionInfo> GetDefinitionsByCodeAndKey(string permissionCode, string permissionKey);

    /// <summary>Gets the permissions by <see cref="IPermissionDefinitionInfo.ModuleDefId"/>.</summary>
    /// <param name="moduleDefId">The module definition ID.</param>
    /// <returns>The permissions by tab.</returns>
    IEnumerable<IPermissionDefinitionInfo> GetDefinitionsByModuleDefId(int moduleDefId);

    /// <summary>Gets the permissions by <see cref="IPermissionDefinitionInfo.ModuleDefId"/> and <see cref="IPermissionDefinitionInfo.PermissionCode"/> for the given module in the tab.</summary>
    /// <param name="moduleId">The module ID.</param>
    /// <param name="tabId">The tab ID.</param>
    /// <returns>The permissions by tab.</returns>
    IEnumerable<IPermissionDefinitionInfo> GetDefinitionsByModule(int moduleId, int tabId);

    /// <summary>Adds a new permission.</summary>
    /// <param name="permissionDefinition">The permission.</param>
    /// <returns>The new permission ID.</returns>
    int AddDefinition(IPermissionDefinitionInfo permissionDefinition);

    /// <summary>Deletes an existing permission.</summary>
    /// <param name="permissionDefinition">The permission to delete.</param>
    void DeleteDefinition(IPermissionDefinitionInfo permissionDefinition);

    /// <summary>Gets the permission by the <see cref="IPermissionDefinitionInfo.PermissionId"/>.</summary>
    /// <param name="permissionDefinitionId">The permission ID.</param>
    /// <returns>The permission.</returns>
    IPermissionDefinitionInfo GetDefinition(int permissionDefinitionId);

    /// <summary>Updates an existing permission.</summary>
    /// <param name="permission">The permission.</param>
    void UpdateDefinition(IPermissionDefinitionInfo permission);

    /// <summary>Clears the permission definition cache.</summary>
    /// <remarks>
    /// <see cref="AddDefinition"/>, <see cref="UpdateDefinition"/> and <see cref="DeleteDefinition"/> will clear the cache automatically.
    /// This method is only needed if you want to clear the cache manually.
    /// </remarks>
    void ClearCache();
}
