// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Services.FileSystem
{
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
        /// Gets or sets a value indicating whether file versions are active for the folder
        /// </summary>
        bool IsVersioned { get; set; }

        /// <summary>
        /// Gets or sets a reference to the active Workflow for the folder
        /// </summary>
        int WorkflowID { get; set; }

        /// <summary>
        /// Gets or sets a reference to the parent folder
        /// </summary>
        int ParentID { get; set; }

        /// <summary>
        /// Gets or sets the path this folder is mapped on its provider file system
        /// </summary>
        string MappedPath { get; set; }

        /// <summary>
        /// Gets a value indicating whether the folder has any child subfolder
        /// </summary>
        bool HasChildren { get; }

        #region Supoort for BaseEntityInfo on inherited classes

        int CreatedByUserID { get; }

        DateTime CreatedOnDate { get; }

        int LastModifiedByUserID { get; }

        DateTime LastModifiedOnDate { get; }

        UserInfo CreatedByUser(int portalId);

        UserInfo LastModifiedByUser(int portalId);

        #endregion
    }
}
