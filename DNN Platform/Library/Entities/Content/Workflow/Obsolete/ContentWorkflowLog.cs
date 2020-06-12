// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow

// ReSharper enable CheckNamespace
{
    /// <summary>
    /// This entity represents a Workflow Log
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
    public class ContentWorkflowLog
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
        /// Action name (usually is a localized representation of the ContentWorkflowLogType)
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
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
