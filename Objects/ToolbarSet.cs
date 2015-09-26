
using System.Collections.Generic;

namespace DNNConnect.CKEditorProvider.Objects
{
    /// <summary>
    /// Toolbar Set Class
    /// </summary>
    public class ToolbarSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarSet" /> class.
        /// </summary>
        public ToolbarSet()
        {
            ToolbarGroups = new List<ToolbarGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarSet" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ToolbarSet(string name)
        {
            Name = name;
            ToolbarGroups = new List<ToolbarGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarSet" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="priority">The priority.</param>
        public ToolbarSet(string name, int priority)
        {
            Name = name;
            Priority = priority;
            ToolbarGroups = new List<ToolbarGroup>();
        }

        /// <summary>
        /// Gets or sets The Name of the Toolbar Set
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Toolbar Priority from 1-20
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the toolbar group.
        /// </summary>
        /// <value>
        /// The toolbar group.
        /// </value>
        public List<ToolbarGroup> ToolbarGroups { get; set; }
    }
}