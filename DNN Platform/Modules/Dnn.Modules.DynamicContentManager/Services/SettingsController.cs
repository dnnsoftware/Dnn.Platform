// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentManager.Components;
using Dnn.Modules.DynamicContentManager.Components.Entities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.DynamicContentManager.Services
{
    // <summary>
    // SettingsController provides the Web Services to manage Settings
    // </summary>
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class SettingsController : DnnApiController
    {
        /// <summary>
        /// Saves the module's settings
        /// </summary>
        /// <param name="settings">The settings to save</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Save(DCCSettings settings)
        {
            SettingsManager.Instance.Save(settings, PortalSettings, ActiveModule.ModuleID);

            var response = new
            {
                success = true
            };

            return Request.CreateResponse(response);
        }
    }
}
