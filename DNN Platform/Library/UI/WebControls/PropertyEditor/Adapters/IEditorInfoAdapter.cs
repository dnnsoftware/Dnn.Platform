namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      IEditorInfoAdapter
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The IEditorInfoAdapter control provides an Adapter Interface for datasources 
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public interface IEditorInfoAdapter
    {
        EditorInfo CreateEditControl();

        bool UpdateValue(PropertyEditorEventArgs e);

        bool UpdateVisibility(PropertyEditorEventArgs e);
    }
}
