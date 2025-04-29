// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Entities.Portals;

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals.Templates;
using DotNetNuke.Entities.Users;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;

/// <summary>PoralController provides business layer of poatal.</summary>
/// <remarks>
/// DotNetNuke supports the concept of virtualised sites in a single install. This means that multiple sites,
/// each potentially with multiple unique URL's, can exist in one instance of DotNetNuke i.e. one set of files and one database.
/// </remarks>
public partial class PortalController
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Replaced by PortalController.Instance.GetCurrentPortalSettings", RemovalVersion = 10)]
    public static partial PortalSettings GetCurrentPortalSettings()
    {
        return GetCurrentPortalSettingsInternal();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 4, 0, "Replaced by PortalController.Instance.GetPortalSettings", RemovalVersion = 10)]
    public static partial Dictionary<string, string> GetPortalSettingsDictionary(int portalId)
    {
        return Instance.GetPortalSettings(portalId);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use one of the alternate overloads", RemovalVersion = 10)]
    public partial int CreatePortal(string portalName, string firstName, string lastName, string username, string password, string email, string description, string keyWords, string templatePath, string templateFile, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
    {
        var adminUser = new UserInfo
        {
            FirstName = firstName,
            LastName = lastName,
            Username = username,
            DisplayName = firstName + " " + lastName,
            Membership = { Password = password },
            Email = email,
            IsSuperUser = false,
        };
        adminUser.Membership.Approved = true;
        adminUser.Profile.FirstName = firstName;
        adminUser.Profile.LastName = lastName;
        return this.CreatePortal(portalName, adminUser, description, keyWords, templatePath, templateFile, homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use one of the alternate overloads", RemovalVersion = 10)]
    public partial int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, string templatePath, string templateFile, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
    {
        var template = PortalTemplateController.Instance.GetPortalTemplate(Path.Combine(templatePath, templateFile), null);

        return this.CreatePortal(portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "0", RemovalVersion = 10)]
    public partial void DeletePortalInfo(int portalId)
    {
        UserController.DeleteUsers(portalId, false, true);

        DataProvider.Instance().DeletePortalInfo(portalId);

        EventLogController.Instance.AddLog("PortalId", portalId.ToString(), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_DELETED);

        DataCache.ClearHostCache(true);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use one of the alternate overloads", RemovalVersion = 10)]
    public partial void ParseTemplate(int portalId, string templatePath, string templateFile, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
    {
        var importer = new PortalTemplateImporter(templatePath, templateFile);
        importer.ParseTemplate(portalId, administratorId, mergeTabs.ToNewEnum(), isNewPortal);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use one of the other overloads", RemovalVersion = 10)]
    public partial void ParseTemplate(int portalId, string templatePath, string templateFile, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal, out LocaleCollection localeCollection)
    {
        var importer = new PortalTemplateImporter(templatePath, templateFile);
        importer.ParseTemplateInternal(portalId, administratorId, mergeTabs.ToNewEnum(), isNewPortal, out localeCollection);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Replaced by UpdatePortalExpiry(int, string)", RemovalVersion = 10)]
    public partial void UpdatePortalExpiry(int portalId)
    {
        this.UpdatePortalExpiry(portalId, GetActivePortalLanguage(portalId));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use one of the alternate overloads", RemovalVersion = 10)]
    public partial void UpdatePortalInfo(PortalInfo portal, bool clearCache)
    {
        this.UpdatePortalInternal(portal, clearCache);
    }
}
