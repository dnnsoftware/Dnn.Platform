// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.DTO;
    using DotNetNuke.Collections;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Web.Api;

    [MenuPermission(Scope = ServiceScope.Regular)]
    public class UserSettingsController : PersonaBarApiController
    {
        /// <summary>
        /// Update Person Bar's User Settings.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateUserSettings(IDictionary<string, object> settings)
        {
            try
            {
                var controller = PersonaBarUserSettingsController.Instance;
                var portalId = PortalController.GetEffectivePortalId(this.PortalSettings.PortalId);

                var userSettings = new UserSettings();
                settings.ForEach(kvp =>
                {
                    userSettings.Add(kvp.Key, kvp.Value);
                });

                controller.UpdatePersonaBarUserSettings(userSettings, this.UserInfo.UserID, portalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }
    }
}
