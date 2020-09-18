// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>
    /// This entity represents a Workflow.
    /// </summary>
    [PrimaryKey("WorkflowID")]
    [TableName("ContentWorkflows")]
    [Serializable]
    public class Workflow
    {
        /// <summary>
        /// Gets first workflow state.
        /// </summary>
        [IgnoreColumn]
        public WorkflowState FirstState
        {
            get
            {
                return this.States == null || !this.States.Any() ? null : this.States.OrderBy(s => s.Order).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets last workflow state.
        /// </summary>
        [IgnoreColumn]
        public WorkflowState LastState
        {
            get
            {
                return this.States == null || !this.States.Any() ? null : this.States.OrderBy(s => s.Order).LastOrDefault();
            }
        }

        /// <summary>
        /// Gets or sets workflow Id.
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// Gets or sets portal Id.
        /// </summary>
        public int PortalID { get; set; }

        /// <summary>
        /// Gets or sets workflow Name.
        /// </summary>
        [Required]
        [StringLength(40)]
        public string WorkflowName { get; set; }

        /// <summary>
        /// Gets or sets workflow Key. This property can be used to.
        /// </summary>
        [StringLength(40)]
        public string WorkflowKey { get; set; }

        /// <summary>
        /// Gets or sets workflow Description.
        /// </summary>
        [StringLength(256)]
        public string Description { get; set; }

        /// <summary>
        /// Gets a value indicating whether system workflow have a special behavior. It cannot be deleted and new states cannot be added.
        /// </summary>
        public bool IsSystem { get; internal set; }

        /// <summary>
        /// Gets workflow states.
        /// </summary>
        [IgnoreColumn]
        public IEnumerable<WorkflowState> States { get; internal set; }
    }
}
