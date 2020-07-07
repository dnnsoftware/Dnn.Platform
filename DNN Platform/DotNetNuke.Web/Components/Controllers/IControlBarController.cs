// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Components.Controllers
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Web.Components.Controllers.Models;

    public interface IControlBarController
    {
        /// <summary>
        /// Get all desktop modules that belong to a category.
        /// </summary>
        /// <param name="portalId">Portal Id where modules are installed.</param>
        /// <param name="category">Catenory name.</param>
        /// <param name="searchTerm">Search term to filter modules.</param>
        /// <returns>A list with all matched desktop modules.</returns>
        IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetCategoryDesktopModules(
            int portalId,
            string category, string searchTerm = "");

        /// <summary>
        /// Get all desktop modules bookmark by user in Control Bar.
        /// </summary>
        /// <param name="portalId">Portal Id where modules are installed.</param>
        /// <param name="userId">User Id who has bookmarked the modules.</param>
        /// <param name="searchTerm">Search term to filter modules.</param>
        /// <returns>A list with all matched modules.</returns>
        IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetBookmarkedDesktopModules(int portalId, int userId,
            string searchTerm = "");

        /// <summary>
        /// Save a user bookmark.
        /// </summary>
        /// <param name="portalId">Portal Id where save the bookmark.</param>
        /// <param name="userId">User Id who is going to create the bookmark.</param>
        /// <param name="bookarkTitle">Title for the personalization setting.</param>
        /// <param name="bookmarkValue">Value for the personalization setting.</param>
        void SaveBookMark(int portalId, int userId, string bookarkTitle, string bookmarkValue);

        /// <summary>
        /// Get the category name where the bookmarked modules are shown.
        /// </summary>
        /// <param name="portalId">Portal Id where modules are installed.</param>
        /// <returns>The name of the bookmark category.</returns>
        string GetBookmarkCategory(int portalId);

        /// <summary>
        /// Returns the upgrade indicator model.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="isLocal"></param>
        /// <param name="isSecureConnection"></param>
        /// <returns>An instance of the view model UpgradeIndicator.</returns>
        UpgradeIndicatorViewModel GetUpgradeIndicator(Version version, bool isLocal, bool isSecureConnection);

        /// <summary>
        /// Get the current Logo URL shown in Control Bar.
        /// </summary>
        /// <returns>The URL of the Control Bar Logo.</returns>
        string GetControlBarLogoURL();

        /// <summary>
        /// Gets all custom Menu item available in the system.
        /// </summary>
        /// <returns>All custom Menu Items.</returns>
        IEnumerable<MenuItemViewModel> GetCustomMenuItems();
    }
}
