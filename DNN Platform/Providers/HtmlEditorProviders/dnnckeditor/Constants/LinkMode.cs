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

namespace WatchersNET.CKEditor.Constants
{
    /// <summary>
    /// File Browser Link Modes
    /// </summary>
    public enum LinkMode
    {
        /// <summary>
        /// Relative URL
        /// </summary>
        RelativeURL = 0,

        /// <summary>
        /// Absolute URL
        /// </summary>
        AbsoluteURL = 1,

        /// <summary>
        /// Relative Secured URL
        /// </summary>
        RelativeSecuredURL = 2,

        /// <summary>
        /// Absolute Secured URL
        /// </summary>
        AbsoluteSecuredURL = 3,
    }
}