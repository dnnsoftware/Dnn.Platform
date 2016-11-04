using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.Pages.Components
{
    public static class Converters
    {

        public static T ConvertToPageItem<T>(TabInfo tab, IEnumerable<TabInfo> portalTabs) where T : PageItem, new()
        {
            return new T
            {
                Id = tab.TabID,
                Name = tab.LocalizedTabName,
                Url = tab.FullUrl,
                ChildrenCount = portalTabs?.Count(ct => ct.ParentId == tab.TabID) ?? 0,
                Status = GetTabStatus(tab),
                ParentId = tab.ParentId,
                Level = tab.Level,
                IsSpecial = TabController.IsSpecialTab(tab.TabID, PortalSettings.Current),
                TabPath = tab.TabPath.Replace("//", "/")
            };
        }
        
        public static ModuleItem ConvertToModuleItem(ModuleInfo module) => new ModuleItem()
        {
            Id = module.ModuleID,
            Title = module.ModuleTitle,
            FriendlyName = module.DesktopModule.FriendlyName
        };

        public static PageSettings ConvertToPageSettings(TabInfo tab)
        {
            if (tab == null)
            {
                return null;
            }

            var description = !string.IsNullOrEmpty(tab.Description) ? tab.Description : PortalSettings.Current.Description;
            var keywords = !string.IsNullOrEmpty(tab.KeyWords) ? tab.KeyWords : PortalSettings.Current.KeyWords;

            var pageManagementController = PageManagementController.Instance;

            return new PageSettings
            {
                TabId = tab.TabID,
                Name = tab.TabName,
                LocalizedName = tab.LocalizedTabName,
                Title = tab.Title,
                Description = description,
                Keywords = keywords,
                Tags = string.Join(",", from t in tab.Terms select t.Name),
                Alias = PortalSettings.Current.PortalAlias.HTTPAlias,
                Url = tab.Url,
                Created = pageManagementController.GetCreatedInfo(tab),
                Hierarchy = pageManagementController.GetTabHierarchy(tab),
                Status = GetTabStatus(tab),
                PageType = GetPageType(tab.Url),
                CreatedOnDate = tab.CreatedOnDate,
                IncludeInMenu = tab.IsVisible,
                CustomUrlEnabled = !tab.IsSuperTab && (Config.GetFriendlyUrlProvider() == "advanced"),
                StartDate = tab.StartDate != Null.NullDate ? tab.StartDate : (DateTime?) null,
                EndDate = tab.EndDate != Null.NullDate ? tab.EndDate : (DateTime?) null
            };
        }

        private static string GetPageType(string tabUrl)
        {
            return Globals.GetURLType(tabUrl).ToString().ToLower();
        }

        // TODO: Refactor to use enum
        private static string GetTabStatus(TabInfo tab)
        {
            if (tab.DisableLink)
            {
                return "Disabled";
            }

            return tab.IsVisible ? "Visible" : "Hidden";
        }
    }
}