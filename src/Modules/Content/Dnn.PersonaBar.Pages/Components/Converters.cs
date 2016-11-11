using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Pages.Services.Dto;
using Dnn.PersonaBar.Themes.Components;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.FileSystem;

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
                TabPath = tab.TabPath.Replace("//", "/"),
                PageType = GetPageType(tab.Url)
        };
        }
        
        public static ModuleItem ConvertToModuleItem(ModuleInfo module) => new ModuleItem
        {
            Id = module.ModuleID,
            Title = module.ModuleTitle,
            FriendlyName = module.DesktopModule.FriendlyName,
            EditSettingUrl = GetModuleEditSettingUrl(module)
        };

        private static string GetModuleEditSettingUrl(ModuleInfo module)
        {
            return Globals.NavigateURL(module.TabID, PortalSettings.Current, "Module", "ModuleId=" + module.ModuleID);
        }

        public static T ConvertToPageSettings<T>(TabInfo tab) where T: PageSettings, new()
        {
            if (tab == null)
            {
                return null;
            }

            var description = !string.IsNullOrEmpty(tab.Description) ? tab.Description : PortalSettings.Current.Description;
            var keywords = !string.IsNullOrEmpty(tab.KeyWords) ? tab.KeyWords : PortalSettings.Current.KeyWords;
            var pageType = GetPageType(tab.Url);

            var pageManagementController = PageManagementController.Instance;
            
            var fileId = GetFileIdRedirection(tab.Url);
            string fileUrl = GetFileUrlRedirection(fileId);

            return new T
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
                ExternalRedirection = pageType == "url" ? tab.Url : null,
                FileIdRedirection = pageType == "file" ? fileId : null,
                FileUrlRedirection = pageType == "file" ? fileUrl : null,
                ExistingTabRedirection = pageType == "tab" ? tab.Url : null,
                Created = pageManagementController.GetCreatedInfo(tab),
                Hierarchy = pageManagementController.GetTabHierarchy(tab),
                Status = GetTabStatus(tab),
                PageType = pageType,
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
                PageStyleSheet = (string)tab.TabSettings["CustomStylesheet"],
                ThemeName = GetThemeNameFromSkinSrc(tab.SkinSrc),
                SkinSrc = tab.SkinSrc,
                ContainerSrc = tab.ContainerSrc
            };
        }
        
        private static string GetThemeNameFromSkinSrc(string skinSrc)
        {
            if (string.IsNullOrWhiteSpace(skinSrc))
            {
                return null;
            }
            var themeController = ThemesController.Instance;
            var layout = themeController.GetThemeFile(PortalSettings.Current, skinSrc, ThemeType.Skin);
            return layout?.ThemeName;
        }

        private static int? GetFileIdRedirection(string tabUrl)
        {
            if (tabUrl == null || !tabUrl.StartsWith("FileId="))
            {
                return null;
            }

            int fileRedirectionId;
            if (int.TryParse(tabUrl.Substring(7), out fileRedirectionId))
            {
                return fileRedirectionId;
            }
            return null;
        }

        private static string GetFileUrlRedirection(int? fileId)
        {
            if (!fileId.HasValue)
            {
                return null;
            }
            var file = FileManager.Instance.GetFile(fileId.Value);
            return file != null ? FileManager.Instance.GetUrl(file) : null;
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