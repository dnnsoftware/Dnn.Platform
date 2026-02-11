// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Security.Permissions;

    /// <summary>This entity represents a state permission.</summary>
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class WorkflowStatePermission : PermissionInfoBase
    {
        /// <summary>Gets or sets workflow state permission ID.</summary>
        public int WorkflowStatePermissionID { get; set; }

        /// <summary>Gets or sets state ID.</summary>
        public int StateID { get; set; }
    }
}
