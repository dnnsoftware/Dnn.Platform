// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Containers;

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;

using Dnn.PersonaBar.Library.Common;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Abstractions;
using DotNetNuke.Application;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Personalization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

using Globals = DotNetNuke.Common.Globals;

public class PersonaBarContainer : IPersonaBarContainer
{
    private static IPersonaBarContainer instance;

    public PersonaBarContainer(INavigationManager navigationManager)
    {
        this.NavigationManager = navigationManager;
    }

    public static IPersonaBarContainer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Globals.DependencyProvider.GetRequiredService<IPersonaBarContainer>();
            }

            return instance;
        }
    }

    /// <inheritdoc/>
    public virtual IList<string> RootItems => new List<string> { "Content", "Manage", "Settings", "Edit" };

    /// <inheritdoc/>
    public virtual bool Visible => true;

    protected INavigationManager NavigationManager { get; }

    public static void SetInstance(IPersonaBarContainer instance, bool overwrite = false)
    {
        if (PersonaBarContainer.instance == null || overwrite)
        {
            PersonaBarContainer.instance = instance;
        }
    }

    public static void ClearInstance()
    {
        instance = null;
    }

    /// <inheritdoc/>
    public virtual void Initialize(UserControl personaBarControl)
    {
    }

    /// <inheritdoc/>
    public virtual IDictionary<string, object> GetConfiguration()
    {
        var portalSettings = PortalSettings.Current;

        return this.GetConfigration(portalSettings);
    }

    /// <inheritdoc/>
    public virtual void FilterMenu(PersonaBarMenu menu)
    {
    }

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
        settings.Add("userMode", Personalization.GetUserMode().ToString());
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

        var customModules = new List<string>() { "serversummary" };
        settings.Add("customModules", customModules);

        settings.Add("disableEditBar", Host.DisableEditBar);

        var customPersonaBarThemePath = HostingEnvironment.MapPath("~/Portals/_default/PersonaBarTheme.css");
        var customPersonaBarThemeExists = File.Exists(customPersonaBarThemePath);
        settings.Add("personaBarTheme", customPersonaBarThemeExists);

        return settings;
    }
}
