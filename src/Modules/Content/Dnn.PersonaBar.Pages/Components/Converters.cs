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
                AbsoluteUrl = tab.FullUrl,
                LocalizedName = tab.LocalizedTabName,
                Title = tab.Title,
                Description = description,
                Keywords = keywords,
                Tags = string.Join(",", from t in tab.Terms select t.Name),
                Alias = PortalSettings.Current.PortalAlias.HTTPAlias,
                Url = pageManagementController.GetTabUrl(tab),
                InternalUrl = tab.Url,
                Created = pageManagementController.GetCreatedInfo(tab),
                Hierarchy = pageManagementController.GetTabHierarchy(tab),
                Status = GetTabStatus(tab),
                PageType = GetPageType(tab.Url),
                CreatedOnDate = tab.CreatedOnDate,
                IncludeInMenu = tab.IsVisible,
                CustomUrlEnabled = !tab.IsSuperTab && (Config.GetFriendlyUrlProvider() == "advanced"),
                StartDate = tab.StartDate != Null.NullDate ? tab.StartDate : (DateTime?) null,
                EndDate = tab.EndDate != Null.NullDate ? tab.EndDate : (DateTime?) null,
                IsSecure = tab.IsSecure,
                AllowIndex = AllowIndex(tab),
                CacheProvider = (string)tab.TabSettings["CacheProvider"],
                CacheDuration = CacheDuration(tab),
                CacheIncludeExclude = CacheIncludeExclude(tab),
                CacheIncludeVaryBy = (string)tab.TabSettings["IncludeVaryBy"],
                CacheExcludeVaryBy = (string)tab.TabSettings["ExcludeVaryBy"],
                CacheMaxVaryByCount = MaxVaryByCount(tab),
                PageHeadText = tab.PageHeadText,
                SiteMapPriority = tab.SiteMapPriority,
                PermanentRedirect = tab.PermanentRedirect,
                LinkNewWindow = LinkNewWindow(tab),
                PageStylesheet = (string)tab.TabSettings["CustomStylesheet"],
                Theme = GetTabTheme(tab)
            };
        }

        private static int? CacheDuration(TabInfo tab)
        {
            return tab.TabSettings["CacheDuration"] != null ? int.Parse((string)tab.TabSettings["CacheDuration"]) : (int?)null;
        }

        private static bool? CacheIncludeExclude(TabInfo tab)
        {
            return tab.TabSettings["CacheIncludeExclude"] != null ? (string)tab.TabSettings["CacheIncludeExclude"] == "1" : (bool?)null;
        }

        private static bool LinkNewWindow(TabInfo tab)
        {
            return tab.TabSettings["LinkNewWindow"] != null && (string)tab.TabSettings["LinkNewWindow"] == "True";
        }

        private static int? MaxVaryByCount(TabInfo tab)
        {
            return tab.TabSettings["MaxVaryByCount"] != null ? int.Parse((string)tab.TabSettings["MaxVaryByCount"]) : (int?)null;
        }

        private static Theme GetTabTheme(TabInfo tab)
        {
            return new Theme
            {
                SkinSrc = tab.SkinSrc,
                ContainerSrc = tab.ContainerSrc
            };
        }

        private static bool AllowIndex(TabInfo tab)
        {
            bool allowIndex;
            return !tab.TabSettings.ContainsKey("AllowIndex") || !bool.TryParse(tab.TabSettings["AllowIndex"].ToString(), out allowIndex) || allowIndex;
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