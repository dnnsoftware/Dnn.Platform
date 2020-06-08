// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.ComponentModel.DataAnnotations;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    /// <summary>
    /// This entity represents a Workflow State
    /// </summary>
    [PrimaryKey("StateID")]
    [TableName("ContentWorkflowStates")]
    [Serializable]
    public class WorkflowState 
    {
        /// <summary>
        /// State Id
        /// </summary>
        public int StateID { get; set; }

        /// <summary>
        /// Workflow associated to the state
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// State name
        /// </summary>
        [Required]
        [StringLength(40)]
        public string StateName { get; set; }

        /// <summary>
        /// State Order
        /// </summary>
        public int Order { get; internal set; } 

        /// <summary>
        /// Indicates if the state is a system state. System states (i.e.: Draft, Published) have a special behavior. They cannot be deleted or moved.
        /// </summary>
        public bool IsSystem { get; internal set; }

        /// <summary>
        /// If set to true the Workflow Engine will send system notification to the reviewer of the state when the workflow reach it
        /// </summary>
        public bool SendNotification { get; set; }

        /// <summary>
        /// If set to true the Workflow Engine <see cref="IWorkflowEngine"/> will send system notification to administrators user when the workflow reach it
        /// </summary>
        public bool SendNotificationToAdministrators { get; set; }
    }
}
