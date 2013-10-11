#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.HtmlControls;

//using DotNetNuke.UI.Utilities;
using DotNetNuke.Collections.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Personalization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Framework
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : CDefault
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[sun1]	1/19/2004	Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class CDefault : PageBase
    {
        public string Author = "";
        public string Comment = "";
        public string Copyright = "";
        public string Description = "";
        public string Generator = "";
        public string KeyWords = "";
        public new string Title = "";

        #region Private Members

        private int GettingStartedTabId
        {
            get
            {
                return PortalController.GetPortalSettingAsInteger("GettingStartedTabId", PortalSettings.PortalId, -1);
            }
        }

        private string GettingStartedTitle
        {
            get
            {
                var tabcontroller = new TabController();
                var tab = tabcontroller.GetTab(GettingStartedTabId, PortalSettings.PortalId, false);
                return tab.Title;
            }
        }

        #endregion

        #region Private Methods

        private bool IsPage(int tabId)
        {
            bool result = false;
            result = (PortalSettings.ActiveTab.TabID == tabId);
            return result;
        }

        #endregion

        protected override void RegisterAjaxScript()
        {
            if (Page.Form != null)
            {
                ServicesFrameworkInternal.Instance.RegisterAjaxScript(Page);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Allows the scroll position on the page to be moved to the top of the passed in control.
        /// </summary>
        /// <param name="objControl">Control to scroll to</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void ScrollToControl(Control objControl)
        {
            if (ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.Positioning))
            {
                ClientAPI.RegisterClientReference(this, ClientAPI.ClientNamespaceReferences.dnn_dom_positioning);
                ClientAPI.RegisterClientVariable(this, "ScrollToControl", objControl.ClientID, true);
                DNNClientAPI.SetScrollTop(Page);
            }
        }

        protected string CurrentPortalAliasUrl
        {
            get
            {
                //This statement throws an exception when PortalSettings.PortalAlias.HTTPAlias is a child alias
                //return HttpContext.Current.Request.Url.AbsoluteUri.Substring(0, HttpContext.Current.Request.Url.AbsoluteUri.ToLower().IndexOf(PortalSettings.PortalAlias.HTTPAlias)) + PortalSettings.PortalAlias.HTTPAlias;
                return Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias);
            }
        }

        protected string CurrentDomainUrl
        {
            get
            {
                return Globals.AddHTTP(Globals.GetDomainName(Request));
            }
        }

        protected void ManageGettingStarted()
        {
            //add js of Getting Started Page
            if (GettingStartedTabId > -1 && IsPage(GettingStartedTabId) && Request["ctl"] == null)
            {
                var scriptManager = ScriptManager.GetCurrent(Page);
                if (scriptManager == null)
                {
                    scriptManager = new ScriptManager();
                    Page.Form.Controls.AddAt(0, scriptManager);
                }

                scriptManager.EnablePageMethods = true;

                var gettingStartedFilePath = HttpContext.Current.IsDebuggingEnabled
                                        ? "~/js/Debug/dnn.gettingstarted.js"
                                        : "~/js/dnn.gettingstarted.js";

                ClientResourceManager.RegisterScript(this, gettingStartedFilePath);
                Page.ClientScript.RegisterClientScriptBlock(GetType(), "PageCurrentPortalAliasUrl", "var pageCurrentPortalAliasUrl = '" + CurrentPortalAliasUrl + "';", true);
                Page.ClientScript.RegisterClientScriptBlock(GetType(), "PageCurrentDomainUrl", "var pageCurrentDomainUrl = '" + CurrentDomainUrl + "';", true);
                Page.ClientScript.RegisterClientScriptBlock(GetType(), "PageCurrentPortalId", "var pageCurrentPortalId = " + PortalSettings.PortalId + ";", true);
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "GettingStartedPageTitle", "var gettingStartedPageTitle = '" + GettingStartedTitle + "';", true);

                //stop the Ge
                Personalization.SetProfile("GettingStarted", "Display", false);
            }

            if (ShowGettingStartedPage)
            {
                DNNClientAPI.ShowModalPage(Page, GettingStartedPageUrl);
            }
        }

        protected void ManageInstallerFiles()
        {
            if (!HostController.Instance.GetBoolean("InstallerFilesRemoved"))
            {
                Services.Upgrade.Upgrade.DeleteInstallerFiles();
                HostController.Instance.Update("InstallerFilesRemoved", "True", true);
            }
        }

        protected string GettingStartedPageUrl
        {
            get
            {
                string result = "";
                var tabcontroller = new TabController();
                var tab = tabcontroller.GetTab(GettingStartedTabId, PortalSettings.PortalId, false);
                var modulecontroller = new ModuleController();
                var modules = modulecontroller.GetTabModules(tab.TabID).Values;

                if (modules.Count > 0)
                {
                    PortalModuleBase pmb = new PortalModuleBase();
                    result = pmb.EditUrl(tab.TabID, "", false, "mid=" + modules.ElementAt(0).ModuleID, "popUp=true", "ReturnUrl=" + Server.UrlEncode(Globals.NavigateURL()));
                }
                else
                {
                    result = Globals.NavigateURL(tab.TabID);
                }

                return result;
            }
        }

        protected string AdvancedSettingsPageUrl
        {
            get
            {
                string result = "";
                var tabcontroller = new TabController();
                var tab = tabcontroller.GetTabByName("Advanced Settings", PortalSettings.PortalId); //tabcontroller.GetTab(GettingStartedTabId, PortalSettings.PortalId, false);
                var modulecontroller = new ModuleController();
                var modules = modulecontroller.GetTabModules(tab.TabID).Values;

                if (modules.Count > 0)
                {
                    PortalModuleBase pmb = new PortalModuleBase();
                    result = pmb.EditUrl(tab.TabID, "", false, "mid=" + modules.ElementAt(0).ModuleID, "popUp=true", "ReturnUrl=" + Server.UrlEncode(Globals.NavigateURL()));
                }
                else
                {
                    result = Globals.NavigateURL(tab.TabID);
                }

                return result;
            }
        }

        protected bool ShowGettingStartedPage
        {
            get
            {
                var result = false;
                if (GettingStartedTabId > -1)
                {
                    if (!IsPage(GettingStartedTabId) && PortalSettings.UserInfo.IsSuperUser && Host.EnableGettingStartedPage)
                    {
                        result = Convert.ToBoolean(Personalization.GetProfile("GettingStarted", "Display"));
                    }
                }
                return result;
            }
        }

        #region WebMethods

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod()]
        public static bool SetGettingStartedPageAsShown(int portailId)
        {
            string pageShown = PortalController.GetPortalSetting("GettingStartedPageShown", portailId, Boolean.FalseString);
            if (!string.Equals(pageShown, Boolean.TrueString))
            {
                PortalController.UpdatePortalSetting(portailId, "GettingStartedPageShown", Boolean.TrueString);
            }
            return true;
        }

        #endregion

        #region Obsolete Methods

        [Obsolete("Deprecated in DotNetNuke 6.0.  Replaced by RegisterStyleSheet")]
        public void AddStyleSheet(string id, string href, bool isFirst)
        {
            RegisterStyleSheet(this, href, isFirst);
        }

        [Obsolete("Deprecated in DotNetNuke 6.0.  Replaced by RegisterStyleSheet")]
        public void AddStyleSheet(string id, string href)
        {
            RegisterStyleSheet(this, href, false);
        }

        #endregion
    }
}
