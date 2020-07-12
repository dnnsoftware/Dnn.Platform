// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.ControlPanels
{
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
    using DotNetNuke.Framework.JavaScriptLibraries;
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

    public partial class RibbonBar : ControlPanelBase
    {
        public override bool IncludeInControlHierarchy
        {
            get
            {
                return base.IncludeInControlHierarchy && (this.IsPageAdmin() || this.IsModuleAdmin());
            }
        }

        public override bool IsDockable { get; set; }

        protected string GetButtonConfirmMessage(string toolName)
        {
            if (toolName == "DeletePage")
            {
                return ClientAPI.GetSafeJSString(Localization.GetString("Tool.DeletePage.Confirm", this.LocalResourceFile));
            }

            if (toolName == "CopyPermissionsToChildren")
            {
                if (PortalSecurity.IsInRole("Administrators"))
                {
                    return ClientAPI.GetSafeJSString(Localization.GetString("Tool.CopyPermissionsToChildren.Confirm", this.LocalResourceFile));
                }

                return ClientAPI.GetSafeJSString(Localization.GetString("Tool.CopyPermissionsToChildrenPageEditor.Confirm", this.LocalResourceFile));
            }

            if (toolName == "CopyDesignToChildren")
            {
                if (PortalSecurity.IsInRole("Administrators"))
                {
                    return ClientAPI.GetSafeJSString(Localization.GetString("Tool.CopyDesignToChildren.Confirm", this.LocalResourceFile));
                }

                return ClientAPI.GetSafeJSString(Localization.GetString("Tool.CopyDesignToChildrenPageEditor.Confirm", this.LocalResourceFile));
            }

            return string.Empty;
        }

        protected void DetermineNodesToInclude(object sender, EventArgs e)
        {
            var skinObject = (Web.DDRMenu.SkinObject)sender;
            string admin = this.StripLocalizationPrefix(Localization.GetString("//Admin.String", Localization.GlobalResourceFile)).Trim();
            string host = this.StripLocalizationPrefix(Localization.GetString("//Host.String", Localization.GlobalResourceFile)).Trim();

            skinObject.IncludeNodes = admin + ", " + host;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.ID = "RibbonBar";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.ddlMode.SelectedIndexChanged += this.ddlMode_SelectedIndexChanged;
            this.ddlUICulture.SelectedIndexChanged += this.ddlUICulture_SelectedIndexChanged;

            try
            {
                this.AdminPanel.Visible = false;
                this.AdvancedToolsPanel.Visible = false;

                if (this.ControlPanel.Visible && this.IncludeInControlHierarchy)
                {
                    ClientResourceManager.RegisterStyleSheet(this.Page, "~/admin/ControlPanel/module.css");
                    ClientResourceManager.RegisterScript(this.Page, "~/Resources/ControlPanel/ControlPanel.debug.js");
                }

                JavaScript.RequestRegistration(CommonJs.DnnPlugins);

                Control copyPageButton = this.CurrentPagePanel.FindControl("CopyPage");
                if (copyPageButton != null)
                {
                    copyPageButton.Visible = LocaleController.Instance.IsDefaultLanguage(LocaleController.Instance.GetCurrentLocale(this.PortalSettings.PortalId).Code);
                }

                if (this.Request.IsAuthenticated)
                {
                    UserInfo user = UserController.Instance.GetCurrentUserInfo();
                    if (user != null)
                    {
                        bool isAdmin = user.IsInRole(PortalSettings.Current.AdministratorRoleName);
                        this.AdminPanel.Visible = isAdmin;
                    }
                }

                if (this.IsPageAdmin())
                {
                    this.ControlPanel.Visible = true;
                    this.BodyPanel.Visible = true;

                    if (DotNetNukeContext.Current.Application.Name == "DNNCORP.CE")
                    {
                        // Hide Support icon in CE
                        this.AdminPanel.FindControl("SupportTickets").Visible = false;
                    }
                    else
                    {
                        // Show PE/XE tools
                        this.AdvancedToolsPanel.Visible = true;
                    }

                    this.Localize();

                    if (!this.Page.IsPostBack)
                    {
                        UserInfo objUser = UserController.Instance.GetCurrentUserInfo();
                        if (objUser != null)
                        {
                            if (objUser.IsSuperUser)
                            {
                                this.hypMessage.ImageUrl = Upgrade.UpgradeIndicator(DotNetNukeContext.Current.Application.Version, this.Request.IsLocal, this.Request.IsSecureConnection);
                                if (!string.IsNullOrEmpty(this.hypMessage.ImageUrl))
                                {
                                    this.hypMessage.ToolTip = Localization.GetString("hypUpgrade.Text", this.LocalResourceFile);
                                    this.hypMessage.NavigateUrl = Upgrade.UpgradeRedirect();
                                }
                            }
                            else
                            {
                                if (PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName) && Host.DisplayCopyright)
                                {
                                    this.hypMessage.ImageUrl = "~/images/branding/iconbar_logo.png";
                                    this.hypMessage.ToolTip = DotNetNukeContext.Current.Application.Description;
                                    this.hypMessage.NavigateUrl = Localization.GetString("hypMessageUrl.Text", this.LocalResourceFile);
                                }
                                else
                                {
                                    this.hypMessage.Visible = false;
                                }

                                if (!TabPermissionController.CanAddContentToPage())
                                {
                                    this.CommonTasksPanel.Visible = false;
                                }
                            }

                            if (this.PortalSettings.AllowUserUICulture)
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

                                // Localization.LoadCultureDropDownList(ddlUICulture, CultureDropDownTypes.NativeName, currentCulture);
                                IEnumerable<ListItem> cultureListItems = Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, currentCulture, string.Empty, false);
                                foreach (var cultureItem in cultureListItems)
                                {
                                    this.ddlUICulture.AddItem(cultureItem.Text, cultureItem.Value);
                                }

                                var selectedCultureItem = this.ddlUICulture.FindItemByValue(currentCulture);
                                if (selectedCultureItem != null)
                                {
                                    selectedCultureItem.Selected = true;
                                }

                                // only show language selector if more than one language
                                if (this.ddlUICulture.Items.Count > 1)
                                {
                                    this.lblUILanguage.Visible = true;
                                    this.ddlUICulture.Visible = true;

                                    if (oCulture == null)
                                    {
                                        this.SetLanguage(true);
                                    }
                                }
                            }
                        }

                        this.SetMode(false);
                    }
                }
                else if (this.IsModuleAdmin())
                {
                    this.ControlPanel.Visible = true;
                    this.BodyPanel.Visible = false;
                    this.adminMenus.Visible = false;
                    if (!this.Page.IsPostBack)
                    {
                        this.SetMode(false);
                    }
                }
                else
                {
                    this.ControlPanel.Visible = false;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void ddlMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.Page.IsCallback)
            {
                return;
            }

            this.SetMode(true);
            this.Response.Redirect(this.Request.RawUrl, true);
        }

        protected string PreviewPopup()
        {
            var previewUrl = string.Format(
                "{0}/Default.aspx?ctl={1}&previewTab={2}&TabID={2}",
                Globals.AddHTTP(this.PortalSettings.PortalAlias.HTTPAlias),
                "MobilePreview",
                this.PortalSettings.ActiveTab.TabID);

            if (this.PortalSettings.EnablePopUps)
            {
                return UrlUtils.PopUpUrl(previewUrl, this, this.PortalSettings, true, false, 660, 800);
            }
            else
            {
                return string.Format("location.href = \"{0}\"", previewUrl);
            }
        }

        private void Localize()
        {
            Control ctrl = this.AdminPanel.FindControl("SiteNewPage");
            if ((ctrl != null) && ctrl is DnnRibbonBarTool)
            {
                var toolCtrl = (DnnRibbonBarTool)ctrl;
                toolCtrl.Text = Localization.GetString("SiteNewPage", this.LocalResourceFile);
                toolCtrl.ToolTip = Localization.GetString("SiteNewPage.ToolTip", this.LocalResourceFile);
            }
        }

        private void SetMode(bool update)
        {
            if (update)
            {
                this.SetUserMode(this.ddlMode.SelectedValue);
            }

            if (!TabPermissionController.CanAddContentToPage())
            {
                this.RemoveModeDropDownItem("LAYOUT");
            }

            if (!(new PreviewProfileController().GetProfilesByPortal(this.PortalSettings.PortalId).Count > 0))
            {
                this.RemoveModeDropDownItem("PREVIEW");
            }

            switch (this.UserMode)
            {
                case PortalSettings.Mode.View:
                    this.ddlMode.FindItemByValue("VIEW").Selected = true;
                    break;
                case PortalSettings.Mode.Edit:
                    this.ddlMode.FindItemByValue("EDIT").Selected = true;
                    break;
                case PortalSettings.Mode.Layout:
                    this.ddlMode.FindItemByValue("LAYOUT").Selected = true;
                    break;
            }
        }

        private void RemoveModeDropDownItem(string value)
        {
            var item = this.ddlMode.FindItemByValue(value);
            if (item != null)
            {
                this.ddlMode.Items.Remove(item);
            }
        }

        private void SetLanguage(bool update)
        {
            if (update)
            {
                DotNetNuke.Services.Personalization.Personalization.SetProfile("Usability", "UICulture", this.ddlUICulture.SelectedValue);
            }
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

        private void ddlUICulture_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.Page.IsCallback)
            {
                return;
            }

            this.SetLanguage(true);
            this.Response.Redirect(this.Request.RawUrl, true);
        }
    }
}
