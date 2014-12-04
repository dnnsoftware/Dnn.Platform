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
    /// Toolbar Group Class
    /// </summary>
    public class ToolbarGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarGroup" /> class.
        /// </summary>
        public ToolbarGroup()
        {
            this.items = new List<string>();
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