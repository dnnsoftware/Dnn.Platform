using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components
{
    public interface IUserValidator
    {
        ConsoleErrorResultModel ValidateUser(int? userId, PortalSettings portalSettings, UserInfo user, out UserInfo userInfo);
    }
}