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
    using System;

    /// <summary>
    /// Toolbar Class
    /// </summary>
    [Obsolete("This class is phasing out please use ToolbarSet Class instead")]
    public class Toolbar
    {
        /// <summary>
        /// Gets or sets The Name of the Toolbar Set
        /// </summary>
        public string sToolbarName { get; set; }

        /// <summary>
        /// Gets or sets The Hole Toolbar Set
        /// </summary>
        public string sToolbarSet { get; set; }

        /// <summary>
        /// Gets or sets Toolbar Prioritity from 1-20
        /// </summary>
        public int iPriority { get; set; }
    }
}
