using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Library.Prompt
{
    public interface IConsoleCommand
    {
        void Init(string[] args, DotNetNuke.Entities.Portals.PortalSettings portalSettings, UserInfo userInfo, int activeTabId);
        ConsoleResultModel Run();
        bool IsValid();
        string ValidationMessage { get; }
    }
}