// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control for a tool in a <see cref="DnnRibbonBar"/>.</summary>
    /// <param name="navigationManager">A navigation manager.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="eventLogger">The event logger.</param>
    [ParseChildren(true)]
    public class DnnRibbonBarTool(INavigationManager navigationManager, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IEventLogger eventLogger)
        : Control, IDnnRibbonBarTool
    {
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private IDictionary<string, RibbonBarToolInfo> allTools;
        private DnnTextLink dnnLink;
        private DnnTextButton dnnLinkButton;

        /// <summary>Initializes a new instance of the <see cref="DnnRibbonBarTool"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public DnnRibbonBarTool()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnRibbonBarTool"/> class.</summary>
        /// <param name="navigationManager">A navigation manager.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnRibbonBarTool(INavigationManager navigationManager)
            : this(navigationManager, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnRibbonBarTool"/> class.</summary>
        /// <param name="navigationManager">A navigation manager.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public DnnRibbonBarTool(INavigationManager navigationManager, IApplicationStatusInfo appStatus, IHostSettings hostSettings)
            : this(navigationManager, appStatus, hostSettings, null)
        {
        }

        /// <summary>Gets or sets the tool info.</summary>
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

        /// <summary>Gets or sets the URL.</summary>
        public virtual string NavigateUrl
        {
            get => Utilities.GetViewStateAsString(this.ViewState["NavigateUrl"], Null.NullString);
            set => this.ViewState["NavigateUrl"] = value;
        }

        /// <summary>Gets or sets the CSS class.</summary>
        public virtual string ToolCssClass
        {
            get => Utilities.GetViewStateAsString(this.ViewState["ToolCssClass"], Null.NullString);
            set => this.ViewState["ToolCssClass"] = value;
        }

        /// <summary>Gets or sets the text.</summary>
        public virtual string Text
        {
            get => Utilities.GetViewStateAsString(this.ViewState["Text"], Null.NullString);
            set => this.ViewState["Text"] = value;
        }

        /// <summary>Gets or sets the tooltip text.</summary>
        public virtual string ToolTip
        {
            get => Utilities.GetViewStateAsString(this.ViewState["ToolTip"], Null.NullString);
            set => this.ViewState["ToolTip"] = value;
        }

        /// <inheritdoc />
        public virtual string ToolName
        {
            get
            {
                return this.ToolInfo.ToolName;
            }

            set
            {
                if (this.AllTools.TryGetValue(value, out var toolInfo))
                {
                    this.ToolInfo = toolInfo;
                }
                else
                {
                    throw new NotSupportedException($"Tool not found [{value}]");
                }
            }
        }

        /// <summary>Gets the navigation manager.</summary>
        protected INavigationManager NavigationManager { get; } = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        /// <summary>Gets the link button.</summary>
        protected virtual DnnTextButton DnnLinkButton
        {
            get
            {
                if (this.dnnLinkButton == null)
                {
                    // Appending _CPCommandBtn is also assumed in the RibbonBar.ascx. If changed, one would need to change in both places.
                    this.dnnLinkButton = new DnnTextButton { ID = this.ID + "_CPCommandBtn", };
                }

                return this.dnnLinkButton;
            }
        }

        /// <summary>Gets the link.</summary>
        protected virtual DnnTextLink DnnLink => this.dnnLink ??= new DnnTextLink();

        /// <summary>Gets the tools.</summary>
        protected virtual IDictionary<string, RibbonBarToolInfo> AllTools =>
            this.allTools ??= new Dictionary<string, RibbonBarToolInfo>
            {
                // Framework
                {
                    "PageSettings",
                    new RibbonBarToolInfo("PageSettings", false, false, string.Empty, string.Empty, string.Empty, true)
                },
                { "CopyPage", new RibbonBarToolInfo("CopyPage", false, false, string.Empty, string.Empty, string.Empty, true) },
                { "DeletePage", new RibbonBarToolInfo("DeletePage", false, true, string.Empty, string.Empty, string.Empty, true) },
                { "ImportPage", new RibbonBarToolInfo("ImportPage", false, false, string.Empty, string.Empty, string.Empty, true) },
                { "ExportPage", new RibbonBarToolInfo("ExportPage", false, false, string.Empty, string.Empty, string.Empty, true) },
                { "NewPage", new RibbonBarToolInfo("NewPage", false, false, string.Empty, string.Empty, string.Empty, true) },
                {
                    "CopyPermissionsToChildren",
                    new RibbonBarToolInfo("CopyPermissionsToChildren", false, true, string.Empty, string.Empty, string.Empty, false)
                },
                {
                    "CopyDesignToChildren",
                    new RibbonBarToolInfo("CopyDesignToChildren", false, true, string.Empty, string.Empty, string.Empty, false)
                },
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

        /// <summary>Gets the current portal settings.</summary>
        private static PortalSettings PortalSettings => PortalSettings.Current;

        /// <summary>Handles a click event on the tool.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
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
                        TabController.CopyPermissionsToChildren(this.eventLogger, PortalSettings.ActiveTab, PortalSettings.ActiveTab.TabPermissions);
                        this.Page.Response.Redirect(this.Page.Request.RawUrl);
                    }

                    break;
                case "CopyDesignToChildren":
                    if (this.HasToolPermissions("CopyDesignToChildren"))
                    {
                        TabController.CopyDesignToChildren(this.eventLogger, PortalSettings.ActiveTab, PortalSettings.ActiveTab.SkinSrc, PortalSettings.ActiveTab.ContainerSrc);
                        this.Page.Response.Redirect(this.Page.Request.RawUrl);
                    }

                    break;
                case "ClearCache":
                    if (this.HasToolPermissions("ClearCache"))
                    {
                        this.ClearCache();
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

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            this.Controls.Add(this.DnnLinkButton);
            this.Controls.Add(this.DnnLink);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            this.EnsureChildControls();
            this.DnnLinkButton.Click += this.ControlPanelTool_OnClick;
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            this.ProcessTool();
            this.Visible = this.DnnLink.Visible || this.DnnLinkButton.Visible;
            base.OnPreRender(e);
        }

        /// <summary>Processes the tool.</summary>
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
                                if (!(PortalSettings.SSLSetup != Abstractions.Security.SiteSslSetup.Off && PortalSettings.SSLEnforced))
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

        /// <summary>Enable the tool.</summary>
        /// <returns>Whether it was successfully enabled.</returns>
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

        /// <summary>Whether the current user has the required permissions for the tool.</summary>
        /// <param name="toolName">The tool name.</param>
        /// <returns>Whether the user can access the tool.</returns>
        protected virtual bool HasToolPermissions(string toolName)
        {
            bool isHostTool = false;
            if (this.ToolInfo.ToolName == toolName)
            {
                isHostTool = this.ToolInfo.IsHostTool;
            }
            else if (this.AllTools.TryGetValue(toolName, out var tool))
            {
                isHostTool = tool.IsHostTool;
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
                    returnValue = !string.IsNullOrEmpty(this.hostSettings.HelpUrl);
                    break;
                default:
                    // if it has a module definition, look it up and check permissions
                    // if it doesn't exist, assume no permission
                    string friendlyName = string.Empty;
                    if (this.ToolInfo.ToolName == toolName)
                    {
                        friendlyName = this.ToolInfo.ModuleFriendlyName;
                    }
                    else if (this.AllTools.TryGetValue(toolName, out var toolInfo))
                    {
                        friendlyName = toolInfo.ModuleFriendlyName;
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

        /// <summary>Build the tool URL.</summary>
        /// <returns>The URL.</returns>
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
                    if (!string.IsNullOrEmpty(this.hostSettings.HelpUrl))
                    {
                        var version = Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, false);
                        returnValue = TestableGlobals.Instance.FormatHelpUrl(this.hostSettings.HelpUrl, PortalSettings, "Home", version);
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

        /// <summary>Gets the tool text.</summary>
        /// <returns>The text.</returns>
        protected virtual string GetText()
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                return this.GetString($"Tool.{this.ToolInfo.ToolName}.Text");
            }

            return this.Text;
        }

        /// <summary>Gets the tooltip for the tool.</summary>
        /// <returns>The tooltip text.</returns>
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
                string tip = this.GetString($"Tool.{this.ToolInfo.ToolName}.ToolTip");
                if (string.IsNullOrEmpty(tip))
                {
                    tip = this.GetString($"Tool.{this.ToolInfo.ToolName}.Text");
                }

                return tip;
            }

            return this.ToolTip;
        }

        /// <summary>Gets the tab URL.</summary>
        /// <param name="additionalParams">Additional parameters.</param>
        /// <returns>The URL.</returns>
        protected virtual string GetTabURL(List<string> additionalParams)
        {
            int portalId = this.ToolInfo.IsHostTool ? Null.NullInteger : PortalSettings.PortalId;

            additionalParams ??= [];

            var moduleInfo = ModuleController.Instance.GetModuleByDefinition(portalId, this.ToolInfo.ModuleFriendlyName);

            if (moduleInfo == null)
            {
                return string.Empty;
            }

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

            return this.NavigationManager.NavigateURL(moduleInfo.TabID, isHostPage, PortalSettings, this.ToolInfo.ControlKey, currentCulture, additionalParams.ToArray());
        }

        /// <summary>Gets a value indicating whether the current page has children.</summary>
        /// <returns>Whether the page has children.</returns>
        protected virtual bool ActiveTabHasChildren()
        {
            var children = TabController.GetTabsByParent(PortalSettings.ActiveTab.TabID, PortalSettings.ActiveTab.PortalID);

            if ((children == null) || children.Count < 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>Gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <returns>The localized text.</returns>
        protected virtual string GetString(string key)
        {
            return Utilities.GetLocalizedStringFromParent(key, this);
        }

        /// <summary>Clears the cache.</summary>
        protected virtual void ClearCache()
        {
            DataCache.ClearCache();
        }

        /// <summary>Restarts the application.</summary>
        protected virtual void RestartApplication()
        {
            var log = new LogInfo { BypassBuffering = true, LogTypeKey = nameof(EventLogType.HOST_ALERT), };
            log.AddProperty("Message", this.GetString("UserRestart"));
            LogController.Instance.AddLog(log);
            Config.Touch(this.appStatus);
        }

        private static ModuleInfo GetInstalledModule(int portalId, string friendlyName)
        {
            return ModuleController.Instance.GetModuleByDefinition(portalId, friendlyName);
        }
    }
}
