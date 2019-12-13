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

namespace Dnn.PersonaBar.UI.Services
{
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class UserSettingsController : PersonaBarApiController
    {
        /// <summary>
        /// Update Person Bar's User Settings
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateUserSettings(IDictionary<string, object> settings)
        {
            try
            {
                var controller = PersonaBarUserSettingsController.Instance;
                var portalId = PortalController.GetEffectivePortalId(PortalSettings.PortalId);

                var userSettings = new UserSettings();
                settings.ForEach(kvp =>
                {
                    userSettings.Add(kvp.Key, kvp.Value);
                });

                controller.UpdatePersonaBarUserSettings(userSettings, UserInfo.UserID, portalId);
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
