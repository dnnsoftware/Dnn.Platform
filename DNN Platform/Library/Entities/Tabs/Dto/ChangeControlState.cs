using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Entities.Tabs.Dto
{
    /// <summary>
    /// Class that represents the full state of a tab regarding if versioning and workflow are enabled.
    /// </summary>
    public class ChangeControlState
    {
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
        /// Gets if change control is enabled for the tab.
        /// </summary>
        /// <value>
        /// True if versioning or workflow are enabled, false otherwise.
        /// </value>
        public bool IsChangeControlEnabledForTab
        {
            get { return IsVersioningEnabledForTab || IsWorkflowEnabledForTab; }
        }

        /// <summary>
        /// Gets if versioning is enabled for the tab.
        /// </summary>
        /// <value>
        /// True if versioning is enabled, false otherwise.
        /// </value>
        public bool IsVersioningEnabledForTab { get; set; }
        /// <summary>
        /// Gets if workflow is enabled for the tab.
        /// </summary>
        /// <value>
        /// True if workflow is enabled, false otherwise.
        /// </value>
        public bool IsWorkflowEnabledForTab { get; set; }
    }
}
