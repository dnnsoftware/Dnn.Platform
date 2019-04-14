using System;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt
{
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


