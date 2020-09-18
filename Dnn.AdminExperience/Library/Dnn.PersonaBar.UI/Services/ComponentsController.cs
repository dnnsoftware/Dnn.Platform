﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Common;

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.UI.Services.DTO;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    /// <summary>
    /// Services used for common components.
    /// </summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class ComponentsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ComponentsController));

        public string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx");

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

                var groups = RoleController.GetRoleGroups(this.PortalId)
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
                var matchedUsers = UserController.GetUsersByDisplayName(this.PortalId, displayMatch, 0, count,
                    ref totalRecords, false, false);
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

        public HttpResponseMessage GetSuggestionRoles(string keyword, int roleGroupId, int count)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new List<SuggestionDto>());
                }

                var portalRoles = RoleController.Instance.GetRoles(this.PortalId);
                

                var matchedRoles = portalRoles.Where(r => (roleGroupId == -2 || r.RoleGroupID == roleGroupId)
                                                          && r.RoleName.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger
                                                          && r.SecurityMode != SecurityMode.SocialGroup
                                                          && r.Status == RoleStatus.Approved).ToList();

                if (roleGroupId <= Null.NullInteger
                        && Globals.glbRoleUnauthUserName.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger)
                {
                    matchedRoles.Add(new RoleInfo { RoleID = int.Parse(Globals.glbRoleUnauthUser), RoleName = Globals.glbRoleUnauthUserName });
                }

                var data = matchedRoles.OrderBy(r => r.RoleName).Select(r => new SuggestionDto()
                    {
                        Value = r.RoleID,
                        Label = r.RoleName
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
