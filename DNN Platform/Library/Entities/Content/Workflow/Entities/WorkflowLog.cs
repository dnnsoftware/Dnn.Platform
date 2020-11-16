// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>
    /// This entity represents a Workflow Log.
    /// </summary>
    [PrimaryKey("WorkflowLogID")]
    [TableName("ContentWorkflowLogs")]
    [Serializable]
    public class WorkflowLog
    {
        /// <summary>
        /// Gets or sets workflow log Id.
        /// </summary>
        public int WorkflowLogID { get; set; }

        /// <summary>
        /// Gets or sets workflow associated to the log entry.
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// Gets or sets content Item associated to the log entry.
        /// </summary>
        public int ContentItemID { get; set; }

        /// <summary>
        /// Gets or sets type (<see cref="WorkflowLogType"/> enum).
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Gets or sets action name (usually is a localized representation of the ContentWorkflowLogType).
        /// </summary>
        [StringLength(40)]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets comment.
        /// </summary>
        [StringLength(256)]
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets log date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets user Id associated to the log.
        /// </summary>
        public int User { get; set; }
    }
}
