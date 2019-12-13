#region Usings

using System.Collections;

#endregion

namespace DotNetNuke.Security.Roles
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Roles
    /// Class:      RoleComparer
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The RoleComparer class provides an Implementation of IComparer for
    /// RoleInfo objects
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class RoleComparer : IComparer
    {
        #region IComparer Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares two RoleInfo objects by performing a comparison of their rolenames
        /// </summary>
        /// <param name="x">One of the items to compare</param>
        /// <param name="y">One of the items to compare</param>
        /// <returns>An Integer that determines whether x is greater, smaller or equal to y </returns>
        /// -----------------------------------------------------------------------------
        public int Compare(object x, object y)
        {
            return new CaseInsensitiveComparer().Compare(((RoleInfo) x).RoleName, ((RoleInfo) y).RoleName);
        }

        #endregion
    }
}
