// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class that represents the full state of a tab regarding if versioning and workflow are enabled.
    /// </summary>
    public class ChangeControlState
    {
        /// <summary>
        /// Gets a value indicating whether gets if change control is enabled for the tab.
        /// </summary>
        /// <value>
        /// True if versioning or workflow are enabled, false otherwise.
        /// </value>
        public bool IsChangeControlEnabledForTab
        {
            get { return this.IsVersioningEnabledForTab || this.IsWorkflowEnabledForTab; }
        }

        /// <summary>
        /// Gets or sets the portal id.
        /// </summary>
        /// <value>
        /// The portal id.
        /// </value>
        public int PortalId { get; set; }

        /// <summary>
        /// Gets or sets the tab id.
        /// </summary>
        /// <value>
        /// The tab id.
        /// </value>
        public int TabId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets if versioning is enabled for the tab.
        /// </summary>
        /// <value>
        /// True if versioning is enabled, false otherwise.
        /// </value>
        public bool IsVersioningEnabledForTab { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets if workflow is enabled for the tab.
        /// </summary>
        /// <value>
        /// True if workflow is enabled, false otherwise.
        /// </value>
        public bool IsWorkflowEnabledForTab { get; set; }
    }
}
