﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI;
using Microsoft.Extensions.DependencyInjection;
using Dnn.PersonaBar.Library.Common;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Application;
using DotNetNuke.Abstractions;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.ImprovementsProgram;
using Newtonsoft.Json.Linq;
using Globals = DotNetNuke.Common.Globals;

namespace Dnn.PersonaBar.Library.Containers
{
    public class PersonaBarContainer : IPersonaBarContainer
    {
        protected INavigationManager NavigationManager { get; }
        public PersonaBarContainer(INavigationManager navigationManager)
        {
            this.NavigationManager = navigationManager;
        }

        #region Instance Methods

        private static IPersonaBarContainer _instance;

        public static IPersonaBarContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Globals.DependencyProvider.GetRequiredService<IPersonaBarContainer>();
                }

                return _instance;
            }
        }

        public static void SetInstance(IPersonaBarContainer instance, bool overwrite = false)
        {
            if (_instance == null || overwrite)
            {
                _instance = instance;
            }
        }

        public static void ClearInstance()
        {
            _instance = null;
        }

        #endregion

        #region IPersonaBarContainer Implements

        public virtual IList<string> RootItems => new List<string> {"Content", "Manage", "Settings", "Edit"};

        public virtual bool Visible => true;

        public virtual void Initialize(UserControl personaBarControl)
        {

        }

        public virtual IDictionary<string, object> GetConfiguration()
        {
            var portalSettings = PortalSettings.Current;

            return this.GetConfigration(portalSettings);
        }

        public virtual void FilterMenu(PersonaBarMenu menu)
        {

        }

        #endregion

        #region Private Methods

        private IDictionary<string, object> GetConfigration(PortalSettings portalSettings)
        {
            var settings = new Dictionary<string, object>();
            var user = portalSettings.UserInfo;
            var portalId = portalSettings.PortalId;
            var preferredTimeZone = TimeZoneHelper.GetPreferredTimeZone(user.Profile.PreferredTimeZone);

            var menuStructure = PersonaBarController.Instance.GetMenu(portalSettings, user);

            settings.Add("applicationPath", Globals.ApplicationPath);
            settings.Add("buildNumber", Host.CrmVersion.ToString(CultureInfo.InvariantCulture));
            settings.Add("userId", user.UserID);
            settings.Add("avatarUrl", Globals.ResolveUrl(Utilities.GetProfileAvatar(user)));
            settings.Add("culture", Thread.CurrentThread.CurrentUICulture.Name);
            settings.Add("logOff", this.NavigationManager.NavigateURL("Logoff"));
            settings.Add("visible", this.Visible);
            settings.Add("userMode", portalSettings.UserMode.ToString());
            settings.Add("userSettings", PersonaBarUserSettingsController.Instance.GetPersonaBarUserSettings());
            settings.Add("menuStructure", JObject.FromObject(menuStructure));
            settings.Add("sku", DotNetNukeContext.Current.Application.SKU);
            settings.Add("debugMode", HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled);
            settings.Add("portalId", portalId);
            settings.Add("preferredTimeZone", preferredTimeZone);

            if (!settings.ContainsKey("isAdmin"))
            {
                settings.Add("isAdmin", user.IsInRole(portalSettings.AdministratorRoleName));
            }

            if (!settings.ContainsKey("isHost"))
            {
                settings.Add("isHost", user.IsSuperUser);
            }

            if (BeaconService.Instance.IsBeaconEnabledForPersonaBar())
            {
                settings.Add("beaconUrl", GetBeaconUrl());
            }

            var customModules = new List<string>() { "serversummary" };
            settings.Add("customModules", customModules);

            return settings;
        }

        private static string GetBeaconUrl()
        {
            var beaconService = BeaconService.Instance;
            var user = UserController.Instance.GetCurrentUserInfo();
            return beaconService.GetBeaconEndpoint() + beaconService.GetBeaconQuery(user);
        }

        #endregion
    }
}
