// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Permissions
{
    using System.Collections;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : CompareWorkflowStatePermissions
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   CompareWorkflowStatePermissions provides the a custom IComparer implementation for
    ///   WorkflowStatePermissionInfo objects.
    /// </summary>
    internal class CompareWorkflowStatePermissions : IComparer
    {
        public int Compare(object x, object y)
        {
            return ((WorkflowStatePermissionInfo)x).WorkflowStatePermissionID.CompareTo(((WorkflowStatePermissionInfo)y).WorkflowStatePermissionID);
        }
    }
}
