// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow
// ReSharper enable CheckNamespace
{
    /// <summary>
    /// This entity represents a Workflow
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
    public class ContentWorkflow
    {
        /// <summary>
        /// Workflow Id
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// Portal Id
        /// </summary>
        public int PortalID { get; set; }

        /// <summary>
        /// Workflow Name
        /// </summary>
        public string WorkflowName { get; set; }
        
        /// <summary>
        /// Workflow Description
        /// </summary>
        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public bool StartAfterCreating { get; set; }

        public bool StartAfterEditing { get; set; }

        public bool DispositionEnabled { get; set; }

        /// <summary>
        /// Workflow states
        /// </summary>
        public IEnumerable<ContentWorkflowState> States { get; set; }
        
    }
}
