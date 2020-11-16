// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteGroups.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.SiteGroups.Models;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class SiteGroupsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SiteGroupsController));

        IManagePortalGroups GroupManager
        {
            get
            {
                return SiteGroups.Instance;
            }
        }

        [HttpGet]
        public HttpResponseMessage GetSiteGroups()
        {
            var groups = this.GroupManager.SiteGroups();
            return this.Request.CreateResponse(HttpStatusCode.OK, groups);
        }

        [HttpGet]
        public HttpResponseMessage GetAvailablePortals()
        {
            var portals = this.GroupManager.AvailablePortals();
            return this.Request.CreateResponse(HttpStatusCode.OK, portals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Save(PortalGroupInfo portalGroup)
        {
            try
            {
                var id = this.GroupManager.Save(portalGroup);
                return this.Request.CreateResponse(HttpStatusCode.OK, id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Delete(int groupId)
        {
            try
            {
                this.GroupManager.Delete(groupId);
                return this.Request.CreateResponse(HttpStatusCode.OK, groupId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}