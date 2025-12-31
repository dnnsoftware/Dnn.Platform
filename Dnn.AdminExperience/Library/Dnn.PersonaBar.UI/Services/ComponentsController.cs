// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.UI.Services.DTO;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api.Internal;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Services used for common components.</summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class ComponentsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ComponentsController));
        private readonly RoleProvider roleProvider;

        /// <summary>Initializes a new instance of the <see cref="ComponentsController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public ComponentsController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ComponentsController"/> class.</summary>
        /// <param name="roleProvider">The role provider.</param>
        public ComponentsController(RoleProvider roleProvider)
        {
            this.roleProvider = roleProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>();
        }

        /// <summary>Gets the local resource file path.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx");

        private static int UnauthUserRoleId => int.Parse(Globals.glbRoleUnauthUser, CultureInfo.InvariantCulture);

        /// <summary>An API action for getting the role groups for the portal.</summary>
        /// <param name="reload">Whether to clear the cache.</param>
        /// <returns>An HTTP response with a list of <see cref="RoleGroupDto"/> values.</returns>
        [HttpGet]
        public HttpResponseMessage GetRoleGroups(bool reload = false)
        {
            try
            {
                if (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) && !PagePermissionsAttributesHelper.HasTabPermission("VIEW"))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, Localization.GetString("UnauthorizedRequest", this.LocalResourcesFile));
                }

                if (reload)
                {
                    DataCache.RemoveCache(string.Format(DataCache.RoleGroupsCacheKey, this.PortalId));
                }

                var groups = RoleController.GetRoleGroups(this.roleProvider, this.PortalId)
                                .Cast<RoleGroupInfo>()
                                .Select(RoleGroupDto.FromRoleGroupInfo);

                return this.Request.CreateResponse(HttpStatusCode.OK, groups);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Error = ex.Message });
            }
        }

        /// <summary>An API action to get users to suggest for an autocomplete.</summary>
        /// <param name="keyword">The input.</param>
        /// <param name="count">The number of suggestions to return.</param>
        /// <returns>An HTTP response with a list of <see cref="SuggestionDto"/> values.</returns>
        [HttpGet]
        public HttpResponseMessage GetSuggestionUsers(string keyword, int count)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new List<SuggestionDto>());
                }

                var displayMatch = keyword + "%";
                var totalRecords = 0;
                var totalRecords2 = 0;
                var matchedUsers = UserController.GetUsersByDisplayName(this.PortalId, displayMatch, 0, count, ref totalRecords, false, false);
                matchedUsers.AddRange(UserController.GetUsersByUserName(this.PortalId, displayMatch, 0, count, ref totalRecords2, false, false));
                var finalUsers = matchedUsers
                    .Cast<UserInfo>()
                    .Where(x => x.Membership.Approved)
                    .Select(u => new SuggestionDto()
                    {
                        Value = u.UserID,
                        Label = $"{u.DisplayName}",
                    });

                return this.Request.CreateResponse(HttpStatusCode.OK, finalUsers.ToList().GroupBy(x => x.Value).Select(group => group.First()));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Error = ex.Message });
            }
        }

        /// <summary>An API action to get roles to suggest for an autocomplete.</summary>
        /// <param name="keyword">The input.</param>
        /// <param name="roleGroupId">The role group ID.</param>
        /// <param name="count">The number of suggestions to return.</param>
        /// <returns>An HTTP response with a list of <see cref="SuggestionDto"/> values.</returns>
        public HttpResponseMessage GetSuggestionRoles(string keyword, int roleGroupId, int count)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new List<SuggestionDto>());
                }

                var matchedRoles = RoleController.Instance.GetRoles(this.PortalId)
                                                    .Where(r => (roleGroupId == -2 || r.RoleGroupID == roleGroupId)
                                                          && r.RoleName.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) > -1
                                                          && r.Status == RoleStatus.Approved).ToList();

                if (roleGroupId <= Null.NullInteger
                        && Globals.glbRoleUnauthUserName.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    matchedRoles.Add(new RoleInfo { RoleID = UnauthUserRoleId, RoleName = Globals.glbRoleUnauthUserName });
                }

                var roleList = new SortedList<string, int>();
                foreach (var role in matchedRoles)
                {
                    roleList[Localization.LocalizeRole(role.RoleName)] = role.RoleID;
                }

                var data = roleList.Select(r => new SuggestionDto()
                {
                    Value = r.Value,
                    Label = r.Key,
                });

                return this.Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Error = ex.Message });
            }
        }
    }
}
