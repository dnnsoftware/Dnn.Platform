// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles
{
    using System.Collections;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Roles
    /// Class:      RoleComparer
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The RoleComparer class provides an Implementation of IComparer for
    /// RoleInfo objects.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class RoleComparer : IComparer
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares two RoleInfo objects by performing a comparison of their rolenames.
        /// </summary>
        /// <param name="x">One of the items to compare.</param>
        /// <param name="y">One of the items to compare.</param>
        /// <returns>An Integer that determines whether x is greater, smaller or equal to y. </returns>
        /// -----------------------------------------------------------------------------
        public int Compare(object x, object y)
        {
            return new CaseInsensitiveComparer().Compare(((RoleInfo)x).RoleName, ((RoleInfo)y).RoleName);
        }
    }
}
