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

using DotNetNuke.Entities.Controllers;
using System;
using System.Web;
using System.Linq;
using DotNetNuke.Framework;
using System.Net;

namespace DotNetNuke.Services.UserRequest
{
    public class UserRequestIPAddressController : ServiceLocator<IUserRequestIPAddressController,UserRequestIPAddressController>, IUserRequestIPAddressController
    {
        public string GetUserRequestIPAddress(HttpRequestBase request)
        {            
            var userRequestIPHeader = HostController.Instance.GetString("UserRequestIPHeader", "X-Forwarded-For");
            var userIPAddress = string.Empty;

            if (request.Headers.AllKeys.Contains(userRequestIPHeader))
            {
                userIPAddress = request.Headers[userRequestIPHeader];
                userIPAddress = userIPAddress.Split(',')[0];                
            }            

            if (string.IsNullOrEmpty(userIPAddress))
            {
                var remoteAddrVariable = "REMOTE_ADDR";
                if (request.ServerVariables.AllKeys.Contains(remoteAddrVariable))
                {
                    userIPAddress = request.ServerVariables[remoteAddrVariable];
                }
            }

            if (string.IsNullOrEmpty(userIPAddress))
            {
                userIPAddress = request.UserHostAddress;
            }

            if (string.IsNullOrEmpty(userIPAddress) || userIPAddress.Trim() == "::1")
            {
                userIPAddress = string.Empty;
            }
            
            if (!string.IsNullOrEmpty(userIPAddress) && !ValidateIPv4(userIPAddress))
            {
                userIPAddress = string.Empty;
            }

            return userIPAddress;
        }

        private bool ValidateIPv4(string ipString)
        {
            if (ipString.Split('.').Length != 4) return false;
            IPAddress address;
            return IPAddress.TryParse(ipString, out address);
        }


        protected override Func<IUserRequestIPAddressController> GetFactory()
        {
            return () => new UserRequestIPAddressController();
        }
    }
}