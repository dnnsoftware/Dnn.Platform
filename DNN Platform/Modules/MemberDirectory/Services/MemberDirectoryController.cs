#region Copyright

// 
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Users.Social;

namespace DotNetNuke.Modules.MemberDirectory.Services
{
    [SupportedModules("DotNetNuke.Modules.MemberDirectory")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class MemberDirectoryController : DnnApiController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (MemberDirectoryController));
        #region Private Methods

        private static void AddSearchTerm(ref string propertyNames, ref string propertyValues, string name, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                propertyNames += name + ",";

                if (name.Equals("Country", StringComparison.InvariantCultureIgnoreCase) ||
                    name.Equals("Region", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = GetMatchedListEntryIds(name, value);
                }
                propertyValues += value + ",";
            }
        }

        private static string GetMatchedListEntryIds(string name, string value)
        {
            var listEntries = new ListController().GetListEntryInfoItems(name)
                .Where(i => i.Text.StartsWith(value, StringComparison.InvariantCultureIgnoreCase)
                            || i.TextNonLocalized.StartsWith(value, StringComparison.InvariantCultureIgnoreCase)
                            || i.Value.StartsWith(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(i => i.EntryID);

            return $"${string.Join("$", listEntries)}$";
        }

        private bool CanViewGroupMembers(int portalId, int groupId)
        {
            var group = RoleController.Instance.GetRole(portalId, r => r.RoleID == groupId);
            if(group == null)
            {
                return false;
            }

            var canView = (group.SecurityMode == SecurityMode.SecurityRole)
                               ? (PortalSettings.UserInfo.IsInRole(PortalSettings.AdministratorRoleName))
                               : (PortalSettings.UserInfo.IsInRole(group.RoleName));

			//if current user can view the group page and group is public, then should be able to view members.
			if (!canView)
			{
				canView = ModulePermissionController.CanViewModule(ActiveModule) && group.IsPublic;
			}
            return canView;
        }

        private IList<Member> GetMembers(IEnumerable<UserInfo> users)
        {
            return users.Select(user => new Member(user, PortalSettings)).ToList();
        }

        private static string GetSetting(IDictionary settings, string key, string defaultValue)
        {
            var setting = defaultValue;
            if (settings[key] != null)
            {
                setting = Convert.ToString(settings[key]);
            }
            return setting;
        }

        private IEnumerable<UserInfo> GetUsers(int userId, int groupId, string searchTerm, int pageIndex, int pageSize, string propertyNames, string propertyValues)
        {
            var portalId = PortalSettings.PortalId;            
            var isAdmin = PortalSettings.UserInfo.IsInRole(PortalSettings.AdministratorRoleName);

            var filterBy = GetSetting(ActiveModule.ModuleSettings, "FilterBy", String.Empty);
            var filterValue = GetSetting(ActiveModule.ModuleSettings, "FilterValue", String.Empty);

            if (filterBy == "Group" && filterValue == "-1" && groupId > 0)
            {
                filterValue = groupId.ToString();
            }

            var sortField = GetSetting(ActiveModule.TabModuleSettings, "SortField", "DisplayName");

            // QuickFix DNN-6096. See: https://dnntracker.atlassian.net/browse/DNN-6096
            // Instead of changing the available SortFields, we'll use "UserId" as SortField if the TabModuleSetting SortField was CreatedOnDate.
            // This is because the GetUsersBasicSearch and GetUsersAdvancedSearch do not allow sorting on CreatedOnDate. Sorting on UserId however
            // has the same effect as sorting on CreatedOnDate.
            if (sortField.Equals("CreatedOnDate", StringComparison.InvariantCultureIgnoreCase))
            {
                sortField = "UserId";
            }

            var sortOrder = GetSetting(ActiveModule.TabModuleSettings, "SortOrder", "ASC");

            var excludeHostUsers = Boolean.Parse(GetSetting(ActiveModule.TabModuleSettings, "ExcludeHostUsers", "false"));
            var isBasicSearch = false;
            if (String.IsNullOrEmpty(propertyNames))
            {
                isBasicSearch = true;
                AddSearchTerm(ref propertyNames, ref propertyValues, "DisplayName", searchTerm);
            }

            IList<UserInfo> users;
            switch (filterBy)
            {
                case "User":
                    users = new List<UserInfo> { UserController.GetUserById(portalId, userId) };
                    break;
                case "Group":
                    if (groupId == -1)
                    {
                        groupId = Int32.Parse(filterValue);
                    }
                    if (CanViewGroupMembers(portalId, groupId))
                    {
                        users = UserController.Instance.GetUsersAdvancedSearch(portalId, userId, -1,
                                                                                       groupId,
                                                                                       -1, isAdmin, pageIndex, pageSize,
                                                                                       sortField, (sortOrder == "ASC"),
                                                                                       propertyNames, propertyValues);
                    }
                    else
                    {
                        users = new List<UserInfo>();
                    }
                    break;
                case "Relationship":
                    users = UserController.Instance.GetUsersAdvancedSearch(portalId, userId, userId, -1,
                                                                           Int32.Parse(filterValue), isAdmin, pageIndex, pageSize,
                                                                           sortField, (sortOrder == "ASC"),
                                                                           propertyNames, propertyValues);
                    break;
                case "ProfileProperty":
                    var propertyValue = GetSetting(ActiveModule.ModuleSettings, "FilterPropertyValue", String.Empty);
                    AddSearchTerm(ref propertyNames, ref propertyValues, filterValue, propertyValue);

                    users = UserController.Instance.GetUsersAdvancedSearch(portalId, userId, -1, -1,
                                                                           -1, isAdmin, pageIndex, pageSize,
                                                                           sortField, (sortOrder == "ASC"),
                                                                           propertyNames, propertyValues);
                    break;
                default:
                    users = isBasicSearch ? UserController.Instance.GetUsersBasicSearch(PortalSettings.PortalId, pageIndex, pageSize,
                                                                           sortField, (sortOrder == "ASC"),
                                                                           "DisplayName", searchTerm)
                                                                           :
                                                                           UserController.Instance.GetUsersAdvancedSearch(portalId, PortalSettings.UserId, -1, -1,
                                                                               -1, isAdmin, pageIndex, pageSize,
                                                                               sortField, (sortOrder == "ASC"),
                                                                               propertyNames, propertyValues);
                    break;
            }
            if (excludeHostUsers)
            {                
                return FilterExcludedUsers(users);
            }
            return users;
        }

        private IEnumerable<UserInfo> FilterExcludedUsers(IEnumerable<UserInfo> users)
        {
            return users.Where(u => !u.IsSuperUser).Select(u => u).ToList();
        }

        #endregion

        #region Public Methods

        [HttpGet]
        public HttpResponseMessage AdvancedSearch(int userId, int groupId, int pageIndex, int pageSize, string searchTerm1, string searchTerm2, string searchTerm3, string searchTerm4)
        {
            try
            {
                if (userId < 0) userId = PortalSettings.UserId;
                
                var searchField1 = GetSetting(ActiveModule.TabModuleSettings, "SearchField1", "DisplayName");
                var searchField2 = GetSetting(ActiveModule.TabModuleSettings, "SearchField2", "Email");
                var searchField3 = GetSetting(ActiveModule.TabModuleSettings, "SearchField3", "City");
                var searchField4 = GetSetting(ActiveModule.TabModuleSettings, "SearchField4", "Country");

                var propertyNames = "";
                var propertyValues = "";
                AddSearchTerm(ref propertyNames, ref propertyValues, searchField1, searchTerm1);
                AddSearchTerm(ref propertyNames, ref propertyValues, searchField2, searchTerm2);
                AddSearchTerm(ref propertyNames, ref propertyValues, searchField3, searchTerm3);
                AddSearchTerm(ref propertyNames, ref propertyValues, searchField4, searchTerm4);

                return Request.CreateResponse(HttpStatusCode.OK, GetMembers(
                                                                    GetUsers(userId, groupId, searchTerm1, pageIndex, pageSize, propertyNames, propertyValues)));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage BasicSearch(int groupId, string searchTerm, int pageIndex, int pageSize)
        {
            try
            {
                var users = GetUsers(PortalSettings.UserId, groupId, string.IsNullOrEmpty(searchTerm) ? string.Empty : searchTerm.Trim(), pageIndex, pageSize, "", "");
                return Request.CreateResponse(HttpStatusCode.OK, GetMembers(users));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetMember(int userId)
        {
            try
            {
                var users = new List<UserInfo>();
                var user = UserController.GetUserById(PortalSettings.PortalId, userId);
                users.Add(user);

                return Request.CreateResponse(HttpStatusCode.OK, GetMembers(users));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetSuggestions(int groupId, string displayName)
        {
            try
            {
                var names = (from UserInfo user in GetUsers(PortalSettings.UserId, groupId, displayName.Trim(), 0, 10, "", "")
                             select new { label = user.DisplayName, value = user.DisplayName, userId = user.UserID })
                                .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, names);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AcceptFriend(FriendDTO postData)
        {
            try
            {
                var friend = UserController.GetUserById(PortalSettings.PortalId, postData.FriendId);
                FriendsController.Instance.AcceptFriend(friend);
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AddFriend(FriendDTO postData)
        {
            try
            {
                var friend = UserController.GetUserById(PortalSettings.PortalId, postData.FriendId);
                FriendsController.Instance.AddFriend(friend);
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Follow(FollowDTO postData)
        {
            try
            {
                var follow = UserController.GetUserById(PortalSettings.PortalId, postData.FollowId);
                FollowersController.Instance.FollowUser(follow);
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveFriend(FriendDTO postData)
        {
            try
            {
                var friend = UserController.GetUserById(PortalSettings.PortalId, postData.FriendId);
                FriendsController.Instance.DeleteFriend(friend);
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UnFollow(FollowDTO postData)
        {
            try
            {
                var follow = UserController.GetUserById(PortalSettings.PortalId, postData.FollowId);
                FollowersController.Instance.UnFollowUser(follow);
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region DTO

        public class FollowDTO
        {
            public int FollowId { get; set; }
        }

        public class FriendDTO
        {
            public int FriendId { get; set; }
        }

        #endregion
    }
}