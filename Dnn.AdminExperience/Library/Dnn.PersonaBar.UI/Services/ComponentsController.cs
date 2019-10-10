﻿#region Copyright
// 
// DotNetNuke® - https://www.dnnsoftware.com
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.UI.Services.DTO;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;

namespace Dnn.PersonaBar.UI.Services
{
    /// <summary>
    /// Services used for common components.
    /// </summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class ComponentsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ComponentsController));
        public string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx");

        #region API in Admin Level

        [HttpGet]
        public HttpResponseMessage GetRoleGroups(bool reload = false)
        {
            try
            {
                if(!UserInfo.IsInRole(PortalSettings.AdministratorRoleName) && !PagePermissionsAttributesHelper.HasTabPermission("VIEW"))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, Localization.GetString("UnauthorizedRequest", LocalResourcesFile));
                }

                if (reload)
                {
                    DataCache.RemoveCache(string.Format(DataCache.RoleGroupsCacheKey, PortalId));
                }

                var groups = RoleController.GetRoleGroups(PortalId)
                                .Cast<RoleGroupInfo>()
                                .Select(RoleGroupDto.FromRoleGroupInfo);

                return Request.CreateResponse(HttpStatusCode.OK, groups);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Error = ex.Message });
            }
        }

        #endregion

        #region API in Regular Level

        [HttpGet]
        public HttpResponseMessage GetSuggestionUsers(string keyword, int count)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new List<SuggestionDto>());
                }

                var displayMatch = keyword + "%";
                var totalRecords = 0;
                var totalRecords2 = 0;
                var matchedUsers = UserController.GetUsersByDisplayName(PortalId, displayMatch, 0, count,
                    ref totalRecords, false, false);
                matchedUsers.AddRange(UserController.GetUsersByUserName(PortalId, displayMatch, 0, count, ref totalRecords2, false, false));
                var finalUsers = matchedUsers
                    .Cast<UserInfo>()
                    .Where(x=>x.Membership.Approved)
                    .Select(u => new SuggestionDto()
                    {
                        Value = u.UserID,
                        Label = $"{u.DisplayName}"
                    });

                return Request.CreateResponse(HttpStatusCode.OK, finalUsers.ToList().GroupBy(x => x.Value).Select(group => group.First()));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Error = ex.Message });
            }

        }

        public HttpResponseMessage GetSuggestionRoles(string keyword, int roleGroupId, int count)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new List<SuggestionDto>());
                }

                var matchedRoles = RoleController.Instance.GetRoles(PortalId)
                    .Where(r => (roleGroupId == -2 || r.RoleGroupID == roleGroupId)
                                && r.RoleName.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) > -1
                                   && r.Status == RoleStatus.Approved)
                    .Select(r => new SuggestionDto()
                    {
                        Value = r.RoleID,
                        Label = r.RoleName
                    });

                return Request.CreateResponse(HttpStatusCode.OK, matchedRoles);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Error = ex.Message });
            }

        }

        #endregion
    }
}