﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Linq;
using System.Web.UI;

using DotNetNuke.Common.Internal;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.UI.Utilities;

#endregion

namespace DotNetNuke.Framework
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : CDefault
    /// -----------------------------------------------------------------------------
    /// -----------------------------------------------------------------------------
    public class CDefault : PageBase
    {
        public string Author = string.Empty;
        public string Comment = string.Empty;
        public string Copyright = string.Empty;
        public string Description = string.Empty;
        public string Generator = string.Empty;
        public string KeyWords = string.Empty;
        public new string Title = string.Empty;

        private static readonly object InstallerFilesRemovedLock = new object();

        protected override void RegisterAjaxScript()
        {
            if (Page.Form != null)
            {
                if (ServicesFrameworkInternal.Instance.IsAjaxScriptSupportRequired)
                {
                    ServicesFrameworkInternal.Instance.RegisterAjaxScript(Page);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Allows the scroll position on the page to be moved to the top of the passed in control.
        /// </summary>
        /// <param name="objControl">Control to scroll to</param>
        /// -----------------------------------------------------------------------------
        public void ScrollToControl(Control objControl)
        {
            if (ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.Positioning))
            {
                JavaScript.RegisterClientReference(this, ClientAPI.ClientNamespaceReferences.dnn_dom_positioning);
                ClientAPI.RegisterClientVariable(this, "ScrollToControl", objControl.ClientID, true);
                DNNClientAPI.SetScrollTop(Page);
            }
        }

        protected void ManageInstallerFiles()
        {
            if (!HostController.Instance.GetBoolean("InstallerFilesRemoved"))
            {
                lock (InstallerFilesRemovedLock)
                {
                    if (!HostController.Instance.GetBoolean("InstallerFilesRemoved"))
                    {
                        Services.Upgrade.Upgrade.DeleteInstallerFiles();
                        HostController.Instance.Update("InstallerFilesRemoved", "True", true);
                    }
                }
            }
        }

        protected string AdvancedSettingsPageUrl
        {
            get
            {
                string result ;
                var tab = TabController.Instance.GetTabByName("Advanced Settings", PortalSettings.PortalId);
                var modules = ModuleController.Instance.GetTabModules(tab.TabID).Values;

                if (modules.Count > 0)
                {
                    var pmb = new PortalModuleBase();
                    result = pmb.EditUrl(tab.TabID, "", false, string.Concat("mid=", modules.ElementAt(0).ModuleID), "popUp=true", string.Concat("ReturnUrl=", Server.UrlEncode(TestableGlobals.Instance.NavigateURL())));
                }
                else
                {
                    result = TestableGlobals.Instance.NavigateURL(tab.TabID);
                }

                return result;
            }
        }
    }
}
