#region Copyright
// 
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
#region Usings

using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;

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

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    [ParseChildren(true)]
    public class DnnRibbonBarTool : Control, IDnnRibbonBarTool
    {
        #region Properties

        private IDictionary<string, RibbonBarToolInfo> _allTools;
        private DnnTextLink _dnnLink;
        private DnnTextButton _dnnLinkButton;

        public virtual RibbonBarToolInfo ToolInfo
        {
            get
            {
                if ((ViewState["ToolInfo"] == null))
                {
                    ViewState.Add("ToolInfo", new RibbonBarToolInfo());
                }
                return (RibbonBarToolInfo) ViewState["ToolInfo"];
            }
            set
            {
                ViewState["ToolInfo"] = value;
            }
        }

        public virtual string NavigateUrl
        {
            get
            {
                return Utilities.GetViewStateAsString(ViewState["NavigateUrl"], Null.NullString);
            }
            set
            {
                ViewState["NavigateUrl"] = value;
            }
        }

        public virtual string ToolCssClass
        {
            get
            {
                return Utilities.GetViewStateAsString(ViewState["ToolCssClass"], Null.NullString);
            }
            set
            {
                ViewState["ToolCssClass"] = value;
            }
        }

        public virtual string Text
        {
            get
            {
                return Utilities.GetViewStateAsString(ViewState["Text"], Null.NullString);
            }
            set
            {
                ViewState["Text"] = value;
            }
        }

        public virtual string ToolTip
        {
            get
            {
                return Utilities.GetViewStateAsString(ViewState["ToolTip"], Null.NullString);
            }
            set
            {
                ViewState["ToolTip"] = value;
            }
        }

        protected virtual DnnTextButton DnnLinkButton
        {
            get
            {
                if ((_dnnLinkButton == null))
                {
                    // Appending _CPCommandBtn is also assumed in the RibbonBar.ascx. If changed, one would need to change in both places.
                    _dnnLinkButton = new DnnTextButton {ID = ID + "_CPCommandBtn"};
                }
                return _dnnLinkButton;
            }
        }

        protected virtual DnnTextLink DnnLink
        {
            get
            {
                if ((_dnnLink == null))
                {
                    _dnnLink = new DnnTextLink();
                }
                return _dnnLink;
            }
        }

        protected virtual IDictionary<string, RibbonBarToolInfo> AllTools
        {
            get
            {
                if (_allTools == null)
                {
                    _allTools = new Dictionary<string, RibbonBarToolInfo>
                                    {
										//Framework
                                        {"PageSettings", new RibbonBarToolInfo("PageSettings", false, false, "", "", "", true)},
                                        {"CopyPage", new RibbonBarToolInfo("CopyPage", false, false, "", "", "", true)},
                                        {"DeletePage", new RibbonBarToolInfo("DeletePage", false, true, "", "", "", true)},
                                        {"ImportPage", new RibbonBarToolInfo("ImportPage", false, false, "", "", "", true)},
                                        {"ExportPage", new RibbonBarToolInfo("ExportPage", false, false, "", "", "", true)},
                                        {"NewPage", new RibbonBarToolInfo("NewPage", false, false, "", "", "", true)},
                                        {"CopyPermissionsToChildren", new RibbonBarToolInfo("CopyPermissionsToChildren", false, true, "", "", "", false)},
                                        {"CopyDesignToChildren", new RibbonBarToolInfo("CopyDesignToChildren", false, true, "", "", "", false)},
                                        {"Help", new RibbonBarToolInfo("Help", false, false, "_Blank", "", "", false)},
										//Modules On Tabs
                                        {"Console", new RibbonBarToolInfo("Console", false, false, "", "Console", "", false)},
                                        {"HostConsole", new RibbonBarToolInfo("HostConsole", true, false, "", "Console", "", false)},
                                        {"UploadFile", new RibbonBarToolInfo("UploadFile", false, false, "", "", "WebUpload", true)},
                                        {"NewRole", new RibbonBarToolInfo("NewRole", false, false, "", "Security Roles", "Edit", true)},
                                        {"NewUser", new RibbonBarToolInfo("NewUser", false, false, "", "User Accounts", "Edit", true)},
                                        {"ClearCache", new RibbonBarToolInfo("ClearCache", true, true, "", "", "", false)},
                                        {"RecycleApp", new RibbonBarToolInfo("RecycleApp", true, true, "", "", "", false)}
                                    };
                }

                return _allTools;
            }
        }

        private static PortalSettings PortalSettings
        {
            get
            {
                return PortalSettings.Current;
            }
        }

        public virtual string ToolName
        {
            get
            {
                return ToolInfo.ToolName;
            }
            set
            {
                if ((AllTools.ContainsKey(value)))
                {
                    ToolInfo = AllTools[value];
                }
                else
                {
                    throw new NotSupportedException("Tool not found [" + value + "]");
                }
            }
        }

        #endregion

        #region Events

        protected override void CreateChildControls()
        {
            Controls.Clear();
            Controls.Add(DnnLinkButton);
            Controls.Add(DnnLink);
        }

        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
            DnnLinkButton.Click += ControlPanelTool_OnClick;
        }

        protected override void OnPreRender(EventArgs e)
        {
            ProcessTool();
            Visible = (DnnLink.Visible || DnnLinkButton.Visible);
            base.OnPreRender(e);
        }

        public virtual void ControlPanelTool_OnClick(object sender, EventArgs e)
        {
            switch (ToolInfo.ToolName)
            {
                case "DeletePage":
                    if ((HasToolPermissions("DeletePage")))
                    {
                        string url = TestableGlobals.Instance.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=delete");
                        Page.Response.Redirect(url, true);                        
                    }
                    break;
                case "CopyPermissionsToChildren":
                    if ((HasToolPermissions("CopyPermissionsToChildren")))
                    {
                        TabController.CopyPermissionsToChildren(PortalSettings.ActiveTab, PortalSettings.ActiveTab.TabPermissions);
                        Page.Response.Redirect(Page.Request.RawUrl);
                    }
                    break;
                case "CopyDesignToChildren":
                    if ((HasToolPermissions("CopyDesignToChildren")))
                    {
                        TabController.CopyDesignToChildren(PortalSettings.ActiveTab, PortalSettings.ActiveTab.SkinSrc, PortalSettings.ActiveTab.ContainerSrc);
                        Page.Response.Redirect(Page.Request.RawUrl);
                    }
                    break;
                case "ClearCache":
                    if ((HasToolPermissions("ClearCache")))
                    {
                        ClearCache();
						ClientResourceManager.ClearCache();
                        Page.Response.Redirect(Page.Request.RawUrl);
                    }
                    break;
                case "RecycleApp":
                    if ((HasToolPermissions("RecycleApp")))
                    {
                        RestartApplication();
                        Page.Response.Redirect(Page.Request.RawUrl);
                    }
                    break;
            }
        }

        #endregion

        #region Methods

        protected virtual void ProcessTool()
        {
            DnnLink.Visible = false;
            DnnLinkButton.Visible = false;

            if ((!string.IsNullOrEmpty(ToolInfo.ToolName)))
            {
                if ((ToolInfo.UseButton))
                {
                    DnnLinkButton.Visible = HasToolPermissions(ToolInfo.ToolName);
                    DnnLinkButton.Enabled = EnableTool();
                    DnnLinkButton.Localize = false;

                    DnnLinkButton.CssClass = ToolCssClass;
                    DnnLinkButton.DisabledCssClass = ToolCssClass + " dnnDisabled";

                    DnnLinkButton.Text = GetText();
                    DnnLinkButton.ToolTip = GetToolTip();
                }
                else
                {
                    DnnLink.Visible = HasToolPermissions(ToolInfo.ToolName);
                    DnnLink.Enabled = EnableTool();
                    DnnLink.Localize = false;

                    if ((DnnLink.Enabled))
                    {
                        DnnLink.NavigateUrl = BuildToolUrl();

                        //can't find the page, disable it?
                        if ((string.IsNullOrEmpty(DnnLink.NavigateUrl)))
                        {
                            DnnLink.Enabled = false;
                        }
                        //create popup event 
                        else if (ToolInfo.ShowAsPopUp && PortalSettings.EnablePopUps)
                        {
                            // Prevent PageSettings in a popup if SSL is enabled and enforced, which causes redirection/javascript broswer security issues.
                            if (ToolInfo.ToolName == "PageSettings" || ToolInfo.ToolName == "CopyPage" || ToolInfo.ToolName == "NewPage")
                            {
                                if (!(PortalSettings.SSLEnabled && PortalSettings.SSLEnforced))
                                {
                                    DnnLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(DnnLink.NavigateUrl, this, PortalSettings, true, false));
                                }
                            }
                            else
                            {
                                DnnLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(DnnLink.NavigateUrl, this, PortalSettings, true, false));
                            }
                        }
                    }

                    DnnLink.CssClass = ToolCssClass;
                    DnnLink.DisabledCssClass = ToolCssClass + " dnnDisabled";

                    DnnLink.Text = GetText();
                    DnnLink.ToolTip = GetToolTip();
                    DnnLink.Target = ToolInfo.LinkWindowTarget;
                }
            }
        }

        protected virtual bool EnableTool()
        {
            bool returnValue = true;

            switch (ToolInfo.ToolName)
            {
                case "DeletePage":
                    if ((TabController.IsSpecialTab(TabController.CurrentPage.TabID, PortalSettings.PortalId)))
                    {
                        returnValue = false;
                    }
                    break;
                case "CopyDesignToChildren":
                case "CopyPermissionsToChildren":
                    returnValue = ActiveTabHasChildren();
                    if ((returnValue && ToolInfo.ToolName == "CopyPermissionsToChildren"))
                    {
                        if ((PortalSettings.ActiveTab.IsSuperTab))
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
            if ((ToolInfo.ToolName == toolName))
            {
                isHostTool = ToolInfo.IsHostTool;
            }
            else if ((AllTools.ContainsKey(toolName)))
            {
                isHostTool = AllTools[toolName].IsHostTool;
            }

            if ((isHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser))
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

                    if ((returnValue && toolName == "CopyPermissionsToChildren"))
                    {
                        if ((!PortalSecurity.IsInRole("Administrators")))
                        {
                            returnValue = false;
                        }
                    }
                    break;
                case "CopyPage":
                    returnValue = TabPermissionController.CanCopyPage();
                    break;
                case "DeletePage":
                    returnValue = (TabPermissionController.CanDeletePage());
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
                    //if it has a module definition, look it up and check permissions
                    //if it doesn't exist, assume no permission
                    string friendlyName = "";
                    if ((ToolInfo.ToolName == toolName))
                    {
                        friendlyName = ToolInfo.ModuleFriendlyName;
                    }
                    else if ((AllTools.ContainsKey(toolName)))
                    {
                        friendlyName = AllTools[toolName].ModuleFriendlyName;
                    }

                    if ((!string.IsNullOrEmpty(friendlyName)))
                    {
                        returnValue = false;
                        ModuleInfo moduleInfo;

                        if ((isHostTool))
                        {
                            moduleInfo = GetInstalledModule(Null.NullInteger, friendlyName);
                        }
                        else
                        {
                            moduleInfo = GetInstalledModule(PortalSettings.PortalId, friendlyName);
                        }

                        if ((moduleInfo != null))
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
            if ((ToolInfo.IsHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser))
            {
                return "javascript:void(0);";
            }

            if ((!string.IsNullOrEmpty(NavigateUrl)))
            {
                return NavigateUrl;
            }

            string returnValue = "javascript:void(0);";
            switch (ToolInfo.ToolName)
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
                    returnValue = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "ExportTab");
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
                    if ((!string.IsNullOrEmpty(ToolInfo.ModuleFriendlyName)))
                    {
                        var additionalParams = new List<string>();
                        returnValue = GetTabURL(additionalParams);
                    }
                    break;
            }
            return returnValue;
        }

        protected virtual string GetText()
        {
            if ((string.IsNullOrEmpty(Text)))
            {
                return GetString(string.Format("Tool.{0}.Text", ToolInfo.ToolName));
            }

            return Text;
        }

        protected virtual string GetToolTip()
        {
            if ((ToolInfo.ToolName == "DeletePage"))
            {
                if ((TabController.IsSpecialTab(TabController.CurrentPage.TabID, PortalSettings.PortalId)))
                {
                    return GetString("Tool.DeletePage.Special.ToolTip");
                }
            }

            if ((string.IsNullOrEmpty(Text)))
            {
                string tip = GetString(string.Format("Tool.{0}.ToolTip", ToolInfo.ToolName));
                if ((string.IsNullOrEmpty(tip)))
                {
                    tip = GetString(string.Format("Tool.{0}.Text", ToolInfo.ToolName));
                }
                return tip;
            }

            return ToolTip;
        }

        protected virtual string GetTabURL(List<string> additionalParams)
        {
            int portalId = (ToolInfo.IsHostTool) ? Null.NullInteger : PortalSettings.PortalId;

            string strURL = string.Empty;

            if (((additionalParams == null)))
            {
                additionalParams = new List<string>();
            }

            var moduleInfo = ModuleController.Instance.GetModuleByDefinition(portalId, ToolInfo.ModuleFriendlyName);

            if (((moduleInfo != null)))
            {
                bool isHostPage = (portalId == Null.NullInteger);
                if ((!string.IsNullOrEmpty(ToolInfo.ControlKey)))
                {
                    additionalParams.Insert(0, "mid=" + moduleInfo.ModuleID);
                    if (ToolInfo.ShowAsPopUp && PortalSettings.EnablePopUps)
                    {
                        additionalParams.Add("popUp=true");
                    }
                }

                string currentCulture = Thread.CurrentThread.CurrentCulture.Name;
                strURL = Globals.NavigateURL(moduleInfo.TabID, isHostPage, PortalSettings, ToolInfo.ControlKey, currentCulture, additionalParams.ToArray());
            }

            return strURL;
        }

        protected virtual bool ActiveTabHasChildren()
        {
            var children = TabController.GetTabsByParent(PortalSettings.ActiveTab.TabID, PortalSettings.ActiveTab.PortalID);

            if (((children == null) || children.Count < 1))
            {
                return false;
            }

            return true;
        }
        
        protected virtual string GetString(string key)
        {
            return Utilities.GetLocalizedStringFromParent(key, this);
        }

        private static ModuleInfo GetInstalledModule(int portalID, string friendlyName)
        {
            return ModuleController.Instance.GetModuleByDefinition(portalID, friendlyName);
        }

        protected virtual void ClearCache()
        {
            DataCache.ClearCache();
        }

        protected virtual void RestartApplication()
        {
            var log = new LogInfo { BypassBuffering = true, LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
            log.AddProperty("Message", GetString("UserRestart"));
            LogController.Instance.AddLog(log);
            Config.Touch();
        }

        #endregion
    }
}
