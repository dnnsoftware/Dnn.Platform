// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Services.FileSystem;

namespace Dnn.Modules.ResourceManager.Components
{
    /// <summary>
    /// Manager class for Groups logic on Resource Manager
    /// </summary>
    public interface IGroupManager
    {
        /// <summary>
        /// Find or create the folder of the group
        /// </summary>
        /// <param name="portalId">Id of the portal</param>
        /// <param name="groupId">Id of the group</param>
        /// <returns>The Group folder. If the group has not a folder currently, then a new one will be created and returned</returns>
        IFolderInfo FindOrCreateGroupFolder(int portalId, int groupId);

        /// <summary>
        /// Get the folder of the group
        /// </summary>
        /// <param name="portalId">Id of the portal</param>
        /// <param name="groupId">Id of the group</param>
        /// <returns>The group folder, if it exist, or null if doesn't</returns>
        IFolderInfo GetGroupFolder(int portalId, int groupId);
    }
}