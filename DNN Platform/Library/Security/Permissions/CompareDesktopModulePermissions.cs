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
    /// Class	 : CompareDesktopModulePermissions
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// CompareDesktopModulePermissions provides the a custom IComparer implementation for
    /// DesktopModulePermissionInfo objects
    /// </summary>
    /// -----------------------------------------------------------------------------
    internal class CompareDesktopModulePermissions : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            return ((DesktopModulePermissionInfo) x).DesktopModulePermissionID.CompareTo(((DesktopModulePermissionInfo) y).DesktopModulePermissionID);
        }

        #endregion
    }
}
