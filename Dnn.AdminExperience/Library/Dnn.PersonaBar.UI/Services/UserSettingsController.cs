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
    using Dnn.PersonaBar.Library.Dto;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A Persona Bar API controller for user settings.</summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class UserSettingsController : PersonaBarApiController
    {
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalGroupController portalGroupController;

        /// <summary>Initializes a new instance of the <see cref="UserSettingsController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public UserSettingsController()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="UserSettingsController"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        public UserSettingsController(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
        {
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
        }

        /// <summary>Update Persona Bar's User Settings.</summary>
        /// <param name="settings">The user settings.</param>
        /// <returns>A response with an empty object.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateUserSettings(IDictionary<string, object> settings)
        {
            try
            {
                var controller = PersonaBarUserSettingsController.Instance;
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, this.PortalSettings.PortalId);

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
