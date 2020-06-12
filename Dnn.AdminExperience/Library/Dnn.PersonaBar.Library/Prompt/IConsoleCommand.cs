// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
