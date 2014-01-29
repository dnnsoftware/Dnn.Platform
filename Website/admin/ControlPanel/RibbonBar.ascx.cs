#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
#region Usings

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mobile;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.UI.ControlPanels
{
    public partial class RibbonBar : ControlPanelBase
    {
        public override bool IsDockable { get; set; }

        public override bool IncludeInControlHierarchy
        {
            get
            {
                return base.IncludeInControlHierarchy && (IsPageAdmin() || IsModuleAdmin());
            }
        }


        #region "Private Methods"

        private void Localize()
        {

            Control ctrl = AdminPanel.FindControl("SiteNewPage");
            if (((ctrl != null) && ctrl is DnnRibbonBarTool))
            {
                var toolCtrl = (DnnRibbonBarTool)ctrl;
                toolCtrl.Text = Localization.GetString("SiteNewPage", LocalResourceFile);
                toolCtrl.ToolTip = Localization.GetString("SiteNewPage.ToolTip", LocalResourceFile);
            }

        }

        private void SetMode(bool update)
        {
            if (update)
            {
                SetUserMode(ddlMode.SelectedValue);
            }

            if (!TabPermissionController.CanAddContentToPage())
            {
                RemoveModeDropDownItem("LAYOUT");
            }

            if (!(new PreviewProfileController().GetProfilesByPortal(this.PortalSettings.PortalId).Count > 0))
            {
                RemoveModeDropDownItem("PREVIEW");
            }

            switch (UserMode)
            {
                case PortalSettings.Mode.View:
                    ddlMode.FindItemByValue("VIEW").Selected = true;
                    break;
                case PortalSettings.Mode.Edit:
                    ddlMode.FindItemByValue("EDIT").Selected = true;
                    break;
                case PortalSettings.Mode.Layout:
                    ddlMode.FindItemByValue("LAYOUT").Selected = true;
                    break;
            }
        }

        private void RemoveModeDropDownItem(string value)
        {
            var item = ddlMode.FindItemByValue(value);
            if (item != null)
            {
                ddlMode.Items.Remove(item);
            }
        }

        private void SetLanguage(bool update)
		{
			if (update)
			{
				DotNetNuke.Services.Personalization.Personalization.SetProfile("Usability", "UICulture", ddlUICulture.SelectedValue);
			}
		}

        protected string GetButtonConfirmMessage(string toolName)
        {
            if (toolName == "DeletePage")
            {
                return ClientAPI.GetSafeJSString(Localization.GetString("Tool.DeletePage.Confirm", LocalResourceFile));
            }

            if (toolName == "CopyPermissionsToChildren")
            {
                if (PortalSecurity.IsInRole("Administrators"))
                {
                    return ClientAPI.GetSafeJSString(Localization.GetString("Tool.CopyPermissionsToChildren.Confirm", LocalResourceFile));
                }

                return ClientAPI.GetSafeJSString(Localization.GetString("Tool.CopyPermissionsToChildrenPageEditor.Confirm", LocalResourceFile));
            }

            if (toolName == "CopyDesignToChildren")
            {
                if (PortalSecurity.IsInRole("Administrators"))
                {
                    return ClientAPI.GetSafeJSString(Localization.GetString("Tool.CopyDesignToChildren.Confirm", LocalResourceFile));
                }

                return ClientAPI.GetSafeJSString(Localization.GetString("Tool.CopyDesignToChildrenPageEditor.Confirm", LocalResourceFile));
            }

            return string.Empty;
        }

        protected void DetermineNodesToInclude(object sender, EventArgs e)
        {
            var skinObject = (Web.DDRMenu.SkinObject)sender;
            string admin = StripLocalizationPrefix(Localization.GetString("//Admin.String", Localization.GlobalResourceFile)).Trim();
            string host = StripLocalizationPrefix(Localization.GetString("//Host.String", Localization.GlobalResourceFile)).Trim();

            skinObject.IncludeNodes = admin + ", " + host;

        }

        private string StripLocalizationPrefix(string s)
        {
            const string prefix = "[L]";

            if (s.StartsWith(prefix))
            {
                return s.Substring(prefix.Length);
            }

            return s;
        }

        #endregion

        #region "Event Handlers"

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ID = "RibbonBar";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

			ddlMode.SelectedIndexChanged += ddlMode_SelectedIndexChanged;
			ddlUICulture.SelectedIndexChanged += ddlUICulture_SelectedIndexChanged;

            try
            {
                AdminPanel.Visible = false;
                AdvancedToolsPanel.Visible = false;

                if (ControlPanel.Visible && IncludeInControlHierarchy)
                {
                    ClientResourceManager.RegisterStyleSheet(this.Page, "~/admin/ControlPanel/module.css");
                    jQuery.RequestHoverIntentRegistration();
                    ClientResourceManager.RegisterScript(this.Page, "~/Resources/ControlPanel/ControlPanel.debug.js");
                }

                jQuery.RequestDnnPluginsRegistration();

                Control copyPageButton = CurrentPagePanel.FindControl("CopyPage");
                if ((copyPageButton != null))
                {
                    copyPageButton.Visible = LocaleController.Instance.IsDefaultLanguage(LocaleController.Instance.GetCurrentLocale(PortalSettings.PortalId).Code);
                }


                if ((Request.IsAuthenticated))
                {
                    UserInfo user = UserController.GetCurrentUserInfo();
                    if (((user != null)))
                    {
                        bool isAdmin = user.IsInRole(PortalSettings.Current.AdministratorRoleName);
                        AdminPanel.Visible = isAdmin;
                    }
                }

				if (IsPageAdmin())
				{
					ControlPanel.Visible = true;
					BodyPanel.Visible = true;

                    if ((DotNetNukeContext.Current.Application.Name == "DNNCORP.CE"))
                    {
                        //Hide Support icon in CE
                        AdminPanel.FindControl("SupportTickets").Visible = false;
                    }
                    else
                    {
                        //Show PE/XE tools
                        AdvancedToolsPanel.Visible = true;
                    }

                    Localize();

					if (!Page.IsPostBack)
					{
						UserInfo objUser = UserController.GetCurrentUserInfo();
						if ((objUser != null))
						{
							if (objUser.IsSuperUser)
							{
								hypMessage.ImageUrl = Upgrade.UpgradeIndicator(DotNetNukeContext.Current.Application.Version, Request.IsLocal, Request.IsSecureConnection);
								if (!string.IsNullOrEmpty(hypMessage.ImageUrl))
								{
									hypMessage.ToolTip = Localization.GetString("hypUpgrade.Text", LocalResourceFile);
									hypMessage.NavigateUrl = Upgrade.UpgradeRedirect();
								}
							}
							else
							{
								if (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName) && Host.DisplayCopyright)
								{
									hypMessage.ImageUrl = "~/images/branding/iconbar_logo.png";
									hypMessage.ToolTip = DotNetNukeContext.Current.Application.Description;
									hypMessage.NavigateUrl = Localization.GetString("hypMessageUrl.Text", LocalResourceFile);
								}
								else
								{
									hypMessage.Visible = false;
								}

                                if (!TabPermissionController.CanAddContentToPage())
                                {
                                    CommonTasksPanel.Visible = false;
                                }
							}
							if (PortalSettings.AllowUserUICulture)
							{
								object oCulture = DotNetNuke.Services.Personalization.Personalization.GetProfile("Usability", "UICulture");
								string currentCulture;
								if (oCulture != null)
								{
									currentCulture = oCulture.ToString();
								}
								else
								{
									Localization l = new Localization();
									currentCulture = l.CurrentUICulture;
								}
								//Localization.LoadCultureDropDownList(ddlUICulture, CultureDropDownTypes.NativeName, currentCulture);
                                IEnumerable<ListItem> cultureListItems = Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, currentCulture, "", false);
                                foreach (var cultureItem in cultureListItems)
                                {
                                    ddlUICulture.AddItem(cultureItem.Text, cultureItem.Value);
                                }

                                var selectedCultureItem = ddlUICulture.FindItemByValue(currentCulture);
                                if (selectedCultureItem != null)
                                {
                                    selectedCultureItem.Selected = true;
                                }

								//only show language selector if more than one language
								if (ddlUICulture.Items.Count > 1)
								{
									lblUILanguage.Visible = true;
									ddlUICulture.Visible = true;

									if (oCulture == null)
									{
										SetLanguage(true);
									}
								}
							}
						}
						SetMode(false);
					}
				}
				else if (IsModuleAdmin())
				{
					ControlPanel.Visible = true;
					BodyPanel.Visible = false;
					adminMenus.Visible = false;
					if (!Page.IsPostBack)
					{
						SetMode(false);
					}
				}
				else
				{
					ControlPanel.Visible = false;
				}
			}
			catch (Exception exc)
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		protected void ddlMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Page.IsCallback)
			{
				return;
			}
			SetMode(true);
			Response.Redirect(Request.RawUrl, true);
		}

		private void ddlUICulture_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Page.IsCallback)
			{
				return;
			}
			SetLanguage(true);
			Response.Redirect(Request.RawUrl, true);
		}

		#endregion

		#region "Protected Methods"

		protected string PreviewPopup()
		{
			var previewUrl = string.Format("{0}/Default.aspx?ctl={1}&previewTab={2}&TabID={2}", 
										Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias), 
										"MobilePreview",
										PortalSettings.ActiveTab.TabID);

			if(PortalSettings.EnablePopUps)
			{
				return UrlUtils.PopUpUrl(previewUrl, this, PortalSettings, true, false, 660, 800);
			}
			else
			{
				return string.Format("location.href = \"{0}\"", previewUrl);
			}
		}

		#endregion
	}
}
