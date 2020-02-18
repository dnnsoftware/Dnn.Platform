// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Security.Permissions;

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow
// ReSharper enable CheckNamespace
{
    /// <summary>
    /// This entity represents a state permission
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
    public class ContentWorkflowStatePermission : PermissionInfoBase
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
