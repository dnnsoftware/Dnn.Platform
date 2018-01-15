#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
