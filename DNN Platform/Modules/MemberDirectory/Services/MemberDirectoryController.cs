// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.MemberDirectory.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Web.Api;

    [SupportedModules("DotNetNuke.Modules.MemberDirectory")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class MemberDirectoryController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MemberDirectoryController));

        [HttpGet]
        public HttpResponseMessage AdvancedSearch(int userId, int groupId, int pageIndex, int pageSize, string searchTerm1, string searchTerm2, string searchTerm3, string searchTerm4)
        {
            try
            {
                if (userId < 0)
                {
                    userId = this.PortalSettings.UserId;
                }

                var searchField1 = GetSetting(this.ActiveModule.TabModuleSettings, "SearchField1", "DisplayName");
                var searchField2 = GetSetting(this.ActiveModule.TabModuleSettings, "SearchField2", "Email");
                var searchField3 = GetSetting(this.ActiveModule.TabModuleSettings, "SearchField3", "City");
                var searchField4 = GetSetting(this.ActiveModule.TabModuleSettings, "SearchField4", "Country");

                var propertyNames = string.Empty;
                var propertyValues = string.Empty;
                AddSearchTerm(ref propertyNames, ref propertyValues, searchField1, searchTerm1);
                AddSearchTerm(ref propertyNames, ref propertyValues, searchField2, searchTerm2);
                AddSearchTerm(ref propertyNames, ref propertyValues, searchField3, searchTerm3);
                AddSearchTerm(ref propertyNames, ref propertyValues, searchField4, searchTerm4);

                var members = this.GetUsers(userId, groupId, searchTerm1, pageIndex, pageSize, propertyNames, propertyValues);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetMembers(members));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage BasicSearch(int groupId, string searchTerm, int pageIndex, int pageSize)
        {
            try
            {
                var users = this.GetUsers(-1, groupId, string.IsNullOrEmpty(searchTerm) ? string.Empty : searchTerm.Trim(), pageIndex, pageSize, string.Empty, string.Empty);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetMembers(users));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetMember(int userId)
        {
            try
            {
                var users = new List<UserInfo>();
                var user = UserController.GetUserById(this.PortalSettings.PortalId, userId);
                users.Add(user);

                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetMembers(users));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetSuggestions(int groupId, string displayName)
        {
            try
            {
                var names = (from UserInfo user in this.GetUsers(-1, groupId, displayName.Trim(), 0, 10, string.Empty, string.Empty)
                             select new { label = user.DisplayName, value = user.DisplayName, userId = user.UserID })
                                .ToList();

                return this.Request.CreateResponse(HttpStatusCode.OK, names);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AcceptFriend(FriendDTO postData)
        {
            try
            {
                var friend = UserController.GetUserById(this.PortalSettings.PortalId, postData.FriendId);
                FriendsController.Instance.AcceptFriend(friend);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AddFriend(FriendDTO postData)
        {
            try
            {
                var friend = UserController.GetUserById(this.PortalSettings.PortalId, postData.FriendId);
                FriendsController.Instance.AddFriend(friend);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Follow(FollowDTO postData)
        {
            try
            {
                var follow = UserController.GetUserById(this.PortalSettings.PortalId, postData.FollowId);
                FollowersController.Instance.FollowUser(follow);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveFriend(FriendDTO postData)
        {
            try
            {
                var friend = UserController.GetUserById(this.PortalSettings.PortalId, postData.FriendId);
                FriendsController.Instance.DeleteFriend(friend);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UnFollow(FollowDTO postData)
        {
            try
            {
                var follow = UserController.GetUserById(this.PortalSettings.PortalId, postData.FollowId);
                FollowersController.Instance.UnFollowUser(follow);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static void AddSearchTerm(ref string propertyNames, ref string propertyValues, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
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

        private static string GetSetting(IDictionary settings, string key, string defaultValue)
        {
            var setting = defaultValue;
            if (settings[key] != null)
            {
                setting = Convert.ToString(settings[key]);
            }

            return setting;
        }

        private bool CanViewGroupMembers(int portalId, int groupId)
        {
            var group = RoleController.Instance.GetRole(portalId, r => r.RoleID == groupId);
            if (group == null)
            {
                return false;
            }

            var canView = (group.SecurityMode == SecurityMode.SecurityRole)
                               ? this.PortalSettings.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName)
                               : this.PortalSettings.UserInfo.IsInRole(group.RoleName);

            // if current user can view the group page and group is public, then should be able to view members.
            if (!canView)
            {
                canView = ModulePermissionController.CanViewModule(this.ActiveModule) && group.IsPublic;
            }

            return canView;
        }

        private IList<Member> GetMembers(IEnumerable<UserInfo> users)
        {
            return users.Select(user => new Member(user, this.PortalSettings)).ToList();
        }

        private IEnumerable<UserInfo> GetUsers(int userId, int groupId, string searchTerm, int pageIndex, int pageSize, string propertyNames, string propertyValues)
        {
            var portalId = this.PortalSettings.PortalId;
            var isAdmin = this.PortalSettings.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName);

            var filterBy = GetSetting(this.ActiveModule.ModuleSettings, "FilterBy", string.Empty);
            var filterValue = GetSetting(this.ActiveModule.ModuleSettings, "FilterValue", string.Empty);

            if (filterBy == "Group" && filterValue == "-1" && groupId > 0)
            {
                filterValue = groupId.ToString();
            }

            var sortField = GetSetting(this.ActiveModule.TabModuleSettings, "SortField", "DisplayName");

            // QuickFix DNN-6096. See: https://dnntracker.atlassian.net/browse/DNN-6096
            // Instead of changing the available SortFields, we'll use "UserId" as SortField if the TabModuleSetting SortField was CreatedOnDate.
            // This is because the GetUsersBasicSearch and GetUsersAdvancedSearch do not allow sorting on CreatedOnDate. Sorting on UserId however
            // has the same effect as sorting on CreatedOnDate.
            if (sortField.Equals("CreatedOnDate", StringComparison.InvariantCultureIgnoreCase))
            {
                sortField = "UserId";
            }

            var sortOrder = GetSetting(this.ActiveModule.TabModuleSettings, "SortOrder", "ASC");

            var excludeHostUsers = bool.Parse(GetSetting(this.ActiveModule.TabModuleSettings, "ExcludeHostUsers", "false"));
            var isBasicSearch = false;
            if (string.IsNullOrEmpty(propertyNames))
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
                        groupId = int.Parse(filterValue);
                    }

                    if (this.CanViewGroupMembers(portalId, groupId))
                    {
                        users = UserController.Instance.GetUsersAdvancedSearch(portalId, this.PortalSettings.UserId, userId,
                                                                                       groupId,
                                                                                       -1, isAdmin, pageIndex, pageSize,
                                                                                       sortField, sortOrder == "ASC",
                                                                                       propertyNames, propertyValues);
                    }
                    else
                    {
                        users = new List<UserInfo>();
                    }

                    break;
                case "Relationship":
                    users = UserController.Instance.GetUsersAdvancedSearch(portalId, this.PortalSettings.UserId, userId, -1,
                                                                           int.Parse(filterValue), isAdmin, pageIndex, pageSize,
                                                                           sortField, sortOrder == "ASC",
                                                                           propertyNames, propertyValues);
                    break;
                case "ProfileProperty":
                    var propertyValue = GetSetting(this.ActiveModule.ModuleSettings, "FilterPropertyValue", string.Empty);
                    AddSearchTerm(ref propertyNames, ref propertyValues, filterValue, propertyValue);

                    users = UserController.Instance.GetUsersAdvancedSearch(portalId, this.PortalSettings.UserId, userId, -1,
                                                                           -1, isAdmin, pageIndex, pageSize,
                                                                           sortField, sortOrder == "ASC",
                                                                           propertyNames, propertyValues);
                    break;
                default:
                    users = isBasicSearch ? UserController.Instance.GetUsersBasicSearch(this.PortalSettings.PortalId, pageIndex, pageSize,
                                                                           sortField, sortOrder == "ASC",
                                                                           "DisplayName", searchTerm)
                                                                           :
                                                                           UserController.Instance.GetUsersAdvancedSearch(portalId, this.PortalSettings.UserId, userId, -1,
                                                                               -1, isAdmin, pageIndex, pageSize,
                                                                               sortField, sortOrder == "ASC",
                                                                               propertyNames, propertyValues);
                    break;
            }

            if (excludeHostUsers)
            {
                return this.FilterExcludedUsers(users);
            }

            return users;
        }

        private IEnumerable<UserInfo> FilterExcludedUsers(IEnumerable<UserInfo> users)
        {
            return users.Where(u => !u.IsSuperUser).Select(u => u).ToList();
        }

        public class FollowDTO
        {
            public int FollowId { get; set; }
        }

        public class FriendDTO
        {
            public int FriendId { get; set; }
        }
    }
}
