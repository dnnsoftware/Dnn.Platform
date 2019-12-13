// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.ComponentModel.DataAnnotations;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    /// <summary>
    /// This entity represents a Workflow Log
    /// </summary>
    [PrimaryKey("WorkflowLogID")]
    [TableName("ContentWorkflowLogs")]
    [Serializable]
    public class WorkflowLog
    {
        /// <summary>
        /// Workflow log Id
        /// </summary>
        public int WorkflowLogID { get; set; }

        /// <summary>
        /// Workflow associated to the log entry
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// Content Item associated to the log entry
        /// </summary>
        public int ContentItemID { get; set; }

        /// <summary>
        /// Type (<see cref="WorkflowLogType"/> enum)
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Action name (usually is a localized representation of the ContentWorkflowLogType)
        /// </summary>
        [StringLength(40)]
        public string Action { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
        [StringLength(256)]
        public string Comment { get; set; }

        /// <summary>
        /// Log date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// User Id associated to the log
        /// </summary>
        public int User { get; set; }
    }
}
