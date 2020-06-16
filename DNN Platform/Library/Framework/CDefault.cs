// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Linq;
    using System.Web.UI;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.UI.Utilities;

    // -----------------------------------------------------------------------------
    // Project  : DotNetNuke
    // Class    : CDefault
    // -----------------------------------------------------------------------------
    // -----------------------------------------------------------------------------
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

        protected string AdvancedSettingsPageUrl
        {
            get
            {
                string result;
                var tab = TabController.Instance.GetTabByName("Advanced Settings", this.PortalSettings.PortalId);
                var modules = ModuleController.Instance.GetTabModules(tab.TabID).Values;

                if (modules.Count > 0)
                {
                    var pmb = new PortalModuleBase();
                    result = pmb.EditUrl(tab.TabID, string.Empty, false, string.Concat("mid=", modules.ElementAt(0).ModuleID), "popUp=true", string.Concat("ReturnUrl=", this.Server.UrlEncode(TestableGlobals.Instance.NavigateURL())));
                }
                else
                {
                    result = TestableGlobals.Instance.NavigateURL(tab.TabID);
                }

                return result;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Allows the scroll position on the page to be moved to the top of the passed in control.
        /// </summary>
        /// <param name="objControl">Control to scroll to.</param>
        /// -----------------------------------------------------------------------------
        public void ScrollToControl(Control objControl)
        {
            if (ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.Positioning))
            {
                JavaScript.RegisterClientReference(this, ClientAPI.ClientNamespaceReferences.dnn_dom_positioning);
                ClientAPI.RegisterClientVariable(this, "ScrollToControl", objControl.ClientID, true);
                DNNClientAPI.SetScrollTop(this.Page);
            }
        }

        protected override void RegisterAjaxScript()
        {
            if (this.Page.Form != null)
            {
                if (ServicesFrameworkInternal.Instance.IsAjaxScriptSupportRequired)
                {
                    ServicesFrameworkInternal.Instance.RegisterAjaxScript(this.Page);
                }
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
    }
}
