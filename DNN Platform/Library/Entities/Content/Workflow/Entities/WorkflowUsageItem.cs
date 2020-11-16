// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    using System;

    /// <summary>
    /// This entity represents an item which is using a Workflow.
    /// </summary>
    [Serializable]
    public class WorkflowUsageItem
    {
        /// <summary>
        /// Gets or sets workflow Id.
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// Gets or sets name of the item.
        /// </summary>
        public string ContentName { get; set; }

        /// <summary>
        /// Gets or sets type of the item.
        /// </summary>
        public string ContentType { get; set; }
    }
}
