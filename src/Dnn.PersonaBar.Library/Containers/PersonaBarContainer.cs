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

using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI;
using Dnn.PersonaBar.Library.Common;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Application;
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
        #region Instance Methods

        private static IPersonaBarContainer _instance;

        public static IPersonaBarContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PersonaBarContainer();
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

            return GetConfigration(portalSettings);
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
            settings.Add("logOff", Globals.NavigateURL("Logoff"));
            settings.Add("visible", Visible);
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
