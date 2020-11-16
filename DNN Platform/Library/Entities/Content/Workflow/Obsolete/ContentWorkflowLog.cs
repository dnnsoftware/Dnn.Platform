// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow

// ReSharper enable CheckNamespace
{
    using System;

    /// <summary>
    /// This entity represents a Workflow Log.
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
    public class ContentWorkflowLog
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
        /// Gets or sets action name (usually is a localized representation of the ContentWorkflowLogType).
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets comment.
        /// </summary>
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
