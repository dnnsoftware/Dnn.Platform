// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.SiteGroups.Models;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.SiteGroups.Services
{
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
            var groups = GroupManager.SiteGroups();
            return Request.CreateResponse(HttpStatusCode.OK, groups);
        }

        [HttpGet]
        public HttpResponseMessage GetAvailablePortals()
        {
            var portals = GroupManager.AvailablePortals();
            return Request.CreateResponse(HttpStatusCode.OK, portals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Save(PortalGroupInfo portalGroup)
        {
            try
            {
                var id = GroupManager.Save(portalGroup);
                return Request.CreateResponse(HttpStatusCode.OK, id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Delete(int groupId)
        {
            try
            {
                GroupManager.Delete(groupId);
                return Request.CreateResponse(HttpStatusCode.OK, groupId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}