// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow;

// ReSharper enable CheckNamespace
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Security.Permissions;

/// <summary>This entity represents a state permission.</summary>
[DnnDeprecated(7, 4, 0, "Use IWorkflowEngine", RemovalVersion = 10)]
public partial class ContentWorkflowStatePermission : PermissionInfoBase
{
    /// <summary>Gets or sets workflow state permission Id.</summary>
    public int WorkflowStatePermissionID { get; set; }

    /// <summary>Gets or sets state Id.</summary>
    public int StateID { get; set; }
}
