// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System.Collections.Generic;

    public interface IUserController
    {
        UserInfo GetCurrentUserInfo();

        UserInfo GetUser(int portalId, int userId);

        /// <summary>
        /// Get a user based on their display name and portal.
        /// </summary>
        /// <param name="portalId">the portalid.</param>
        /// <param name="displayName">the displayname.</param>
        /// <returns>The User as a UserInfo object.</returns>
        UserInfo GetUserByDisplayname(int portalId, string displayName);

        /// <summary>
        /// GetUser retrieves a User from the DataStore.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal.</param>
        /// <param name="userId">The Id of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object.</returns>
        UserInfo GetUserById(int portalId, int userId);

        /// <summary>
        /// Get a filtered list of users based on a number of criteria and filters - utilised for advanced searches.
        /// </summary>
        /// <param name="portalId">the portalid of the user(s).</param>
        /// <param name="userId">the userid accessing - for determining correct visibility permissions.</param>
        /// <param name="filterUserId">for filtering relationships on.</param>
        /// <param name="filterRoleId">for filtering by roles.</param>
        /// <param name="relationTypeId">for filtering by relationships.</param>
        /// <param name="isAdmin">determines visibility.</param>
        /// <param name="pageIndex">0 based page index.</param>
        /// <param name="pageSize">page size.</param>
        /// <param name="sortColumn">sort field.</param>
        /// <param name="sortAscending">sort flag indicating whether sort is asc or desc.</param>
        /// <param name="propertyNames">list of property names to filter.</param>
        /// <param name="propertyValues">list of property values to filter.</param>
        /// <returns>Users as a list of UserInfo objects.</returns>
        IList<UserInfo> GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int filterRoleId, int relationTypeId,
            bool isAdmin, int pageIndex, int pageSize, string sortColumn,
            bool sortAscending, string propertyNames, string propertyValues);

        /// <summary>
        /// Get a filtered list of users based on a set of simple filters - utilised for basic searches.
        /// </summary>
        /// <param name="portalId">the portalid of the user(s).</param>
        /// <param name="pageIndex">0 based page index.</param>
        /// <param name="pageSize">page size.</param>
        /// <param name="sortColumn">sort field.</param>
        /// <param name="sortAscending">sort flag indicating whether sort is asc or desc.</param>
        /// <param name="propertyName">list of property names to filter.</param>
        /// <param name="propertyValue">list of property values to filter.</param>
        /// <returns>Users as a list of UserInfo objects.</returns>
        IList<UserInfo> GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn,
            bool sortAscending, string propertyName, string propertyValue);

        /// <summary>
        /// Return User Profile Picture relative Url.
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <param name="width">Width in pixel.</param>
        /// <param name="height">Height in pixel.</param>
        /// <returns>Relative url,  e.g. /DnnImageHandler.ashx?userid=1&amp;h=32&amp;w=32 considering child portal.</returns>
        /// <remarks>Usage: ascx - &lt;asp:Image ID="avatar" runat="server" CssClass="SkinObject" /&gt;
        /// code behind - avatar.ImageUrl = UserController.Instance.GetUserProfilePictureUrl(userInfo.UserID, 32, 32).
        /// </remarks>
        string GetUserProfilePictureUrl(int userId, int width, int height);

        /// <summary>
        /// Return User Profile Picture relative Url.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="userId">User Id.</param>
        /// <param name="width">Width in pixel.</param>
        /// <param name="height">Height in pixel.</param>
        /// <returns>Relative url, e.g. /DnnImageHandler.ashx?userid=1&amp;h=32&amp;w=32 considering child portal.</returns>
        /// <remarks>IMPORTANT NOTE: this overloaded method does not depend on the current portal setting so it can be used
        /// in background threads or scheduler jobs.
        /// </remarks>
        string GetUserProfilePictureUrl(int portalId, int userId, int width, int height);

        /// <summary>
        /// Check username parameter against :
        /// Invalid charaters,
        /// length check for 5 chars,
        /// for space between username.
        /// </summary>
        /// <param name="userName">UserName as string.</param>
        /// <returns>true/false.</returns>
        bool IsValidUserName(string userName);
    }
}
