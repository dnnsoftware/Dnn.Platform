// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Web.Api;

    [DnnAuthorize]
    public class MessagingServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MessagingServiceController));

        [HttpGet]
        public HttpResponseMessage WaitTimeForNextMessage()
        {
            try
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", Value = InternalMessagingController.Instance.WaitTimeForNextMessage(this.UserInfo) });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public HttpResponseMessage Create(CreateDTO postData)
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.PortalSettings.PortalId);
                var roleIdsList = string.IsNullOrEmpty(postData.RoleIds) ? null : postData.RoleIds.FromJson<IList<int>>();
                var userIdsList = string.IsNullOrEmpty(postData.UserIds) ? null : postData.UserIds.FromJson<IList<int>>();
                var fileIdsList = string.IsNullOrEmpty(postData.FileIds) ? null : postData.FileIds.FromJson<IList<int>>();

                var roles = roleIdsList != null && roleIdsList.Count > 0
                    ? roleIdsList.Select(id => RoleController.Instance.GetRole(portalId, r => r.RoleID == id)).Where(role => role != null).ToList()
                    : null;

                List<UserInfo> users = null;
                if (userIdsList != null)
                {
                    users = userIdsList.Select(id => UserController.Instance.GetUser(portalId, id)).Where(user => user != null).ToList();
                }

                var body = HttpUtility.UrlDecode(postData.Body);
                body = PortalSecurity.Instance.InputFilter(body, PortalSecurity.FilterFlag.NoMarkup);
                var message = new Message { Subject = HttpUtility.UrlDecode(postData.Subject), Body = body };
                MessagingController.Instance.SendMessage(message, roles, users, fileIdsList);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", Value = message.MessageID });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage Search(string q)
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.PortalSettings.PortalId);
                var isAdmin = this.UserInfo.IsSuperUser || this.UserInfo.IsInRole("Administrators");
                const int numResults = 10;

                // GetUsersAdvancedSearch doesn't accept a comma or a single quote in the query so we have to remove them for now. See issue 20224.
                q = q.Replace(",", string.Empty).Replace("'", string.Empty);
                if (q.Length == 0)
                {
                    return this.Request.CreateResponse<object>(HttpStatusCode.OK, null);
                }

                var results = UserController.Instance.GetUsersBasicSearch(portalId, 0, numResults, "DisplayName", true, "DisplayName", q)
                    .Select(user => new
                    {
                        id = "user-" + user.UserID,
                        name = user.DisplayName,
                        iconfile = UserController.Instance.GetUserProfilePictureUrl(user.UserID, 32, 32),
                    }).ToList();

                // Roles should be visible to Administrators or User in the Role.
                var roles = RoleController.Instance.GetRolesBasicSearch(portalId, numResults, q);
                results.AddRange(from roleInfo in roles
                                 where
                                     isAdmin ||
                                     this.UserInfo.Social.Roles.SingleOrDefault(ur => ur.RoleID == roleInfo.RoleID && ur.IsOwner) != null
                                 select new
                                 {
                                     id = "role-" + roleInfo.RoleID,
                                     name = roleInfo.RoleName,
                                     iconfile = TestableGlobals.Instance.ResolveUrl(string.IsNullOrEmpty(roleInfo.IconFile)
                                                 ? "~/images/no_avatar.gif"
                                                 : this.PortalSettings.HomeDirectory.TrimEnd('/') + "/" + roleInfo.IconFile),
                                 });

                return this.Request.CreateResponse(HttpStatusCode.OK, results.OrderBy(sr => sr.name));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        public class CreateDTO
        {
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string Subject;
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string Body;
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string RoleIds;
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string UserIds;
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public string FileIds;
        }
    }
}
