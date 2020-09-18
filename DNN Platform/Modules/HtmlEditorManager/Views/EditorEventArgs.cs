// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.HtmlEditorManager.Views
{
    using System;

    /// <summary>
    /// Contains information about the selected editor when saving.
    /// </summary>
    public class EditorEventArgs : EventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="EditorEventArgs" /> class.</summary>
        /// <param name="editor">The editor.</param>
        public EditorEventArgs(string editor)
        {
            this.Editor = editor;
        }

        /// <summary>Gets a value indicating whether this data comes from a valid form submission.</summary>
        public string Editor { get; private set; }
    }
}
