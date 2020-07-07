// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>
    /// This entity represents a workflow action implementation.
    /// </summary>
    [PrimaryKey("ActionId")]
    [TableName("ContentWorkflowActions")]
    public class WorkflowAction
    {
        /// <summary>
        /// Gets or sets action Id.
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// Gets or sets content item type Id.
        /// </summary>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// Gets or sets action type. This is a string representation of the enum <see cref="WorkflowActionTypes"/>.
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Gets or sets action Source. This property represents the path to the class that implement the IWorkflowAction interface
        /// i.e.: "MyProject.WorkflowActions.WorkflowDiscardction, MyProject".
        /// </summary>
        public string ActionSource { get; set; }
    }
}
