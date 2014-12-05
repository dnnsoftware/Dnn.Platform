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
    /// A Link Item Class
    /// </summary>
    public class LinkItem
    {
        #region Properties

        /// <summary>
        ///   Gets or sets Anchor.
        /// </summary>
        public string Anchor { get; set; }

        /// <summary>
        ///   Gets or sets Href.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        ///   Gets or sets Text.
        /// </summary>
        public string Text { get; set; }

        #endregion
    }
}