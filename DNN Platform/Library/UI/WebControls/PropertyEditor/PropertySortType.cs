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
