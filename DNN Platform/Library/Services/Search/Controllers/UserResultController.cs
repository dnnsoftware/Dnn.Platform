// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Controllers
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Services.Search.Entities;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// Search Result Controller for Tab Indexer.
    /// </summary>
    /// <remarks></remarks>
    [Serializable]
    public class UserResultController : BaseResultController
    {
        private const string LocalizedResxFile = "~/DesktopModules/Admin/SearchResults/App_LocalResources/SearchableModules.resx";

        private static readonly Regex SearchResultMatchRegex = new Regex(@"^(\d+)_", RegexOptions.Compiled);

        public override string LocalizedSearchTypeName => Localization.GetString("Crawler_user", LocalizedResxFile);

        private PortalSettings PortalSettings
        {
            get { return PortalController.Instance.GetCurrentPortalSettings(); }
        }

        public override bool HasViewPermission(SearchResult searchResult)
        {
            var userId = GetUserId(searchResult);
            if (userId == Null.NullInteger)
            {
                return false;
            }

            var userInSearchResult = UserController.GetUserById(this.PortalSettings.PortalId, userId);
            if (userInSearchResult == null || userInSearchResult.IsDeleted)
            {
                return false;
            }

            if (searchResult.UniqueKey.Contains("adminonly"))
            {
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                return currentUser.IsSuperUser
                        || currentUser.IsInRole("Administrators")
                        || currentUser.UserID == userId;
            }

            if (searchResult.UniqueKey.Contains("friendsandgroups"))
            {
                var extendedVisibility = searchResult.UniqueKey.IndexOf("_") != searchResult.UniqueKey.LastIndexOf("_")
                                             ? searchResult.UniqueKey.Split('_')[2]
                                             : string.Empty;
                return this.HasSocialReplationship(userInSearchResult, UserController.Instance.GetCurrentUserInfo(), extendedVisibility);
            }

            if (searchResult.UniqueKey.Contains("membersonly"))
            {
                return UserController.Instance.GetCurrentUserInfo().UserID != Null.NullInteger;
            }

            if (searchResult.UniqueKey.Contains("allusers"))
            {
                var scopeForRoles =
                    PortalController.GetPortalSetting("SearchResult_ScopeForRoles", searchResult.PortalId, string.Empty)
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (scopeForRoles.Count > 0)
                {
                    if (userInSearchResult.IsSuperUser)
                    {
                        return scopeForRoles.Contains("Superusers");
                    }

                    return scopeForRoles.Any(i => userInSearchResult.IsInRole(i));
                }

                return true;
            }

            return false;
        }

        public override string GetDocUrl(SearchResult searchResult)
        {
            var url = TestableGlobals.Instance.NavigateURL(this.PortalSettings.UserTabId, string.Empty, "userid=" + GetUserId(searchResult));
            return url;
        }

        private static int GetUserId(SearchDocumentToDelete searchResult)
        {
            var match = SearchResultMatchRegex.Match(searchResult.UniqueKey);
            return match.Success ? Convert.ToInt32(match.Groups[1].Value) : Null.NullInteger;
        }

        private bool HasSocialReplationship(UserInfo targetUser, UserInfo accessingUser, string extendedVisibility)
        {
            if (string.IsNullOrEmpty(extendedVisibility))
            {
                return false;
            }

            var profileVisibility = new ProfileVisibility(this.PortalSettings.PortalId, extendedVisibility);

            var isVisible = accessingUser.UserID == targetUser.UserID;
            if (!isVisible)
            {
                // Relationships
                foreach (var relationship in profileVisibility.RelationshipVisibilities)
                {
                    switch (relationship.RelationshipTypeId)
                    {
                        case (int)DefaultRelationshipTypes.Followers:
                            isVisible = targetUser.Social.Following != null && targetUser.Social.Following.Status == RelationshipStatus.Accepted;
                            break;
                        case (int)DefaultRelationshipTypes.Friends:
                            isVisible = targetUser.Social.Friend != null && targetUser.Social.Friend.Status == RelationshipStatus.Accepted;
                            break;
                        default:
                            isVisible = targetUser.Social.UserRelationships.Any(userRelationship =>
                                                                          (userRelationship.RelationshipId == relationship.RelationshipId
                                                                              && accessingUser.UserID == userRelationship.RelatedUserId
                                                                              && userRelationship.Status == RelationshipStatus.Accepted));
                            break;
                    }

                    if (isVisible)
                    {
                        break;
                    }
                }

                // Groups/Roles
                if (profileVisibility.RoleVisibilities.Any(role => accessingUser.IsInRole(role.RoleName)))
                {
                    isVisible = true;
                }
            }

            return isVisible;
        }
    }
}
