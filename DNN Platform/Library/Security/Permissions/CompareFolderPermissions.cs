// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
