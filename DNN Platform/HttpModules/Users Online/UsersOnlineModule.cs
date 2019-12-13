﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.HttpModules.UsersOnline
{
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

        #region IHttpModule Members

        public void Init(HttpApplication application)
        {
            application.AuthorizeRequest += OnAuthorizeRequest;
        }

        public void Dispose()
        {
        }

        #endregion

        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public void OnAuthorizeRequest(object s, EventArgs e)
        {
            //First check if we are upgrading/installing
            var app = (HttpApplication) s;
            HttpRequest request = app.Request;

            //check if we are upgrading/installing or if this is a captcha request
            if (!Initialize.ProcessHttpModule(request, false, false))
            {
                return;
            }

            //Create a Users Online Controller
            var objUserOnlineController = new UserOnlineController();

            //Is Users Online Enabled?
            if ((objUserOnlineController.IsEnabled()))
            {
                objUserOnlineController.TrackUsers();
            }
        }
    }
}
