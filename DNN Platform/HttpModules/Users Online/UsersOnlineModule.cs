// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.UsersOnline
{
    using System;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;

    [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
    public class UsersOnlineModule : IHttpModule
    {
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public string ModuleName
        {
            get
            {
                return "UsersOnlineModule";
            }
        }

        public void Init(HttpApplication application)
        {
            application.AuthorizeRequest += this.OnAuthorizeRequest;
        }

        public void Dispose()
        {
        }

        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
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
