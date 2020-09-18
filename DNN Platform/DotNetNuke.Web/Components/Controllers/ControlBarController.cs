// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Components.Controllers
{
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

    public class ControlBarController : ServiceLocator<IControlBarController, ControlBarController>, IControlBarController
    {
        private const string BookmarkModulesTitle = "module";
        private const string BookmarkCategoryProperty = "ControlBar_BookmarkCategory";
        private readonly ExtensionPointManager _mef;

        public ControlBarController()
        {
            this._mef = new ExtensionPointManager();
        }

        public IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetCategoryDesktopModules(int portalId, string category, string searchTerm = "")
        {
            var formattedSearchTerm = string.IsNullOrEmpty(searchTerm) ? string.Empty : searchTerm.ToLower(CultureInfo.InvariantCulture);

            Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool> Filter = category == "All"
                ? (kvp => kvp.Key.ToLower(CultureInfo.InvariantCulture).Contains(formattedSearchTerm))
                : (Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool>)(kvp => kvp.Value.DesktopModule.Category == category && kvp.Key.ToLower(CultureInfo.InvariantCulture).Contains(formattedSearchTerm));

            var portalModulesList = DesktopModuleController.GetPortalDesktopModules(portalId).Where(Filter);
            return portalModulesList;
        }

        public IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetBookmarkedDesktopModules(int portalId, int userId, string searchTerm = "")
        {
            var formattedSearchTerm = string.IsNullOrEmpty(searchTerm) ? string.Empty : searchTerm.ToLower(CultureInfo.InvariantCulture);

            IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> bookmarkedModules = this.GetBookmarkedModules(PortalSettings.Current.PortalId, userId)
                .Where(kvp => kvp.Key.ToLower(CultureInfo.InvariantCulture).Contains(formattedSearchTerm));

            return bookmarkedModules;
        }

        public void SaveBookMark(int portalId, int userId, string bookmarkTitle, string bookmarkValue)
        {
            var ensuredBookmarkValue = bookmarkValue;
            if (bookmarkTitle == BookmarkModulesTitle)
            {
                ensuredBookmarkValue = this.EnsureBookmarkValue(portalId, ensuredBookmarkValue);
            }

            var personalizationController = new DotNetNuke.Services.Personalization.PersonalizationController();
            var personalization = personalizationController.LoadProfile(userId, portalId);
            personalization.Profile["ControlBar:" + bookmarkTitle + portalId] = ensuredBookmarkValue;
            personalization.IsModified = true;
            personalizationController.SaveProfile(personalization);
        }

        public string GetBookmarkCategory(int portalId)
        {
            var bookmarkCategory = PortalController.GetPortalSetting(BookmarkCategoryProperty, portalId, string.Empty);
            if (string.IsNullOrEmpty(bookmarkCategory))
            {
                PortalController.UpdatePortalSetting(portalId, BookmarkCategoryProperty, "Common");
                return "Common";
            }

            return bookmarkCategory;
        }

        public UpgradeIndicatorViewModel GetUpgradeIndicator(Version version, bool isLocal, bool isSecureConnection)
        {
            var imageUrl = Upgrade.UpgradeIndicator(version, isLocal, isSecureConnection);
            return !string.IsNullOrEmpty(imageUrl) ? this.GetDefaultUpgradeIndicator(imageUrl) : null;
        }

        public string GetControlBarLogoURL()
        {
            return HostController.Instance.GetString("ControlBarLogoURL", "~/admin/controlpanel/controlbarimages/dnnLogo.png");
        }

        public IEnumerable<MenuItemViewModel> GetCustomMenuItems()
        {
            var menuItemsExtensionPoints = this._mef.GetUserControlExtensionPoints("ControlBar", "CustomMenuItems");
            return menuItemsExtensionPoints.Select(this.GetMenuItemFromExtensionPoint);
        }

        protected override Func<IControlBarController> GetFactory()
        {
            return () => new ControlBarController();
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
                CssClass = string.Empty,
            };
        }

        private MenuItemViewModel GetMenuItemFromExtensionPoint(IUserControlExtensionPoint userControlExtensionPoint)
        {
            return new MenuItemViewModel
            {
                ID = Path.GetFileNameWithoutExtension(userControlExtensionPoint.UserControlSrc),
                Text = userControlExtensionPoint.Text,
                Source = userControlExtensionPoint.UserControlSrc,
                Order = userControlExtensionPoint.Order,
            };
        }

        private string EnsureBookmarkValue(int portalId, string bookmarkValue)
        {
            var bookmarkCategoryModules = this.GetCategoryDesktopModules(portalId, this.GetBookmarkCategory(portalId));
            var ensuredModules = bookmarkValue.Split(',').Where(desktopModuleId => bookmarkCategoryModules.All(m => m.Value.DesktopModuleID.ToString(CultureInfo.InvariantCulture) != desktopModuleId)).ToList();
            return string.Join(",", ensuredModules.Distinct());
        }

        private IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetBookmarkedModules(int portalId, int userId)
        {
            var personalizationController = new DotNetNuke.Services.Personalization.PersonalizationController();
            var personalization = personalizationController.LoadProfile(userId, portalId);
            var bookmarkItems = personalization.Profile["ControlBar:" + BookmarkModulesTitle + portalId];
            if (bookmarkItems == null)
            {
                return new List<KeyValuePair<string, PortalDesktopModuleInfo>>();
            }

            var bookmarkItemsKeys = bookmarkItems.ToString().Split(',').ToList();
            var bookmarkedModules = DesktopModuleController.GetPortalDesktopModules(PortalSettings.Current.PortalId)
                                        .Where(dm => bookmarkItemsKeys.Contains(dm.Value.DesktopModuleID.ToString(CultureInfo.InvariantCulture)));

            return bookmarkedModules;
        }
    }
}
