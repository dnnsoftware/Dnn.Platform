// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.UsersOnline
{
    using System;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>An HTTP module which keeps track of the users actively on the site.</summary>
    [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
    public partial class UsersOnlineModule : IHttpModule
    {
        /// <summary>Gets the HttpModule module name.</summary>
        public string ModuleName => "UsersOnlineModule";

        /// <inheritdoc/>
        public void Init(HttpApplication application)
        {
            application.AuthorizeRequest += this.OnAuthorizeRequest;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>Handles the <see cref="HttpApplication.AuthorizeRequest"/> event.</summary>
        /// <param name="s">The sender.</param>
        /// <param name="e">The event args.</param>
        public void OnAuthorizeRequest(object s, EventArgs e)
        {
            // First check if we are upgrading/installing
            var app = (HttpApplication)s;
            HttpRequest request = app.Request;

            // check if we are upgrading/installing or if this is a captcha request
            if (!Initialize.ProcessHttpModule(request, false, false))
            {
                return;
            }

            // Create a Users Online Controller
            var objUserOnlineController = new UserOnlineController();

            // Is Users Online Enabled?
            if (objUserOnlineController.IsEnabled())
            {
                objUserOnlineController.TrackUsers();
            }
        }
    }
}
