// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
        SortOrderAttribute
    }
}
