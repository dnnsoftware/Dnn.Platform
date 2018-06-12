#region Copyright

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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.Framework;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.Web.Components.Controllers.Models;

namespace DotNetNuke.Web.Components.Controllers
{
    public class ControlBarController: ServiceLocator<IControlBarController, ControlBarController>, IControlBarController
    {
        private const string BookmarkModulesTitle = "module";
        private const string BookmarkCategoryProperty = "ControlBar_BookmarkCategory";
        private readonly ExtensionPointManager _mef;

        public ControlBarController()
        {
            _mef = new ExtensionPointManager();
        }
        public IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetCategoryDesktopModules(int portalId, string category, string searchTerm = "")
        {
            var formattedSearchTerm = String.IsNullOrEmpty(searchTerm) ? string.Empty : searchTerm.ToLower(CultureInfo.InvariantCulture);

            Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool> Filter = category == "All"
                ? (kvp => kvp.Key.ToLower(CultureInfo.InvariantCulture).Contains(formattedSearchTerm))
                : (Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool>)(kvp => kvp.Value.DesktopModule.Category == category && kvp.Key.ToLower(CultureInfo.InvariantCulture).Contains(formattedSearchTerm));

            var portalModulesList = DesktopModuleController.GetPortalDesktopModules(portalId).Where(Filter);
            return portalModulesList;
        }

        public IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetBookmarkedDesktopModules(int portalId, int userId, string searchTerm = "")
        {
            var formattedSearchTerm = String.IsNullOrEmpty(searchTerm) ? string.Empty : searchTerm.ToLower(CultureInfo.InvariantCulture);
            
            IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> bookmarkedModules = GetBookmarkedModules(PortalSettings.Current.PortalId, userId)
                .Where(kvp => kvp.Key.ToLower(CultureInfo.InvariantCulture).Contains(formattedSearchTerm));

            return bookmarkedModules;
        }

        public void SaveBookMark(int portalId, int userId, string bookmarkTitle, string bookmarkValue)
        {
            var ensuredBookmarkValue = bookmarkValue;
            if (bookmarkTitle == BookmarkModulesTitle)
            {
                ensuredBookmarkValue = EnsureBookmarkValue(portalId, ensuredBookmarkValue);
            }
            
            var personalizationController = new DotNetNuke.Services.Personalization.PersonalizationController();
            var personalization = personalizationController.LoadProfile(userId, portalId);
            personalization.Profile["ControlBar:" + bookmarkTitle + portalId] = ensuredBookmarkValue;
            personalization.IsModified = true;
            personalizationController.SaveProfile(personalization);
        }

        public string GetBookmarkCategory(int portalId)
        {
            var bookmarkCategory = PortalController.GetPortalSetting(BookmarkCategoryProperty, portalId, "");
            if (String.IsNullOrEmpty(bookmarkCategory))
            {
                PortalController.UpdatePortalSetting(portalId, BookmarkCategoryProperty, "Common");
                return "Common";
            }
            return bookmarkCategory;
        }

        public UpgradeIndicatorViewModel GetUpgradeIndicator(Version version, bool isLocal, bool isSecureConnection)
        {
            var imageUrl = Upgrade.UpgradeIndicator(version, isLocal, isSecureConnection);
            return !String.IsNullOrEmpty(imageUrl) ? GetDefaultUpgradeIndicator(imageUrl) : null;            
        }

        public string GetControlBarLogoURL()
        {
            return HostController.Instance.GetString("ControlBarLogoURL", "~/admin/controlpanel/controlbarimages/dnnLogo.png");
        }

        public IEnumerable<MenuItemViewModel> GetCustomMenuItems()
        {
            var menuItemsExtensionPoints = _mef.GetUserControlExtensionPoints("ControlBar", "CustomMenuItems");
            return menuItemsExtensionPoints.Select(GetMenuItemFromExtensionPoint);
        }

        private UpgradeIndicatorViewModel GetDefaultUpgradeIndicator(string imageUrl)
        {
            var alt = LocalizationHelper.GetControlBarString("Upgrade.Text");
            var toolTip = LocalizationHelper.GetControlBarString("Upgrade.ToolTip");
            var navigateUrl = Upgrade.UpgradeRedirect();

            return new UpgradeIndicatorViewModel
            {
                ID = "ServiceImg",
                ImageUrl = imageUrl,
                WebAction = "location.href='" + navigateUrl + "'; return false;",
                AltText = alt,
                ToolTip = toolTip,
                CssClass = ""
            };            
        }

        private MenuItemViewModel GetMenuItemFromExtensionPoint(IUserControlExtensionPoint userControlExtensionPoint)
        {
            return new MenuItemViewModel
            {
                ID = Path.GetFileNameWithoutExtension(userControlExtensionPoint.UserControlSrc),
                Text = userControlExtensionPoint.Text,
                Source = userControlExtensionPoint.UserControlSrc,
                Order = userControlExtensionPoint.Order
            };
        }

        private string EnsureBookmarkValue(int portalId, string bookmarkValue)
        {
            var bookmarkCategoryModules = GetCategoryDesktopModules(portalId, GetBookmarkCategory(portalId));            
            var ensuredModules = bookmarkValue.Split(',').Where(desktopModuleId => bookmarkCategoryModules.All(m => m.Value.DesktopModuleID.ToString(CultureInfo.InvariantCulture) != desktopModuleId)).ToList();
            return String.Join(",", ensuredModules.Distinct());
        }

        private IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetBookmarkedModules(int portalId, int userId)
        {
            var personalizationController = new DotNetNuke.Services.Personalization.PersonalizationController();
            var personalization = personalizationController.LoadProfile(userId, portalId);
            var bookmarkItems = personalization.Profile["ControlBar:"+ BookmarkModulesTitle + portalId];
            if (bookmarkItems == null)
            {
                return new List<KeyValuePair<string, PortalDesktopModuleInfo>>();
            }
            var bookmarkItemsKeys = bookmarkItems.ToString().Split(',').ToList();
            var bookmarkedModules = DesktopModuleController.GetPortalDesktopModules(PortalSettings.Current.PortalId)
                                        .Where(dm => bookmarkItemsKeys.Contains(dm.Value.DesktopModuleID.ToString(CultureInfo.InvariantCulture)));

            return bookmarkedModules;
        }
        
        protected override Func<IControlBarController> GetFactory()
        {
            return () => new ControlBarController();
        }
    }
}
