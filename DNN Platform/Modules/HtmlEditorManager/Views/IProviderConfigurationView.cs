// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.HtmlEditorManager.Views
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.HtmlEditorManager.ViewModels;
    using DotNetNuke.Web.Mvp;

    /// <summary>
    /// Interface for the Provider Configuration View.
    /// </summary>
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public interface IProviderConfigurationView : IModuleView<ProviderConfigurationViewModel>
    {
        /// <summary>Occurs when [save editor choice].</summary>
        event EventHandler<EditorEventArgs> SaveEditorChoice;

        /// <summary>Occurs when the editor changed.</summary>
        event EventHandler<EditorEventArgs> EditorChanged;

        /// <summary>Gets or sets the editor panel.</summary>
        /// <value>The editor panel.</value>
        PlaceHolder Editor { get; set; }

        void Refresh();
    }
}
