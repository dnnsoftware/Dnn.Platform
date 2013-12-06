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
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Common;

#endregion

namespace DotNetNuke.Modules.CoreMessaging
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The View class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : PortalModuleBase
    {
        #region Public Properties

        public int ProfileUserId
        {
            get
            {
                var userId = Null.NullInteger;
                if (!string.IsNullOrEmpty(Request.Params["UserId"]))
                {
                    userId = Int32.Parse(Request.Params["UserId"]);
                }
                return userId;
            }
        }

        public string ShowAttachments
        {
            get
            {
                var allowAttachments = PortalController.GetPortalSetting("MessagingAllowAttachments", PortalId, "NO");
                return allowAttachments == "NO" ? "false" : "true";
            }
        }

	    public bool ShowSubscriptionTab
	    {
		    get
		    {
			    return !Settings.ContainsKey("ShowSubscriptionTab") ||
			           Settings["ShowSubscriptionTab"].ToString().Equals("true", StringComparison.InvariantCultureIgnoreCase);
		    }
	    }

        #endregion

        #region Event Handlers

        override protected void OnInit(EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                // Do not redirect but hide the content of the module and display a message.
                CoreMessagingContainer.Visible = false;
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ContentNotAvailable", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                return;
            }
            if (UserId != ProfileUserId && (PortalSettings.ActiveTab.ParentId == PortalSettings.UserTabId || TabId == PortalSettings.UserTabId))
            {
				// Do not redirect but hide the content of the module.
				CoreMessagingContainer.Visible = false;
				return;
            }
            
            if (IsEditable && PermissionsNotProperlySet())
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PermissionsNotProperlySet", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }

            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            jQuery.RequestDnnPluginsRegistration();
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/CoreMessaging/Scripts/CoreMessaging.js");
            jQuery.RequestDnnPluginsRegistration();
			jQuery.RegisterFileUpload(Page);
            AddIe7StyleSheet();

            base.OnInit(e);
        }

        #endregion

        #region Private Methods

        private void AddIe7StyleSheet()
        {
            var browser = Request.Browser;
            if (browser.Type == "IE" || browser.MajorVersion < 8)
            {
                const string cssLink = "<link href=\"/ie-messages.css\" rel=\"stylesheet\" type=\"text/css\" />";
                Page.Header.Controls.Add(new LiteralControl(cssLink));
            }
        }

        private bool PermissionsNotProperlySet()
        {
            List<PermissionInfoBase> permissions;

            if (ModuleConfiguration.InheritViewPermissions)
            {
                var tabPermissionCollection = TabPermissionController.GetTabPermissions(TabId, PortalId);
                permissions = tabPermissionCollection.ToList();
            }
            else
            {
                permissions = ModuleConfiguration.ModulePermissions.ToList();
            }

            return permissions.Find(PermissionPredicate) != null;
        }

        private static bool PermissionPredicate(PermissionInfoBase p)
        {
            return p.PermissionKey == "VIEW" && p.AllowAccess && (p.RoleName == Globals.glbRoleAllUsersName || p.RoleName == Globals.glbRoleUnauthUserName);
        }

        #endregion
    }
}