﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;

    public interface IFolderInfo
    {
        string DisplayName { get; set; }

        string DisplayPath { get; set; }

        int FolderID { get; set; }

        string FolderName { get; }

        string FolderPath { get; set; }

        FolderPermissionCollection FolderPermissions { get; }

        bool IsCached { get; set; }

        bool IsProtected { get; set; }

        DateTime LastUpdated { get; set; }

        string PhysicalPath { get; }

        int PortalID { get; set; }

        int StorageLocation { get; set; }

        Guid UniqueId { get; set; }

        Guid VersionGuid { get; set; }

        int FolderMappingID { get; set; }

        bool IsStorageSecure { get; }

        /// <summary>
        /// Gets or sets a value indicating whether file versions are active for the folder.
        /// </summary>
        bool IsVersioned { get; set; }

        /// <summary>
        /// Gets or sets a reference to the active Workflow for the folder.
        /// </summary>
        int WorkflowID { get; set; }

        /// <summary>
        /// Gets or sets a reference to the parent folder.
        /// </summary>
        int ParentID { get; set; }

        /// <summary>
        /// Gets or sets the path this folder is mapped on its provider file system.
        /// </summary>
        string MappedPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether the folder has any child subfolder.
        /// </summary>
        bool HasChildren { get; }

        int CreatedByUserID { get; }

        DateTime CreatedOnDate { get; }

        int LastModifiedByUserID { get; }

        DateTime LastModifiedOnDate { get; }

        UserInfo CreatedByUser(int portalId);

        UserInfo LastModifiedByUser(int portalId);
    }
}
