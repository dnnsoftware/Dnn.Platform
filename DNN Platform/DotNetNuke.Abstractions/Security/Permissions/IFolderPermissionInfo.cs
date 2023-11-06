// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Security.Permissions;

/// <summary>Information about the application of an instance of a folder permission.</summary>
public interface IFolderPermissionInfo : IPermissionInfo
{
    /// <summary>Gets or sets the ID of the folder permission.</summary>
    int FolderPermissionId { get; set; }

    /// <summary>Gets or sets the folder ID to which the permission applies.</summary>
    int FolderId { get; set; }

    /// <summary>Gets or sets the path of the folder to which the permission applies.</summary>
    string FolderPath { get; set; }

    /// <summary>Gets or sets the portal ID of the folder to which the permission applies.</summary>
    int PortalId { get; set; }
}
