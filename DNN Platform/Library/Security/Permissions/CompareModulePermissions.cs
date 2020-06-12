// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System.Collections;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : CompareModulePermissions
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
            return ((ModulePermissionInfo)x).ModulePermissionID.CompareTo(((ModulePermissionInfo)y).ModulePermissionID);
        }

        #endregion
    }
}
