#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Entities.Urls;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.WebControls;

namespace DotNetNuke.Modules.UrlManagement
{
    public partial class AdvancedUrlSetttings : ModuleUserControlBase, IEditPageTabControlActions
    {
        protected void EditSettings(object source, CommandEventArgs args)
        {
            Response.Redirect(Globals.NavigateURL(ModuleContext.TabId, "UrlProviderSettings", "ProviderId=" + args.CommandArgument));
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            foreach (DataGridColumn column in providersGrid.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof(ImageCommandColumn)))
                {
                    var imageColumn = (ImageCommandColumn)column;

                    var formatString = ModuleContext.EditUrl("ProviderId", "KEYFIELD", "UrlProviderSettings");

                    formatString = formatString.Replace("KEYFIELD", "{0}");
                    imageColumn.NavigateURLFormatString = formatString;
                }
            }

        }

        public void BindAction(int portalId, int tabId, int moduleId)
        {
            var providers = ExtensionUrlProviderController.GetProviders(portalId);
			Localization.LocalizeDataGrid(ref providersGrid, LocalResourceFile);
            providersGrid.DataSource = providers;
            providersGrid.DataBind();

            UrlSettingsExtensionControl.BindAction(portalId, tabId, moduleId);

            var settings = new DotNetNuke.Entities.Urls.FriendlyUrlSettings(portalId);
            if (settings.EnableCustomProviders == false)
            {
                providersGrid.Visible = false;
                providersWarningLabel.Visible = true;
                providersWarningLabel.Text = LocalizeString("ExtensionProvidersDisabled.Text");
            }
            else
            {
                if (providersGrid.Items.Count == 0)
                {
                    providersGrid.Visible = false;
                    providersWarningLabel.Visible = true;
                    providersWarningLabel.Text = LocalizeString("NoProvidersInstalled.Text");
                }
            }
            
            
        }

        public void CancelAction(int portalId, int tabId, int moduleId)
        {
        }

        public void SaveAction(int portalId, int tabId, int moduleId)
        {
            UrlSettingsExtensionControl.SaveAction(portalId, tabId, moduleId);
        }

	    protected void OnChangeProviderStatus(object sender, EventArgs e)
	    {
		    var checkbox = (sender as CheckBox);
		    var providerId = Convert.ToInt32((checkbox.Parent.FindControl("urlProviderId") as HiddenField).Value);
		    if (checkbox.Checked)
		    {
			    ExtensionUrlProviderController.EnableProvider(providerId, ModuleContext.PortalId);
		    }
		    else
		    {
				ExtensionUrlProviderController.DisableProvider(providerId, ModuleContext.PortalId);
		    }
	    }
    }
}