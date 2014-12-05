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
    /// <summary>
    /// Toolbar Button Class
    /// </summary>
    public class ToolbarButton
    {
        /// <summary>
        /// Gets or sets The Name of the Toolbar Button
        /// </summary>
        public string ToolbarName { get; set; }

        /// <summary>
        /// Gets or sets The Toolbar Icon
        /// </summary>
        public string ToolbarIcon { get; set; }
    }
}