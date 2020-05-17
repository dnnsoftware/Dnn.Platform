﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
