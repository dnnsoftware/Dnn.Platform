// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System.Collections;

    /// <summary>Provides a custom <see cref="IComparer"/> implementation for <see cref="DesktopModulePermissionInfo"/> objects.</summary>
    internal class CompareDesktopModulePermissions : IComparer
    {
        /// <inheritdoc/>
        public int Compare(object x, object y)
        {
            return ((DesktopModulePermissionInfo)x).DesktopModulePermissionID.CompareTo(((DesktopModulePermissionInfo)y).DesktopModulePermissionID);
        }
    }
}
