// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls
{
    /// <summary>Enumeration that determines the field that a List binds to.</summary>
    /// <remarks>
    /// LabelMode is used by <see cref="DotNetNuke.UI.WebControls.PropertyEditorControl">PropertyEditorControl</see>
    /// to determine how the label is displayed.
    /// </remarks>
    public enum ListBoundField
    {
        /// <summary>Display the ID.</summary>
        Id = 0,

        /// <summary>Display the value.</summary>
        Value = 1,

        /// <summary>Display the text.</summary>
        Text = 2,
    }
}
