using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Client
{
    [ConsoleCommand("set-mode", Constants.GeneralCategory, "Prompt_SetMode_Description")]
    public class SetMode : IConsoleCommand
    {
        public string LocalResourceFile => Constants.LocalResourcesFile;
        [FlagParameter("mode", "Prompt_SetMode_FlagMode", "DNN View Mode", true)]
        private const string FlagMode = "mode";

        public string ResultHtml => Localization.GetString("Prompt_SetMode_ResultHtml", LocalResourceFile);

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