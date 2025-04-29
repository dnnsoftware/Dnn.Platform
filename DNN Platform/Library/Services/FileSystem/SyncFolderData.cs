// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem;

using DotNetNuke.Security.Permissions;

/// <summary>Represents the data about a folder to be synced.</summary>
internal class SyncFolderData
{
    /// <summary>Gets or sets the site (portal) ID.</summary>
    public int PortalId { get; set; }

    /// <summary>Gets or sets the folder path.</summary>
    public string FolderPath { get; set; }

    /// <summary>Gets or sets the folder permissions.</summary>
    public FolderPermissionCollection Permissions { get; set; }
}
