﻿using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Library.Prompt
{
    public interface IConsoleCommand
    {
        void Initialize(string[] args, DotNetNuke.Entities.Portals.PortalSettings portalSettings, UserInfo userInfo, int activeTabId);
        ConsoleResultModel Run();
        bool IsValid();
        string ValidationMessage { get; }
        string LocalResourceFile { get; }
        string ResultHtml { get; }
    }
}