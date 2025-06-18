// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System.Collections.Generic;

    /// <summary>Provides access to manage users.</summary>
    public interface IUserController
    {
        /// <summary>Gets the current logged-in user's information.</summary>
        /// <returns>The logged-in user's <see cref="UserInfo"/> object or an empty <see cref="UserInfo"/> if the current user is not logged in.</returns>
        UserInfo GetCurrentUserInfo();

        /// <summary>Gets a specific user information.</summary>
        /// <param name="portalId">The site (portal) id on which to get the user from.</param>
        /// <param name="userId">The user id to get the information for.</param>
        /// <returns>A <see cref="UserInfo"/> instance or <see langword="null" />.</returns>
        UserInfo GetUser(int portalId, int userId);

        /// <summary>Get a user based on their display name and portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="displayName">The display name.</param>
        /// <returns>A <see cref="UserInfo"/> instance or <see langword="null" />.</returns>
        UserInfo GetUserByDisplayname(int portalId, string displayName);

        /// <summary>Retrieves a User from the DataStore.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="userId">The ID of the user being retrieved from the Data Store.</param>
        /// <returns>A <see cref="UserInfo"/> instance or <see langword="null" />.</returns>
        UserInfo GetUserById(int portalId, int userId);

        /// <summary>Searches for users via user fields and profile properties.</summary>
        /// <param name="portalId">The portal ID in which to search for users.</param>
        /// <param name="userId">The ID of the user searching. This is used to determine access to view profile properties. Can be <see cref="Null.NullInteger"/> to get anonymous results.</param>
        /// <param name="filterUserId">The ID of a user with whom the resulting user must have a relationship. Only applies if <paramref name="relationTypeId"/> is supplied. Can be <see cref="Null.NullInteger"/> for no filter.</param>
        /// <param name="filterRoleId">The ID of a role which the resulting user must hold. Can be <see cref="Null.NullInteger"/> for no filter.</param>
        /// <param name="relationTypeId">The ID of a relationship type by which the resulting user and the <paramref name="filterUserId"/> are related. Can be <see cref="Null.NullInteger"/> for no filter.</param>
        /// <param name="isAdmin">Whether the requesting user is an admin (i.e. whether they can view admin-only profile properties).</param>
        /// <param name="pageIndex">The 0-based page index.</param>
        /// <param name="pageSize">The number of results to return in each page.</param>
        /// <param name="sortColumn">The name of the column/property to sort by. Sorts by <see cref="IUserInfo.UserID"/> if not supplied.</param>
        /// <param name="sortAscending">Whether the sort is ascending or descending.</param>
        /// <param name="propertyNames">A comma-delimited list of property names (e.g. <c>"Username,Street1"</c>).</param>
        /// <param name="propertyValues">A comma-delimited list of property values (e.g. <c>"jane,123 Street"</c>).</param>
        /// <returns>A list of <see cref="UserInfo"/> instances.</returns>
        /// <remarks>All property values must match for a result to be included (i.e. this is an <c>AND</c> search).</remarks>
        IList<UserInfo> GetUsersAdvancedSearch(
            int portalId,
            int userId,
            int filterUserId,
            int filterRoleId,
            int relationTypeId,
            bool isAdmin,
            int pageIndex,
            int pageSize,
            string sortColumn,
            bool sortAscending,
            string propertyNames,
            string propertyValues);

        /// <summary>Searches for users via a user property.</summary>
        /// <param name="portalId">The portal ID in which to search for users.</param>
        /// <param name="pageIndex">The 0-based page index.</param>
        /// <param name="pageSize">The number of results to return in each page.</param>
        /// <param name="sortColumn">The name of the column/property to sort by. Sorts by <see cref="IUserInfo.UserID"/> if not supplied.</param>
        /// <param name="sortAscending">Whether the sort is ascending or descending.</param>
        /// <param name="propertyName">A property name (i.e. a column from <c>vw_Users</c>, for example <c>"Username"</c>, <c>"DisplayName"</c> or <c>"Email"</c>).</param>
        /// <param name="propertyValue">The value to search by, results will contain this text in the property.</param>
        /// <returns>A list of <see cref="UserInfo"/> instances.</returns>
        IList<UserInfo> GetUsersBasicSearch(
            int portalId,
            int pageIndex,
            int pageSize,
            string sortColumn,
            bool sortAscending,
            string propertyName,
            string propertyValue);

        /// <summary>Gets the (relative) URL to the given user's profile picture in the current portal.</summary>
        /// <param name="userId">User ID.</param>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        /// <returns>Relative URL, e.g. <c>/DnnImageHandler.ashx?userid=1&amp;h=32&amp;w=32</c>, considering child portal.</returns>
        /// <example>
        /// Usage: ascx:
        /// <code>&lt;asp:Image ID="avatar" runat="server" CssClass="SkinObject" /&gt;</code>
        /// code behind:
        /// <code>avatar.ImageUrl = UserController.Instance.GetUserProfilePictureUrl(userInfo.UserID, 32, 32)</code>
        /// </example>
        string GetUserProfilePictureUrl(int userId, int width, int height);

        /// <summary>Gets the (relative) URL to the given user's profile picture in the given portal.</summary>
        /// <param name="portalId">Portal ID.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        /// <returns>Relative URL, e.g. <c>/DnnImageHandler.ashx?userid=1&amp;h=32&amp;w=32</c>, considering child portal.</returns>
        /// <remarks>
        /// IMPORTANT NOTE: this overloaded method does not depend on the current portal setting so it can be used
        /// in background threads or scheduler jobs.
        /// </remarks>
        string GetUserProfilePictureUrl(int portalId, int userId, int width, int height);

        /// <summary>Check if <paramref name="userName"/> is a valid username.</summary>
        /// <remarks>
        /// Checks the <paramref name="userName"/> against the following rules:
        /// <list type="bullet">
        /// <item><description>Invalid characters (based on a host setting or <see cref="Globals.USERNAME_UNALLOWED_ASCII"/>).</description></item>
        /// <item><description>Length check for 5 chars</description></item>
        /// <item><description>It must not start or end with space</description></item>
        /// </list>
        /// </remarks>
        /// <param name="userName">The username to check.</param>
        /// <returns><see langword="true"/> is the <paramref name="userName"/> is valid, <see langword="false"/> if invalid.</returns>
        bool IsValidUserName(string userName);
    }
}
