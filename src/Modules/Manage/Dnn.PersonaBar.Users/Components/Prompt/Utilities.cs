using System.Collections.Generic;
using System.Net;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Users.Components.Prompt
{
    public class Utilities
    {
        public static ConsoleErrorResultModel ValidateUser(int? userId, PortalSettings portalSettings, UserInfo currentUserInfo, out UserInfo userInfo)
        {
            userInfo = null;
            if (!userId.HasValue) return new ConsoleErrorResultModel(Localization.GetString("Prompt_NoUserId", Constants.LocalResourcesFile));

            KeyValuePair<HttpStatusCode, string> response;
            userInfo = UsersController.GetUser(userId.Value, portalSettings, currentUserInfo, out response);
            return userInfo == null ? new ConsoleErrorResultModel(response.Value) : null;
        }
    }
}


