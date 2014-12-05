/*
 * CKEditor Html Editor Provider for DotNetNuke
 * ========
 * http://dnnckeditor.codeplex.com/
 * Copyright (C) Ingo Herbote
 *
 * The software, this file and its contents are subject to the CKEditor Provider
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of the CKEditor Provider.
 */

namespace WatchersNET.CKEditor.Objects
{
    using System.Collections.Generic;

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
            this.ToolbarGroups = new List<ToolbarGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarSet" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ToolbarSet(string name)
        {
            this.Name = name;
            this.ToolbarGroups = new List<ToolbarGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarSet" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="priority">The priority.</param>
        public ToolbarSet(string name, int priority)
        {
            this.Name = name;
            this.Priority = priority;
            this.ToolbarGroups = new List<ToolbarGroup>();
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