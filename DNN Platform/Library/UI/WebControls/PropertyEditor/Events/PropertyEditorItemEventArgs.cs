// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      PropertyEditorItemEventArgs
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PropertyEditorItemEventArgs class is a cusom EventArgs class for
    /// handling Event Args.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class PropertyEditorItemEventArgs : EventArgs
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEditorItemEventArgs"/> class.
        /// Constructs a new PropertyEditorItemEventArgs.
        /// </summary>
        /// <param name="editor">The editor created.</param>
        /// -----------------------------------------------------------------------------
        public PropertyEditorItemEventArgs(EditorInfo editor)
        {
            this.Editor = editor;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets whether the proeprty has changed.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public EditorInfo Editor { get; set; }
    }
}
