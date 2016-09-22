#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.DTO;
using Dnn.PersonaBar.Library.PersonaBar.Controllers;
using Dnn.PersonaBar.UI.Common;
using Dnn.PersonaBar.UI.Components.Controllers;
using Dnn.PersonaBar.UI.Components.Entities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.UI.Services.PersonaBar
{
    [DnnAuthorize(StaticRoles = Constants.AllMajorRoles)]
    public class UserSettingsController : PersonaBarApiController
    {
        /// <summary>
        /// Update Person Bar's User Settings
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateUserSettings(UserSettings settings)
        {
            try
            {
                var controller = PersonaBarUserSettingsController.Instance;
                var portalId = PortalController.GetEffectivePortalId(PortalSettings.PortalId);
                controller.UpdatePersonaBarUserSettings(settings, UserInfo.UserID, portalId);
                return Request.CreateResponse(HttpStatusCode.OK, new {});
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }
    }
}