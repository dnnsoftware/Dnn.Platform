#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
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