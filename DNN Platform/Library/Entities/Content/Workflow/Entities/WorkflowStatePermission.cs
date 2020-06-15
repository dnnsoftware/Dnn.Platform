// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    using DotNetNuke.Security.Permissions;

    /// <summary>
    /// This entity represents a state permission.
    /// </summary>
    public class WorkflowStatePermission : PermissionInfoBase
    {
        /// <summary>
        /// Gets or sets workflow state permission Id.
        /// </summary>
        public int WorkflowStatePermissionID { get; set; }

        /// <summary>
        /// Gets or sets state Id.
        /// </summary>
        public int StateID { get; set; }
    }
}
