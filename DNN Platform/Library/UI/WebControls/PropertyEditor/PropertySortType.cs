// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Enumeration that determines the sort method.
    /// </summary>
    /// <remarks>
    /// PropertySortType is used by <see cref="DotNetNuke.UI.WebControls.PropertyEditorControl">PropertyEditorControl</see>
    /// to determine the order for displaying properties.
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public enum PropertySortType
    {
        None = 0,
        Alphabetical,
        Category,
        SortOrderAttribute,
    }
}
