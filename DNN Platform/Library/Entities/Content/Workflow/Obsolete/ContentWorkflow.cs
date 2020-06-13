// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow

// ReSharper enable CheckNamespace
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This entity represents a Workflow.
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
    public class ContentWorkflow
    {
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
        public string WorkflowName { get; set; }

        /// <summary>
        /// Gets or sets workflow Description.
        /// </summary>
        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public bool StartAfterCreating { get; set; }

        public bool StartAfterEditing { get; set; }

        public bool DispositionEnabled { get; set; }

        /// <summary>
        /// Gets or sets workflow states.
        /// </summary>
        public IEnumerable<ContentWorkflowState> States { get; set; }
    }
}
