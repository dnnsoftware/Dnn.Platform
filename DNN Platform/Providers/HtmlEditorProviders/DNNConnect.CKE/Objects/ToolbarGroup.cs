
using System.Collections.Generic;

namespace DNNConnect.CKEditorProvider.Objects
{
    /// <summary>
    /// Toolbar Group Class
    /// </summary>
    public class ToolbarGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarGroup" /> class.
        /// </summary>
        public ToolbarGroup()
        {
            items = new List<string>();
        }

        /// <summary>
        /// Gets or sets the toolbar buttons.
        /// </summary>
        /// <value>
        /// The toolbar buttons.
        /// </value>
        public List<string> items { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string name { get; set; }
    }
}