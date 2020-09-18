// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;

    [ParseChildren(true)]
    public class DnnRibbonBarTool : Control, IDnnRibbonBarTool
    {
        private IDictionary<string, RibbonBarToolInfo> _allTools;
        private DnnTextLink _dnnLink;
        private DnnTextButton _dnnLinkButton;

        public DnnRibbonBarTool()
        {
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public virtual RibbonBarToolInfo ToolInfo
        {
            get
            {
                if (this.ViewState["ToolInfo"] == null)
                {
                    this.ViewState.Add("ToolInfo", new RibbonBarToolInfo());
                }

                return (RibbonBarToolInfo)this.ViewState["ToolInfo"];
            }

            set
            {
                this.ViewState["ToolInfo"] = value;
            }
        }

        public virtual string NavigateUrl
        {
            get
            {
                return Utilities.GetViewStateAsString(this.ViewState["NavigateUrl"], Null.NullString);
            }

            set
            {
                this.ViewState["NavigateUrl"] = value;
            }
        }

        public virtual string ToolCssClass
        {
            get
            {
                return Utilities.GetViewStateAsString(this.ViewState["ToolCssClass"], Null.NullString);
            }

            set
            {
                this.ViewState["ToolCssClass"] = value;
            }
        }

        public virtual string Text
        {
            get
            {
                return Utilities.GetViewStateAsString(this.ViewState["Text"], Null.NullString);
            }

            set
            {
                this.ViewState["Text"] = value;
            }
        }

        public virtual string ToolTip
        {
            get
            {
                return Utilities.GetViewStateAsString(this.ViewState["ToolTip"], Null.NullString);
            }

            set
            {
                this.ViewState["ToolTip"] = value;
            }
        }

        public virtual string ToolName
        {
            get
            {
                return this.ToolInfo.ToolName;
            }

            set
            {
                if (this.AllTools.ContainsKey(value))
                {
                    this.ToolInfo = this.AllTools[value];
                }
                else
                {
                    throw new NotSupportedException("Tool not found [" + value + "]");
                }
            }
        }

        protected INavigationManager NavigationManager { get; }

        protected virtual DnnTextButton DnnLinkButton
        {
            get
            {
                if (this._dnnLinkButton == null)
                {
                    // Appending _CPCommandBtn is also assumed in the RibbonBar.ascx. If changed, one would need to change in both places.
                    this._dnnLinkButton = new DnnTextButton { ID = this.ID + "_CPCommandBtn" };
                }

                return this._dnnLinkButton;
            }
        }

        protected virtual DnnTextLink DnnLink
        {
            get
            {
                if (this._dnnLink == null)
                {
                    this._dnnLink = new DnnTextLink();
                }

                return this._dnnLink;
            }
        }

        protected virtual IDictionary<string, RibbonBarToolInfo> AllTools
        {
            get
            {
                if (this._allTools == null)
                {
                    this._allTools = new Dictionary<string, RibbonBarToolInfo>
                                    {
                                        // Framework
                                        { "PageSettings", new RibbonBarToolInfo("PageSettings", false, false, string.Empty, string.Empty, string.Empty, true) },
                                        { "CopyPage", new RibbonBarToolInfo("CopyPage", false, false, string.Empty, string.Empty, string.Empty, true) },
                                        { "DeletePage", new RibbonBarToolInfo("DeletePage", false, true, string.Empty, string.Empty, string.Empty, true) },
                                        { "ImportPage", new RibbonBarToolInfo("ImportPage", false, false, string.Empty, string.Empty, string.Empty, true) },
                                        { "ExportPage", new RibbonBarToolInfo("ExportPage", false, false, string.Empty, string.Empty, string.Empty, true) },
                                        { "NewPage", new RibbonBarToolInfo("NewPage", false, false, string.Empty, string.Empty, string.Empty, true) },
                                        { "CopyPermissionsToChildren", new RibbonBarToolInfo("CopyPermissionsToChildren", false, true, string.Empty, string.Empty, string.Empty, false) },
                                        { "CopyDesignToChildren", new RibbonBarToolInfo("CopyDesignToChildren", false, true, string.Empty, string.Empty, string.Empty, false) },
                                        { "Help", new RibbonBarToolInfo("Help", false, false, "_Blank", string.Empty, string.Empty, false) },

                                        // Modules On Tabs
                                        { "Console", new RibbonBarToolInfo("Console", false, false, string.Empty, "Console", string.Empty, false) },
                                        { "HostConsole", new RibbonBarToolInfo("HostConsole", true, false, string.Empty, "Console", string.Empty, false) },
                                        { "UploadFile", new RibbonBarToolInfo("UploadFile", false, false, string.Empty, string.Empty, "WebUpload", true) },
                                        { "NewRole", new RibbonBarToolInfo("NewRole", false, false, string.Empty, "Security Roles", "Edit", true) },
                                        { "NewUser", new RibbonBarToolInfo("NewUser", false, false, string.Empty, "User Accounts", "Edit", true) },
                                        { "ClearCache", new RibbonBarToolInfo("ClearCache", true, true, string.Empty, string.Empty, string.Empty, false) },
                                        { "RecycleApp", new RibbonBarToolInfo("RecycleApp", true, true, string.Empty, string.Empty, string.Empty, false) },
                                    };
                }

                return this._allTools;
            }
        }

        private static PortalSettings PortalSettings
        {
            get
            {
                return PortalSettings.Current;
            }
        }

        public virtual void ControlPanelTool_OnClick(object sender, EventArgs e)
        {
            switch (this.ToolInfo.ToolName)
            {
                case "DeletePage":
                    if (this.HasToolPermissions("DeletePage"))
                    {
                        string url = TestableGlobals.Instance.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=delete");
                        this.Page.Response.Redirect(url, true);
                    }

                    break;
                case "CopyPermissionsToChildren":
                    if (this.HasToolPermissions("CopyPermissionsToChildren"))
                    {
                        TabController.CopyPermissionsToChildren(PortalSettings.ActiveTab, PortalSettings.ActiveTab.TabPermissions);
                        this.Page.Response.Redirect(this.Page.Request.RawUrl);
                    }

                    break;
                case "CopyDesignToChildren":
                    if (this.HasToolPermissions("CopyDesignToChildren"))
                    {
                        TabController.CopyDesignToChildren(PortalSettings.ActiveTab, PortalSettings.ActiveTab.SkinSrc, PortalSettings.ActiveTab.ContainerSrc);
                        this.Page.Response.Redirect(this.Page.Request.RawUrl);
                    }

                    break;
                case "ClearCache":
                    if (this.HasToolPermissions("ClearCache"))
                    {
                        this.ClearCache();
                        ClientResourceManager.ClearCache();
                        this.Page.Response.Redirect(this.Page.Request.RawUrl);
                    }

                    break;
                case "RecycleApp":
                    if (this.HasToolPermissions("RecycleApp"))
                    {
                        this.RestartApplication();
                        this.Page.Response.Redirect(this.Page.Request.RawUrl);
                    }

                    break;
            }
        }

        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            this.Controls.Add(this.DnnLinkButton);
            this.Controls.Add(this.DnnLink);
        }

        protected override void OnInit(EventArgs e)
        {
            this.EnsureChildControls();
            this.DnnLinkButton.Click += this.ControlPanelTool_OnClick;
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.ProcessTool();
            this.Visible = this.DnnLink.Visible || this.DnnLinkButton.Visible;
            base.OnPreRender(e);
        }

        protected virtual void ProcessTool()
        {
            this.DnnLink.Visible = false;
            this.DnnLinkButton.Visible = false;

            if (!string.IsNullOrEmpty(this.ToolInfo.ToolName))
            {
                if (this.ToolInfo.UseButton)
                {
                    this.DnnLinkButton.Visible = this.HasToolPermissions(this.ToolInfo.ToolName);
                    this.DnnLinkButton.Enabled = this.EnableTool();
                    this.DnnLinkButton.Localize = false;

                    this.DnnLinkButton.CssClass = this.ToolCssClass;
                    this.DnnLinkButton.DisabledCssClass = this.ToolCssClass + " dnnDisabled";

                    this.DnnLinkButton.Text = this.GetText();
                    this.DnnLinkButton.ToolTip = this.GetToolTip();
                }
                else
                {
                    this.DnnLink.Visible = this.HasToolPermissions(this.ToolInfo.ToolName);
                    this.DnnLink.Enabled = this.EnableTool();
                    this.DnnLink.Localize = false;

                    if (this.DnnLink.Enabled)
                    {
                        this.DnnLink.NavigateUrl = this.BuildToolUrl();

                        // can't find the page, disable it?
                        if (string.IsNullOrEmpty(this.DnnLink.NavigateUrl))
                        {
                            this.DnnLink.Enabled = false;
                        }

                        // create popup event
                        else if (this.ToolInfo.ShowAsPopUp && PortalSettings.EnablePopUps)
                        {
                            // Prevent PageSettings in a popup if SSL is enabled and enforced, which causes redirection/javascript broswer security issues.
                            if (this.ToolInfo.ToolName == "PageSettings" || this.ToolInfo.ToolName == "CopyPage" || this.ToolInfo.ToolName == "NewPage")
                            {
                                if (!(PortalSettings.SSLEnabled && PortalSettings.SSLEnforced))
                                {
                                    this.DnnLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(this.DnnLink.NavigateUrl, this, PortalSettings, true, false));
                                }
                            }
                            else
                            {
                                this.DnnLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(this.DnnLink.NavigateUrl, this, PortalSettings, true, false));
                            }
                        }
                    }

                    this.DnnLink.CssClass = this.ToolCssClass;
                    this.DnnLink.DisabledCssClass = this.ToolCssClass + " dnnDisabled";

                    this.DnnLink.Text = this.GetText();
                    this.DnnLink.ToolTip = this.GetToolTip();
                    this.DnnLink.Target = this.ToolInfo.LinkWindowTarget;
                }
            }
        }

        protected virtual bool EnableTool()
        {
            bool returnValue = true;

            switch (this.ToolInfo.ToolName)
            {
                case "DeletePage":
                    if (TabController.IsSpecialTab(TabController.CurrentPage.TabID, PortalSettings.PortalId))
                    {
                        returnValue = false;
                    }

                    break;
                case "CopyDesignToChildren":
                case "CopyPermissionsToChildren":
                    returnValue = this.ActiveTabHasChildren();
                    if (returnValue && this.ToolInfo.ToolName == "CopyPermissionsToChildren")
                    {
                        if (PortalSettings.ActiveTab.IsSuperTab)
                        {
                            returnValue = false;
                        }
                    }

                    break;
            }

            return returnValue;
        }

        protected virtual bool HasToolPermissions(string toolName)
        {
            bool isHostTool = false;
            if (this.ToolInfo.ToolName == toolName)
            {
                isHostTool = this.ToolInfo.IsHostTool;
            }
            else if (this.AllTools.ContainsKey(toolName))
            {
                isHostTool = this.AllTools[toolName].IsHostTool;
            }

            if (isHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return false;
            }

            bool returnValue = true;
            switch (toolName)
            {
                case "PageSettings":
                case "CopyDesignToChildren":
                case "CopyPermissionsToChildren":
                    returnValue = TabPermissionController.CanManagePage();

                    if (returnValue && toolName == "CopyPermissionsToChildren")
                    {
                        if (!PortalSecurity.IsInRole("Administrators"))
                        {
                            returnValue = false;
                        }
                    }

                    break;
                case "CopyPage":
                    returnValue = TabPermissionController.CanCopyPage();
                    break;
                case "DeletePage":
                    returnValue = TabPermissionController.CanDeletePage();
                    break;
                case "ImportPage":
                    returnValue = TabPermissionController.CanImportPage();
                    break;
                case "ExportPage":
                    returnValue = TabPermissionController.CanExportPage();
                    break;
                case "NewPage":
                    returnValue = TabPermissionController.CanAddPage();
                    break;
                case "Help":
                    returnValue = !string.IsNullOrEmpty(Host.HelpURL);
                    break;
                default:
                    // if it has a module definition, look it up and check permissions
                    // if it doesn't exist, assume no permission
                    string friendlyName = string.Empty;
                    if (this.ToolInfo.ToolName == toolName)
                    {
                        friendlyName = this.ToolInfo.ModuleFriendlyName;
                    }
                    else if (this.AllTools.ContainsKey(toolName))
                    {
                        friendlyName = this.AllTools[toolName].ModuleFriendlyName;
                    }

                    if (!string.IsNullOrEmpty(friendlyName))
                    {
                        returnValue = false;
                        ModuleInfo moduleInfo;

                        if (isHostTool)
                        {
                            moduleInfo = GetInstalledModule(Null.NullInteger, friendlyName);
                        }
                        else
                        {
                            moduleInfo = GetInstalledModule(PortalSettings.PortalId, friendlyName);
                        }

                        if (moduleInfo != null)
                        {
                            returnValue = ModulePermissionController.CanViewModule(moduleInfo);
                        }
                    }

                    break;
            }

            return returnValue;
        }

        protected virtual string BuildToolUrl()
        {
            if (this.ToolInfo.IsHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return "javascript:void(0);";
            }

            if (!string.IsNullOrEmpty(this.NavigateUrl))
            {
                return this.NavigateUrl;
            }

            string returnValue = "javascript:void(0);";
            switch (this.ToolInfo.ToolName)
            {
                case "PageSettings":
                    returnValue = TestableGlobals.Instance.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=edit");
                    break;

                case "CopyPage":
                    returnValue = TestableGlobals.Instance.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=copy");
                    break;

                case "DeletePage":
                    returnValue = TestableGlobals.Instance.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=delete");
                    break;

                case "ImportPage":
                    returnValue = TestableGlobals.Instance.NavigateURL(PortalSettings.ActiveTab.TabID, "ImportTab");
                    break;

                case "ExportPage":
                    returnValue = this.NavigationManager.NavigateURL(PortalSettings.ActiveTab.TabID, "ExportTab");
                    break;

                case "NewPage":
                    returnValue = TestableGlobals.Instance.NavigateURL("Tab");
                    break;
                case "Help":
                    if (!string.IsNullOrEmpty(Host.HelpURL))
                    {
                        var version = Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, false);
                        returnValue = TestableGlobals.Instance.FormatHelpUrl(Host.HelpURL, PortalSettings, "Home", version);
                    }

                    break;
                case "UploadFile":
                case "HostUploadFile":
                    returnValue = TestableGlobals.Instance.NavigateURL(PortalSettings.ActiveTab.TabID, "WebUpload");
                    break;
                default:
                    if (!string.IsNullOrEmpty(this.ToolInfo.ModuleFriendlyName))
                    {
                        var additionalParams = new List<string>();
                        returnValue = this.GetTabURL(additionalParams);
                    }

                    break;
            }

            return returnValue;
        }

        protected virtual string GetText()
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                return this.GetString(string.Format("Tool.{0}.Text", this.ToolInfo.ToolName));
            }

            return this.Text;
        }

        protected virtual string GetToolTip()
        {
            if (this.ToolInfo.ToolName == "DeletePage")
            {
                if (TabController.IsSpecialTab(TabController.CurrentPage.TabID, PortalSettings.PortalId))
                {
                    return this.GetString("Tool.DeletePage.Special.ToolTip");
                }
            }

            if (string.IsNullOrEmpty(this.Text))
            {
                string tip = this.GetString(string.Format("Tool.{0}.ToolTip", this.ToolInfo.ToolName));
                if (string.IsNullOrEmpty(tip))
                {
                    tip = this.GetString(string.Format("Tool.{0}.Text", this.ToolInfo.ToolName));
                }

                return tip;
            }

            return this.ToolTip;
        }

        protected virtual string GetTabURL(List<string> additionalParams)
        {
            int portalId = this.ToolInfo.IsHostTool ? Null.NullInteger : PortalSettings.PortalId;

            string strURL = string.Empty;

            if (additionalParams == null)
            {
                additionalParams = new List<string>();
            }

            var moduleInfo = ModuleController.Instance.GetModuleByDefinition(portalId, this.ToolInfo.ModuleFriendlyName);

            if (moduleInfo != null)
            {
                bool isHostPage = portalId == Null.NullInteger;
                if (!string.IsNullOrEmpty(this.ToolInfo.ControlKey))
                {
                    additionalParams.Insert(0, "mid=" + moduleInfo.ModuleID);
                    if (this.ToolInfo.ShowAsPopUp && PortalSettings.EnablePopUps)
                    {
                        additionalParams.Add("popUp=true");
                    }
                }

                string currentCulture = Thread.CurrentThread.CurrentCulture.Name;
                strURL = this.NavigationManager.NavigateURL(moduleInfo.TabID, isHostPage, PortalSettings, this.ToolInfo.ControlKey, currentCulture, additionalParams.ToArray());
            }

            return strURL;
        }

        protected virtual bool ActiveTabHasChildren()
        {
            var children = TabController.GetTabsByParent(PortalSettings.ActiveTab.TabID, PortalSettings.ActiveTab.PortalID);

            if ((children == null) || children.Count < 1)
            {
                return false;
            }

            return true;
        }

        protected virtual string GetString(string key)
        {
            return Utilities.GetLocalizedStringFromParent(key, this);
        }

        protected virtual void ClearCache()
        {
            DataCache.ClearCache();
        }

        protected virtual void RestartApplication()
        {
            var log = new LogInfo { BypassBuffering = true, LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
            log.AddProperty("Message", this.GetString("UserRestart"));
            LogController.Instance.AddLog(log);
            Config.Touch();
        }

        private static ModuleInfo GetInstalledModule(int portalID, string friendlyName)
        {
            return ModuleController.Instance.GetModuleByDefinition(portalID, friendlyName);
        }
    }
}
