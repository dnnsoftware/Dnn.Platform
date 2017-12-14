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
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.UserRequest;
using System;
#endregion

namespace DotNetNuke.Services.Authentication
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationLoginBase class provides a bas class for Authentiication 
    /// Login controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class AuthenticationLoginBase : UserModuleBase
    {
        #region Delegates

        public delegate void UserAuthenticatedEventHandler(object sender, UserAuthenticatedEventArgs e);

        #endregion

        protected AuthenticationLoginBase()
        {
            RedirectURL = Null.NullString;
            AuthenticationType = Null.NullString;
            Mode = AuthMode.Login;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Type of Authentication associated with this control
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the control is Enabled
        /// </summary>
        /// <remarks>This property must be overriden in the inherited class</remarks>
        /// -----------------------------------------------------------------------------
        public abstract bool Enabled { get; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the IP address associated with the request
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string IPAddress
        {
            get
            {
                return GetIPAddress();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Authentication mode of the control (Login or Register)
        /// </summary>
        /// <remarks>This property may be overriden in the inherited class</remarks>
        /// -----------------------------------------------------------------------------        
        public virtual AuthMode Mode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Redirect Url for this control
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string RedirectURL { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the control supports Registration
        /// </summary>
        /// <remarks>This property may be overriden in the inherited class</remarks>
        /// -----------------------------------------------------------------------------
        public virtual bool SupportsRegistration { get { return false; } }

        public event UserAuthenticatedEventHandler UserAuthenticated;

        protected virtual void OnUserAuthenticated(UserAuthenticatedEventArgs ea)
        {
            if (UserAuthenticated != null)
            {
                UserAuthenticated(null, ea);
            }
        }

        [Obsolete("Deprecated in 9.2.0. Use UserRequestIPAddressController.Instance.GetUserRequestIPAddress")]
        public static string GetIPAddress()
        {
            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request));                        
        }
    }
}
