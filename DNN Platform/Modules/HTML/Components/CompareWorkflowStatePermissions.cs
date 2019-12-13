using System.Collections;


namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : CompareWorkflowStatePermissions
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   CompareWorkflowStatePermissions provides the a custom IComparer implementation for 
    ///   WorkflowStatePermissionInfo objects
    /// </summary>
    internal class CompareWorkflowStatePermissions : IComparer
    {
        #region IComparer Interface

        public int Compare(object x, object y)
        {
            return ((WorkflowStatePermissionInfo) x).WorkflowStatePermissionID.CompareTo(((WorkflowStatePermissionInfo) y).WorkflowStatePermissionID);
        }

        #endregion
    }
}
