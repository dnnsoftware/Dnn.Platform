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

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="IHttpModule"/> which handles user personalization.</summary>
    public class PersonalizationModule : IHttpModule
    {
        private readonly PersonalizationController personalizationController;

        /// <summary>Initializes a new instance of the <see cref="PersonalizationModule"/> class.</summary>
        public PersonalizationModule()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PersonalizationModule"/> class.</summary>
        /// <param name="personalizationController">The personalization controller.</param>
        public PersonalizationModule(PersonalizationController personalizationController)
        {
            this.personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
        }

        /// <summary>Gets the HttpModule module name.</summary>
        public string ModuleName => "PersonalizationModule";

        /// <inheritdoc/>
        public void Init(HttpApplication application)
        {
            application.EndRequest += this.OnEndRequest;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>Handles the <see cref="HttpApplication.EndRequest"/> event.</summary>
        /// <param name="s">The sender.</param>
        /// <param name="e">The event args.</param>
        public void OnEndRequest(object s, EventArgs e)
        {
            HttpContext context = ((HttpApplication)s).Context;
            HttpRequest request = context.Request;

            if (!Initialize.ProcessHttpModule(request, false, false))
            {
                return;
            }

            // Obtain PortalSettings from Current Context
            var portalSettings = (PortalSettings)context.Items["PortalSettings"];
            if (portalSettings != null)
            {
                // load the user info object
                UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
                this.personalizationController.SaveProfile(context, userInfo.UserID, portalSettings.PortalId);
            }
        }
    }
}
