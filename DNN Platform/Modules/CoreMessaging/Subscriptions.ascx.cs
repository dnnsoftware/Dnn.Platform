// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public partial class Subscriptions : UserControl
    {
        private const string SharedResources = "~/DesktopModules/CoreMessaging/App_LocalResources/SharedResources.resx";

        public ModuleInstanceContext ModuleContext { get; set; }

        public ModuleInfo ModuleConfiguration
        {
            get { return this.ModuleContext != null ? this.ModuleContext.Configuration : null; }

            set
            {
                this.ModuleContext.Configuration = value;
            }
        }

        public string LocalResourceFile { get; set; }

        public string GetSettingsAsJson()
        {
            var settings = GetModuleSettings(PortalSettings.Current, this.ModuleConfiguration, Null.NullInteger);
            foreach (DictionaryEntry entry in this.GetViewSettings())
            {
                if (settings.ContainsKey(entry.Key))
                {
                    settings[entry.Key] = entry.Value;
                }
                else
                {
                    settings.Add(entry.Key, entry.Value);
                }
            }

            return settings.ToJson();
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            if (this.Request.IsAuthenticated)
            {
                ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/CoreMessaging/Scripts/LocalizationController.js");
                ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/CoreMessaging/Scripts/SubscriptionsViewModel.js");
                ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/CoreMessaging/Scripts/Subscription.js");
                ClientResourceManager.RegisterStyleSheet(this.Page, "~/DesktopModules/CoreMessaging/subscriptions.css");
            }
            else
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), false);
            }
        }

        private static Hashtable GetModuleSettings(PortalSettings portalSettings, ModuleInfo moduleInfo, int uniqueId)
        {
            var usePopup =
                portalSettings.EnablePopUps &&
                portalSettings.LoginTabId == Null.NullInteger &&
                !AuthenticationController.HasSocialAuthenticationEnabled();

            var navigationKey =
                moduleInfo != null &&
                moduleInfo.DesktopModule != null
                    ? GetHistoryNavigationKey(moduleInfo.DesktopModule.FriendlyName)
                    : null;

            var moduleRoot =
                moduleInfo != null &&
                moduleInfo.DesktopModule != null
                    ? moduleInfo.DesktopModule.FolderName
                    : null;

            var moduleTitle = moduleInfo != null
                ? moduleInfo.ModuleTitle
                : null;

            var moduleId = moduleInfo != null ? moduleInfo.ModuleID : Null.NullInteger;

            var moduleSettings = moduleInfo != null ? moduleInfo.ModuleSettings : new Hashtable();

            var debug = false;

#if DEBUG
            debug = true;
#else
            if (HttpContext.Current != null)
            {
                debug = HttpContext.Current.IsDebuggingEnabled;
            }
#endif

            return new Hashtable
                   {
                       { "anonymous", PortalSettings.Current.UserId < 0 },
                       { "currentUserId", PortalSettings.Current.UserId },
                       { "debug", debug },
                       { "culture", CultureInfo.CurrentUICulture.Name },
                       { "showMissingKeys", Localization.ShowMissingKeys },
                       { "portalId", portalSettings.PortalId },
                       { "moduleId", moduleId },
                       { "moduleSettings", moduleSettings },
                       { "moduleTitle", moduleTitle },
                       { "moduleRoot", moduleRoot },
                       { "navigationKey", navigationKey },
                       { "sessionTimeout", Convert.ToInt32(GetSessionTimeout().TotalMinutes) },
                       { "sharedResources", GetSharedResources() },
                       { "authorizationUrl", GetLoginUrl(portalSettings) },
                       { "usePopup", usePopup },
                       { "returnUrl", HttpContext.Current.Request.UrlReferrer },
                       { "uniqueId", uniqueId },
                   };
        }

        private static string GetHistoryNavigationKey(string moduleName)
        {
            return HttpContext.Current.Server.HtmlEncode(moduleName.ToLowerInvariant().Replace(" ", string.Empty));
        }

        private static TimeSpan GetSessionTimeout()
        {
            try
            {
                var sessionSection =
                    WebConfigurationManager.GetSection("system.web/sessionState") as SessionStateSection;

                if (sessionSection != null)
                {
                    return sessionSection.Timeout;
                }
            }
            catch
            {
                // FIXME(cbond): The default configuration doesn't actually let us see this data
                // FIXME(cbond): It's too annoying seeing this fill the Event Log, we need to add the permission to web.config
                // Exceptions.LogException(ex);
            }

            return TimeSpan.FromMinutes(25);
        }

        private static IDictionary<string, string> GetSharedResources()
        {
            return new Dictionary<string, string>
                {
                    { "ExceptionTitle", Localization.GetString("ExceptionTitle", SharedResources) },
                    { "ExceptionMessage", Localization.GetString("ExceptionMessage", SharedResources) },
                };
        }

        private static string GetLoginUrl(PortalSettings portalSettings)
        {
            var returnUrl = HttpContext.Current.Request.RawUrl;

            if (portalSettings.UserId < 1)
            {
                var indexOf = returnUrl.IndexOf("?returnurl=", StringComparison.InvariantCultureIgnoreCase);
                if (indexOf >= 0)
                {
                    returnUrl = returnUrl.Substring(0, indexOf);
                }

                returnUrl = Common.Globals.LoginURL(HttpUtility.UrlEncode(returnUrl), true);
            }

            return returnUrl;
        }

        /// <summary>
        /// These values are passed in as the 'settings' parameter of the JavaScript initialization function, together with
        /// values that are automatically retrieved by Social Library such as portalId and moduleId.
        /// </summary>
        private Hashtable GetViewSettings()
        {
            var portalSettings = PortalSettings.Current;
            var userPreferenceController = UserPreferencesController.Instance;
            var user = UserController.GetUserById(portalSettings.PortalId, portalSettings.UserId);
            UserPreference userPreference = null;
            if (user != null)
            {
                userPreference = userPreferenceController.GetUserPreference(user);
            }

            const int notifyFrequency = 2;
            const int messageFrequency = 0;

            return new Hashtable
                   {
                       { "moduleScope", string.Format("#{0}", this.ScopeWrapper.ClientID) },
                       { "pageSize", 25 },
                       { "notifyFrequency", userPreference != null ? (int)userPreference.NotificationsEmailFrequency : notifyFrequency },
                       { "msgFrequency", userPreference != null ? (int)userPreference.MessagesEmailFrequency : messageFrequency },
                   };
        }
    }
}
