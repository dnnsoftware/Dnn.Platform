#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
#region Usings

using System;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Search.Entities;

#endregion

namespace DotNetNuke.Services.Search.Controllers
{
    /// <summary>
    /// Search Result Controller for Tab Indexer
    /// </summary>
    /// <remarks></remarks>
    [Serializable]
    public class UserResultController : BaseResultController
    {
        #region Private Properties

        private PortalSettings PortalSettings
        {
            get { return PortalController.GetCurrentPortalSettings(); }
        }

        #endregion

        #region Abstract Class Implmentation

        public override bool HasViewPermission(SearchResult searchResult)
        {
            var userId = GetUserId(searchResult);
            if (userId == Null.NullInteger)
            {
                return false;
            }

            var userInSearchResult = UserController.GetUserById(PortalSettings.PortalId, userId);
            if (userInSearchResult == null || userInSearchResult.IsDeleted)
            {
                return false;
            }

            if (searchResult.UniqueKey.Contains("adminonly"))
            {
                return UserController.GetCurrentUserInfo().IsSuperUser || UserController.GetCurrentUserInfo().IsInRole("Administrators");
            }
            
            if (searchResult.UniqueKey.Contains("friendsandgroups"))
            {
                var extendedVisibility = searchResult.UniqueKey.IndexOf("_") != searchResult.UniqueKey.LastIndexOf("_")
                                             ? searchResult.UniqueKey.Split('_')[2]
                                             : string.Empty;
                return HasSocialReplationship(userInSearchResult, UserController.GetCurrentUserInfo(), extendedVisibility);
            }

            if (searchResult.UniqueKey.Contains("membersonly"))
            {
                return UserController.GetCurrentUserInfo().UserID != Null.NullInteger;
            }

            if (searchResult.UniqueKey.Contains("allusers"))
            {
                return true;
            }

            return false;
        }

        public override string GetDocUrl(SearchResult searchResult)
        {
            var url = Globals.NavigateURL(PortalSettings.UserTabId, string.Empty, "userid=" + GetUserId(searchResult));
            return url;
        }

        #endregion

        #region Private Methods

        private bool HasSocialReplationship(UserInfo targetUser, UserInfo accessingUser, string extendedVisibility)
        {
            if (string.IsNullOrEmpty(extendedVisibility))
            {
                return false;
            }

            var profileVisibility = new ProfileVisibility(PortalSettings.PortalId, extendedVisibility);

            var isVisible = accessingUser.UserID == targetUser.UserID;
            if (!isVisible)
            {
                //Relationships
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
                                                                              && userRelationship.Status == RelationshipStatus.Accepted)
                                                                      );
                            break;
                    }

                    if (isVisible)
                    {
                        break;
                    }
                }
                //Groups/Roles
                if (profileVisibility.RoleVisibilities.Any(role => accessingUser.IsInRole(role.RoleName)))
                {
                    isVisible = true;
                }
            }

            return isVisible;
        }

        private int GetUserId(SearchResult searchResult)
        {
            var match = Regex.Match(searchResult.UniqueKey, "^(\\d+)_");
            if (!match.Success)
            {
                return Null.NullInteger;
            }

            return Convert.ToInt32(match.Groups[1].Value);
        }
        #endregion
    }
}