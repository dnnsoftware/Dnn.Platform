﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Modules.HtmlEditorManager.Views
{
    using System;
    using System.Web.UI.WebControls;

    using ViewModels;
    using Web.Mvp;

    /// <summary>
    /// View control for selecting an HTML provider
    /// </summary>
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public partial class ProviderConfiguration : ModuleView<ProviderConfigurationViewModel>, IProviderConfigurationView
    {
        /// <summary>Occurs when the save button is clicked.</summary>
        public event EventHandler<EditorEventArgs> SaveEditorChoice;

        /// <summary>Occurs when the editor is changed.</summary>
        public event EventHandler<EditorEventArgs> EditorChanged;

        /// <summary>Gets or sets the editor panel.</summary>
        /// <value>The editor panel.</value>
        public PlaceHolder Editor
        {
            get
            {
                return this.EditorPanel;
            }

            set
            {
                this.EditorPanel = value;
            }
        }

        /// <summary>Handles the Click event of the SaveButton control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SaveButton_Click(object sender, EventArgs e)
        {
            this.SaveEditorChoice(this, new EditorEventArgs(this.ProvidersDropDownList.SelectedValue));
        }

        /// <summary>Handles the SelectedIndexChanged event of the ProvidersDropDownList control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ProvidersDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.EditorChanged(this, new EditorEventArgs(this.ProvidersDropDownList.SelectedValue));
        }

        public void Refresh()
        {
            Response.Redirect(Request.RawUrl, true);
        }

        protected override void OnInit(EventArgs e)
        {
            var currentUser = UserController.Instance.GetCurrentUserInfo();
            if (currentUser == null || !currentUser.IsSuperUser)
            {
                LocalResourceFile = "/DesktopModules/Admin/HtmlEditorManager/App_LocalResources/ProviderConfiguration.ascx.resx";
                Globals.Redirect(Globals.AccessDeniedURL(LocalizeString("CannotManageHTMLEditorProviders")), true);
            }
            base.OnInit(e);
        }        
    }
}
