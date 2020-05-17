// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : CompareTabPermissions
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// CompareTabPermissions provides the a custom IComparer implementation for
    /// TabPermissionInfo objects
    /// </summary>
    /// -----------------------------------------------------------------------------
    internal class CompareFolderPermissions : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            return ((FolderPermissionInfo) x).FolderPermissionID.CompareTo(((FolderPermissionInfo) y).FolderPermissionID);
        }

        #endregion
    }
}
