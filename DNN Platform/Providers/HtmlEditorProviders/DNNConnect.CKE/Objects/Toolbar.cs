// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace DNNConnect.CKEditorProvider.Objects
{
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
