// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      IEditorInfoAdapter
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The IEditorInfoAdapter control provides an Adapter Interface for datasources.
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
