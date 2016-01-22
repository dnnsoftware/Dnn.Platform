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
using System.Collections.Specialized;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;

#endregion

namespace DotNetNuke.Services.Authentication
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserAuthenticatedEventArgs class provides a custom EventArgs object for the
    /// UserAuthenticated event
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class UserAuthenticatedEventArgs : EventArgs
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// All properties Constructor.
        /// </summary>
        /// <param name="user">The user being authenticated.</param>
        /// <param name="token">The user token</param>
        /// <param name="status">The login status.</param>
        /// <param name="type">The type of Authentication</param>
        /// -----------------------------------------------------------------------------
        public UserAuthenticatedEventArgs(UserInfo user, string token, UserLoginStatus status, string type)
        {
            Profile = new NameValueCollection();
            Message = String.Empty;
            AutoRegister = false;
            Authenticated = true;
            User = user;
            LoginStatus = status;
            UserToken = token;
            AuthenticationType = type;
            RememberMe = false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets a flag that determines whether the User was authenticated
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool Authenticated { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Authentication Type
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string AuthenticationType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets a flag that determines whether the user should be automatically registered
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool AutoRegister { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Login Status
        /// </summary>
        /// -----------------------------------------------------------------------------
        public UserLoginStatus LoginStatus { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Message
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string Message { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Profile
        /// </summary>
        /// -----------------------------------------------------------------------------
        public NameValueCollection Profile { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the RememberMe setting
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool RememberMe { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User
        /// </summary>
        /// -----------------------------------------------------------------------------
        public UserInfo User { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the UserToken (the userid or authenticated id)
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string UserToken { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Username
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string UserName { get; set; }
    }
}
