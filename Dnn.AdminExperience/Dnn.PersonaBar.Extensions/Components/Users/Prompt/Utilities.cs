// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt
{
    using System;

    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    public class Utilities
    {
        private static readonly IUserValidator _userValidator = new UserValidator();

        [Obsolete("Deprecated in 9.2.1. Use IUserValidator.ValidateUser")]
        public static ConsoleErrorResultModel ValidateUser(int? userId, PortalSettings portalSettings, UserInfo currentUserInfo, out UserInfo userInfo)
        {
            return _userValidator.ValidateUser(userId, portalSettings, currentUserInfo, out userInfo);
        }
    }
}


