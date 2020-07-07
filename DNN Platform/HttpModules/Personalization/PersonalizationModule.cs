// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Personalization
{
    using System;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Personalization;

    public class PersonalizationModule : IHttpModule
    {
        public string ModuleName
        {
            get
            {
                return "PersonalizationModule";
            }
        }

        public void Init(HttpApplication application)
        {
            application.EndRequest += this.OnEndRequest;
        }

        public void Dispose()
        {
        }

        public void OnEndRequest(object s, EventArgs e)
        {
            HttpContext context = ((HttpApplication)s).Context;
            HttpRequest request = context.Request;

            if (!Initialize.ProcessHttpModule(request, false, false))
            {
                return;
            }

            // Obtain PortalSettings from Current Context
            var _portalSettings = (PortalSettings)context.Items["PortalSettings"];
            if (_portalSettings != null)
            {
                // load the user info object
                UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();
                var personalization = new PersonalizationController();
                personalization.SaveProfile(context, UserInfo.UserID, _portalSettings.PortalId);
            }
        }
    }
}
