using Dnn.PersonaBar.Prompt.Models;

namespace Dnn.PersonaBar.Prompt.Interfaces
{
    public interface IConsoleCommand
    {
        void Init(string[] args, DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Entities.Users.UserInfo userInfo, int activeTabId);
        ConsoleResultModel Run();
        bool IsValid();
        string ValidationMessage { get; }
    }
}