using Dnn.PersonaBar.Library.Prompt.Attributes;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Utilities
{
    [ConsoleCommand("echo", "Echos back the first argument received", new string[] { })]
    public class Echo : ConsoleCommandBase, IConsoleCommand
    {

        public string ValidationMessage { get; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            Initialize(args, portalSettings, userInfo, activeTabId);
        }

        public bool IsValid()
        {
            return true;
        }

        public ConsoleResultModel Run()
        {
            if (Args.Length > 1)
            {
                return new ConsoleResultModel(Args[1]);
            }


            return new ConsoleErrorResultModel("Nothing to echo back");
        }

    }
}