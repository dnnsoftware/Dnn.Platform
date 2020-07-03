// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Prompt
{
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Users;

    public interface IConsoleCommand
    {
        string ValidationMessage { get; }

        string LocalResourceFile { get; }

        string ResultHtml { get; }

        void Initialize(string[] args, DotNetNuke.Entities.Portals.PortalSettings portalSettings, UserInfo userInfo, int activeTabId);

        ConsoleResultModel Run();

        bool IsValid();
    }
}
