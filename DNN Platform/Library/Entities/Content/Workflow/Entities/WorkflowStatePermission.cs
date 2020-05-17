// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    /// <summary>
    /// This entity represents a state permission
    /// </summary>
    public class WorkflowStatePermission : PermissionInfoBase
    {
        /// <summary>
        /// Workflow state permission Id
        /// </summary>
        public int WorkflowStatePermissionID { get; set; }

        /// <summary>
        /// State Id
        /// </summary>
        public int StateID { get; set; }
    }
}
