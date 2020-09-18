// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.CoreMessaging
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The View class displays the content.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : PortalModuleBase
    {
        public int ProfileUserId
        {
            get
            {
                var userId = Null.NullInteger;
                if (!string.IsNullOrEmpty(this.Request.Params["UserId"]))
                {
                    userId = int.Parse(this.Request.Params["UserId"]);
                }

                return userId;
            }
        }

        public string ShowAttachments
        {
            get
            {
                var allowAttachments = PortalController.GetPortalSetting("MessagingAllowAttachments", this.PortalId, "NO");
                return allowAttachments == "NO" ? "false" : "true";
            }
        }

        public bool ShowSubscriptionTab
        {
            get
            {
                return !this.Settings.ContainsKey("ShowSubscriptionTab") ||
                       this.Settings["ShowSubscriptionTab"].ToString().Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public bool DisablePrivateMessage
        {
            get
            {
                return this.PortalSettings.DisablePrivateMessage && !this.UserInfo.IsSuperUser
                    && !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (!this.Request.IsAuthenticated)
            {
                // Do not redirect but hide the content of the module and display a message.
                this.CoreMessagingContainer.Visible = false;
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ContentNotAvailable", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                return;
            }

            if (this.UserId != this.ProfileUserId && (this.PortalSettings.ActiveTab.ParentId == this.PortalSettings.UserTabId || this.TabId == this.PortalSettings.UserTabId))
            {
                // Do not redirect but hide the content of the module.
                this.CoreMessagingContainer.Visible = false;
                return;
            }

            if (this.IsEditable && this.PermissionsNotProperlySet())
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PermissionsNotProperlySet", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }

            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            JavaScript.RequestRegistration(CommonJs.Knockout);
            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/CoreMessaging/Scripts/CoreMessaging.js");
            jQuery.RegisterFileUpload(this.Page);
            this.AddIe7StyleSheet();

            base.OnInit(e);
        }

        private static bool PermissionPredicate(PermissionInfoBase p)
        {
            return p.PermissionKey == "VIEW" && p.AllowAccess && (p.RoleName == Globals.glbRoleAllUsersName || p.RoleName == Globals.glbRoleUnauthUserName);
        }

        private void AddIe7StyleSheet()
        {
            var browser = this.Request.Browser;
            if (browser.Type == "IE" || browser.MajorVersion < 8)
            {
                const string cssLink = "<link href=\"/ie-messages.css\" rel=\"stylesheet\" type=\"text/css\" />";
                this.Page.Header.Controls.Add(new LiteralControl(cssLink));
            }
        }

        private bool PermissionsNotProperlySet()
        {
            List<PermissionInfoBase> permissions;

            if (this.ModuleConfiguration.InheritViewPermissions)
            {
                var tabPermissionCollection = TabPermissionController.GetTabPermissions(this.TabId, this.PortalId);
                permissions = tabPermissionCollection.ToList();
            }
            else
            {
                permissions = this.ModuleConfiguration.ModulePermissions.ToList();
            }

            return permissions.Find(PermissionPredicate) != null;
        }
    }
}
