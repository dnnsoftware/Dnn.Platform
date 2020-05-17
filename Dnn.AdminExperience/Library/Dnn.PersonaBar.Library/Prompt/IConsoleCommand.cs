// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Library.Prompt.Models;
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
