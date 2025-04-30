// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls
{
    /// <summary>Enumeration that determines the sort method.</summary>
    /// <remarks>
    /// PropertySortType is used by <see cref="DotNetNuke.UI.WebControls.PropertyEditorControl">PropertyEditorControl</see>
    /// to determine the order for displaying properties.
    /// </remarks>
    public enum PropertySortType
    {
        /// <summary>Not sorted.</summary>
        None = 0,

        /// <summary>Sorted alphabetically.</summary>
        Alphabetical = 1,

        /// <summary>Sorted by category.</summary>
        Category = 2,

        /// <summary>Sorted by <see cref="DotNetNuke.UI.WebControls.SortOrderAttribute"/>.</summary>
        SortOrderAttribute = 3,
    }
}
