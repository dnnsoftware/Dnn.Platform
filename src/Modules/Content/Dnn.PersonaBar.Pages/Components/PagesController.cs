#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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

using System;
using System.Linq;
using System.Net;
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;

namespace Dnn.PersonaBar.Pages.Components
{
    public class PagesController : ServiceLocator<IPagesController, PagesController>, IPagesController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PagesController));

        private readonly ITabController _tabController;

        public PagesController()
        {
            _tabController = TabController.Instance;
        }

        private static PortalSettings PortalSettings => PortalSettings.Current;

        public bool IsValidTabPath(TabInfo tab, string newTabPath, out string errorMessage)
        {
            var valid = true;
            errorMessage = string.Empty;

            //get default culture if the tab's culture is null
            var cultureCode = tab != null ? tab.CultureCode : string.Empty;
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = PortalSettings.DefaultLanguage;
            }

            //Validate Tab Path
            var tabId = TabController.GetTabByTabPath(PortalSettings.PortalId, newTabPath, cultureCode);
            if (tabId != Null.NullInteger && (tab == null || tabId != tab.TabID))
            {
                var existingTab = _tabController.GetTab(tabId, PortalSettings.PortalId, false);
                if (existingTab != null && existingTab.IsDeleted)
                {
                    errorMessage = "TabRecycled";
                }
                else
                {
                    errorMessage = "TabExists";
                }

                valid = false;
            }

            //check whether have conflict between tab path and portal alias.
            if (TabController.IsDuplicateWithPortalAlias(PortalSettings.PortalId, newTabPath))
            {
                errorMessage = "PathDuplicateWithAlias";
                valid = false;
            }

            return valid;
        }

        public PageSettings GetPageDetails(int pageId)
        {
            var tab = TabController.Instance.GetTab(pageId, PortalSettings.PortalId);
            if (tab == null)
            {
                return null;
            }

            var description = !string.IsNullOrEmpty(tab.Description) ? tab.Description : PortalSettings.Description;
            var keywords = !string.IsNullOrEmpty(tab.KeyWords) ? tab.KeyWords : PortalSettings.KeyWords;

            return new PageSettings
            {
                TabId = tab.TabID,
                Name = tab.TabName,
                LocalizedName = tab.LocalizedTabName,
                Title = tab.Title,
                Description = description,
                Keywords = keywords,
                Tags = string.Join(",", from t in tab.Terms select t.Name),
                Alias = PortalSettings.PortalAlias.HTTPAlias,
                Url = tab.Url,
                CreatedOnDate = tab.CreatedOnDate,
                IncludeInMenu = tab.IsVisible,
                CustomUrlEnabled = !tab.IsSuperTab && (Config.GetFriendlyUrlProvider() == "advanced"),
                StartDate = tab.StartDate != Null.NullDate ? tab.StartDate : (DateTime?)null,
                EndDate = tab.EndDate != Null.NullDate ? tab.EndDate : (DateTime?)null,
                Permissions = GetPermissionsData(pageId)
            };
        }

        private PagePermissions GetPermissionsData(int pageId)
        {
            var permissions = new PagePermissions(true);
            if (pageId > 0)
            {
                var tab = TabController.Instance.GetTab(pageId, PortalSettings.PortalId);
                if (tab != null)
                {
                    foreach (TabPermissionInfo permission in tab.TabPermissions)
                    {
                        if (permission.UserID != Null.NullInteger)
                        {
                            permissions.AddUserPermission(permission);
                        }
                        else
                        {
                            permissions.AddRolePermission(permission);
                        }
                    }

                    permissions.RolePermissions =
                        permissions.RolePermissions.OrderByDescending(p => p.Locked)
                            .ThenByDescending(p => p.IsDefault)
                            .ThenBy(p => p.RoleName)
                            .ToList();
                    permissions.UserPermissions = permissions.UserPermissions.OrderBy(p => p.DisplayName).ToList();
                }
            }

            return permissions;
        }

        protected override Func<IPagesController> GetFactory()
        {
            return () => new PagesController();
        }
    }
}