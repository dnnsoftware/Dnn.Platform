// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.CoreMessaging
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Implements the logic for the default view.</summary>
    public partial class View : PortalModuleBase
    {
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IPortalController portalController;
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public View()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        /// <param name="javaScript">The JavaScript library helper.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public View(IJavaScriptLibraryHelper javaScript)
            : this(javaScript, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        public View(IJavaScriptLibraryHelper javaScript, IPortalController portalController, IClientResourceController clientResourceController)
        {
            this.javaScript = javaScript ?? this.DependencyProvider.GetRequiredService<IJavaScriptLibraryHelper>();
            this.portalController = portalController ?? this.DependencyProvider.GetRequiredService<IPortalController>();
            this.clientResourceController = clientResourceController ?? this.DependencyProvider.GetRequiredService<IClientResourceController>();
        }

        /// <summary>Gets the user id from the request parameters.</summary>
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

        /// <summary>Gets a string indicating whether attachments are allowed "true" or not "false".</summary>
        public string ShowAttachments =>
            PortalController.GetPortalSetting(this.portalController, "MessagingAllowAttachments", this.PortalId, "NO") == "NO" ? "false" : "true";

        /// <summary>Gets a value indicating whether the subscriptions tab should be shown.</summary>
        public bool ShowSubscriptionTab =>
            !this.Settings.ContainsKey("ShowSubscriptionTab") ||
            this.Settings["ShowSubscriptionTab"].ToString().Equals("true", StringComparison.InvariantCultureIgnoreCase);

        /// <summary>Gets a value indicating whether the private messaging should be disabled.</summary>
        public bool DisablePrivateMessage =>
            this.PortalSettings.DisablePrivateMessage && !this.UserInfo.IsSuperUser
                                                      && !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName);

        /// <inheritdoc/>
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
            this.javaScript.RequestRegistration(CommonJs.DnnPlugins);
            this.javaScript.RequestRegistration(CommonJs.Knockout);
            this.clientResourceController.RegisterScript("~/DesktopModules/CoreMessaging/Scripts/CoreMessaging.js");
            this.javaScript.RequestRegistration(CommonJs.jQueryFileUpload);
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
