// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components;

using System;

using DotNetNuke.Common;
using DotNetNuke.Framework;
using DotNetNuke.Services.FileSystem;

/// <summary>Manages groups.</summary>
public class GroupManager : ServiceLocator<IGroupManager, GroupManager>, IGroupManager
{
    /// <inheritdoc/>
    public IFolderInfo FindOrCreateGroupFolder(int portalId, int groupId)
    {
        Requires.NotNegative("portalId", portalId);
        Requires.NotNegative("groupId", groupId);

        return this.GetGroupFolder(portalId, groupId) ?? FolderManager.Instance.AddFolder(portalId, "Groups/" + groupId);
    }

    /// <inheritdoc/>
    public IFolderInfo GetGroupFolder(int portalId, int groupId)
    {
        Requires.NotNegative("portalId", portalId);
        Requires.NotNegative("groupId", groupId);

        return FolderManager.Instance.GetFolder(portalId, "Groups/" + groupId);
    }

    /// <inheritdoc/>
    protected override Func<IGroupManager> GetFactory()
    {
        return () => new GroupManager();
    }
}
