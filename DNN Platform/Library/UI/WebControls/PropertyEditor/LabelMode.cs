// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls
{
    /// <summary>Enumeration that determines the label mode.</summary>
    /// <remarks>
    /// LabelMode is used by <see cref="DotNetNuke.UI.WebControls.PropertyEditorControl">PropertyEditorControl</see>
    /// to determine how the label is displayed.
    /// </remarks>
    public enum LabelMode
    {
        /// <summary>No label.</summary>
        None = 0,

        /// <summary>Label on the left.</summary>
        Left = 1,

        /// <summary>Label on the right.</summary>
        Right = 2,

        /// <summary>Label on the top.</summary>
        Top = 3,

        /// <summary>Label on the bottom.</summary>
        Bottom = 4,
    }
}
