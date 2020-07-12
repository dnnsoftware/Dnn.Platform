// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components
{
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Web;
    using System.Web.UI;

    using Dnn.PersonaBar.Security.Services.Dto;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
    using DotNetNuke.Services.Localization;

    public class SecurityController
    {
        private static PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

        public IEnumerable<string> GetAuthenticationProviders()
        {
            var authSystems = AuthenticationController.GetEnabledAuthenticationServices();
            UserControl uc = new UserControl();
            var authProviders = (from authProvider in authSystems
                                 let authLoginControl = (AuthenticationLoginBase)uc.LoadControl("~/" + authProvider.LoginControlSrc)
                                 let oAuthLoginControl = authLoginControl as OAuthLoginBase
                                 where oAuthLoginControl == null && authLoginControl.Enabled
                                 select authProvider.AuthenticationType);

            return authProviders;
        }

        public IList<UserRoleInfo> GetAdminUsers(int portalId, string cultureCode = "")
        {
            var activeLanguage = string.IsNullOrEmpty(cultureCode)
                ? LocaleController.Instance.GetDefaultLocale(portalId).Code
                : cultureCode;
            var portal = PortalController.Instance.GetPortal(portalId, activeLanguage);
            var adminUsers = RoleController.Instance.GetUserRoles(portalId, null, portal.AdministratorRoleName);

            return adminUsers;
        }

        public TabDto GetPortalTabs(int portalId, string cultureCode, bool isMultiLanguage)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = LocaleController.Instance.GetDefaultLocale(portalId).Code;
            }

            var rootNode = new TabDto
            {
                Name = PortalSettings.PortalName,
                TabId = Null.NullInteger.ToString(CultureInfo.InvariantCulture),
                ParentTabId = Null.NullInteger,
                ChildTabs = new List<TabDto>()
            };

            var portalInfo = PortalController.Instance.GetPortal(portalId);
            var tabs = TabController.GetPortalTabs(isMultiLanguage ? TabController.GetTabsBySortOrder(portalId, portalInfo.DefaultLanguage, true) : TabController.GetTabsBySortOrder(portalId, cultureCode, true), Null.NullInteger, false, "<" + Localization.GetString("None_Specified") + ">", true, false, true, false, false).Where(t => !t.IsSystem).ToList();
            foreach (var tab in tabs)
            {
                if (tab.Level == 0 && tab.TabID != portalInfo.AdminTabId)
                {
                    var node = new TabDto
                    {
                        Name = tab.TabName,
                        TabId = tab.TabID.ToString(CultureInfo.InvariantCulture),
                        ParentTabId = tab.ParentId,
                        ChildTabs = new List<TabDto>()
                    };

                    this.AddChildNodes(node, portalInfo, cultureCode);
                    rootNode.ChildTabs.Add(node);
                }
            }

            return rootNode;
        }

        public List<DataTable> GetModifiedSettings()
        {
            var tables = new List<DataTable>();
            var reader = DataProvider.Instance().ExecuteReader("SecurityAnalyzer_GetModifiedSettings");
            if (reader != null)
            {
                do
                {
                    var table = new DataTable { Locale = CultureInfo.CurrentCulture };
                    table.Load(reader);
                    tables.Add(table);
                }
                while (!reader.IsClosed);
            }
            return tables;
        }

        private void AddChildNodes(TabDto parentNode, PortalInfo portal, string cultureCode)
        {
            if (parentNode.ChildTabs != null)
            {
                parentNode.ChildTabs.Clear();
                int parentId;
                int.TryParse(parentNode.TabId, out parentId);
                var tabs = this.GetFilteredTabs(TabController.Instance.GetTabsByPortal(portal.PortalID).WithCulture(cultureCode, true)).WithParentId(parentId);

                foreach (var tab in tabs)
                {
                    if (tab.ParentId == parentId)
                    {
                        var node = new TabDto
                        {
                            Name = tab.TabName,
                            TabId = tab.TabID.ToString(CultureInfo.InvariantCulture),
                            ParentTabId = tab.ParentId
                        };
                        this.AddChildNodes(node, portal, cultureCode);
                        parentNode.ChildTabs.Add(node);
                    }
                }
            }
        }

        private TabCollection GetFilteredTabs(TabCollection tabs)
        {
            var filteredTabs = tabs.Where(kvp => !kvp.Value.IsSystem && !kvp.Value.IsDeleted && !kvp.Value.DisableLink).Select(kvp => kvp.Value);
            return new TabCollection(filteredTabs);
        }
    }
}
