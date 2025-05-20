// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls
{
    /// <summary>Enumeration that determines the grouping method.</summary>
    /// <remarks>
    /// GroupByMode is used by <see cref="DotNetNuke.UI.WebControls.PropertyEditorControl">PropertyEditorControl</see>
    /// to determine the grouping mode for displaying properties.
    /// </remarks>
    public enum GroupByMode
    {
        /// <summary>No grouping.</summary>
        None = 0,

        /// <summary>Group by section.</summary>
        Section = 1,
    }
}
