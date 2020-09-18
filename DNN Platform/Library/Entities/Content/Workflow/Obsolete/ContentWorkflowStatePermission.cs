// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow

// ReSharper enable CheckNamespace
{
    using System;

    using DotNetNuke.Security.Permissions;

    /// <summary>
    /// This entity represents a state permission.
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
    public class ContentWorkflowStatePermission : PermissionInfoBase
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
