// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>
    /// PoralController provides business layer of poatal.
    /// </summary>
    /// <remarks>
    /// DotNetNuke supports the concept of virtualised sites in a single install. This means that multiple sites,
    /// each potentially with multiple unique URL's, can exist in one instance of DotNetNuke i.e. one set of files and one database.
    /// </remarks>
    public partial class PortalController
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. Replaced by PortalController.Instance.GetCurrentPortalSettings. Scheduled removal in v10.0.0.")]
        public static PortalSettings GetCurrentPortalSettings()
        {
            return GetCurrentPortalSettingsInternal();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.4.0. Replaced by PortalController.Instance.GetPortalSettings. Scheduled removal in v10.0.0.")]
        public static Dictionary<string, string> GetPortalSettingsDictionary(int portalId)
        {
            return Instance.GetPortalSettings(portalId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Use one of the alternate overloads. Scheduled removal in v10.0.0.")]
        public int CreatePortal(string portalName, string firstName, string lastName, string username, string password, string email,
                        string description, string keyWords, string templatePath, string templateFile, string homeDirectory,
                        string portalAlias, string serverPath, string childPath, bool isChildPortal)
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
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Use one of the alternate overloads. Scheduled removal in v10.0.0.")]
        public int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, string templatePath,
                        string templateFile, string homeDirectory, string portalAlias,
                        string serverPath, string childPath, bool isChildPortal)
        {
            var template = this.GetPortalTemplate(Path.Combine(templatePath, templateFile), null);

            return this.CreatePortal(portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias,
                                serverPath, childPath, isChildPortal);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Scheduled removal in v10.0.0.")]
        public void DeletePortalInfo(int portalId)
        {
            UserController.DeleteUsers(portalId, false, true);

            DataProvider.Instance().DeletePortalInfo(portalId);

            EventLogController.Instance.AddLog("PortalId", portalId.ToString(), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_DELETED);

            DataCache.ClearHostCache(true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Use one of the alternate overloads. Scheduled removal in v10.0.0.")]
        public void ParseTemplate(int portalId, string templatePath, string templateFile, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            this.ParseTemplateInternal(portalId, templatePath, templateFile, administratorId, mergeTabs, isNewPortal);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Use one of the other overloads. Scheduled removal in v10.0.0.")]
        public void ParseTemplate(int portalId, string templatePath, string templateFile, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal, out LocaleCollection localeCollection)
        {
            this.ParseTemplateInternal(portalId, templatePath, templateFile, administratorId, mergeTabs, isNewPortal, out localeCollection);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. Replaced by UpdatePortalExpiry(int, string). Scheduled removal in v10.0.0.")]
        public void UpdatePortalExpiry(int portalId)
        {
            this.UpdatePortalExpiry(portalId, GetActivePortalLanguage(portalId));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.0. Use one of the alternate overloads. Scheduled removal in v10.0.0.")]
        public void UpdatePortalInfo(PortalInfo portal, bool clearCache)
        {
            this.UpdatePortalInternal(portal, clearCache);
        }
    }
}
