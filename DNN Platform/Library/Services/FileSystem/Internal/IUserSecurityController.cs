// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    public interface IUserSecurityController
    {
        /// <summary>
        /// Checks if the Current user is Host user or Admin user of the provided portal.
        /// </summary>
        /// <param name="portalId">Portal Id to check Admin users.</param>
        /// <returns>True if the Current user is Host user or Admin user. False otherwise.</returns>
        bool IsHostAdminUser(int portalId);

        /// <summary>
        /// Checks if the provided user is Host user or Admin user of the provided portal.
        /// </summary>
        /// <param name="portalId">Portal Id to check Admin users.</param>
        /// <param name="userId">User Id to check.</param>
        /// <returns>True if the user is Host user or Admin user. False otherwise.</returns>
        bool IsHostAdminUser(int portalId, int userId);

        /// <summary>
        /// Checks if the provided permission is allowed for the current user in the provided folder.
        /// </summary>
        /// <param name="folder">Folder to check.</param>
        /// <param name="permissionKey">Permission key to check.</param>
        /// <returns></returns>
        bool HasFolderPermission(IFolderInfo folder, string permissionKey);
    }
}
