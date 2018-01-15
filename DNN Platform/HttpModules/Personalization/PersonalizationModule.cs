#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Personalization;

#endregion

namespace DotNetNuke.HttpModules.Personalization
{
    public class PersonalizationModule : IHttpModule
    {
        public string ModuleName
        {
            get
            {
                return "PersonalizationModule";
            }
        }

        #region IHttpModule Members

        public void Init(HttpApplication application)
        {
            application.EndRequest += OnEndRequest;
        }

        public void Dispose()
        {
        }

        #endregion

        public void OnEndRequest(object s, EventArgs e)
        {
            HttpContext context = ((HttpApplication) s).Context;
            HttpRequest request = context.Request;

            if (!Initialize.ProcessHttpModule(request, false, false))
            {
                return;
            }
			
            //Obtain PortalSettings from Current Context
            var _portalSettings = (PortalSettings)context.Items["PortalSettings"];
            if (_portalSettings != null)
            {
                //load the user info object
                UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();
                var personalization = new PersonalizationController();
                personalization.SaveProfile(context, UserInfo.UserID, _portalSettings.PortalId);
            }
        }
    }
}