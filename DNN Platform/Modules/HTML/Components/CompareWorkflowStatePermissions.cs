// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
