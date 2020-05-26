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
    /// Class	 : CompareModulePermissions
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// CompareModulePermissions provides the a custom IComparer implementation for
    /// ModulePermissionInfo objects
    /// </summary>
    /// -----------------------------------------------------------------------------
    internal class CompareModulePermissions : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            return ((ModulePermissionInfo) x).ModulePermissionID.CompareTo(((ModulePermissionInfo) y).ModulePermissionID);
        }

        #endregion
    }
}
