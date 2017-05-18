#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.HttpModules.UsersOnline
{
    public class UsersOnlineModule : IHttpModule
    {
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