using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Client
{
    [ConsoleCommand("clear-screen", Constants.GeneralCategory, "Prompt_Cls_Description")]
    public class ClearScreen : IConsoleCommand
    {
        public string LocalResourceFile => Constants.LocalResourcesFile;

        public string ResultHtml => Localization.GetString("Prompt_Cls_ResultHtml", LocalResourceFile);

        public string ValidationMessage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Initialize(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            throw new NotImplementedException();
        }

        public ConsoleResultModel Run()
        {
            throw new NotImplementedException();
        }

    }
}