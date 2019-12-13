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
    internal class CompareTabPermissions : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            return ((TabPermissionInfo) x).TabPermissionID.CompareTo(((TabPermissionInfo) y).TabPermissionID);
        }

        #endregion
    }
}
