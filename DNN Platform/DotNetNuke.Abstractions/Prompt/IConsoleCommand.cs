// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 

using DotNetNuke.Abstractions.Users;

namespace DotNetNuke.Abstractions.Prompt
{
    public interface IConsoleCommand
    {
        void Initialize(string[] args, Portals.IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId);
        IConsoleResultModel Run();
        bool IsValid();
        string ValidationMessage { get; }
        string LocalResourceFile { get; }
        string ResultHtml { get; }
    }
}
