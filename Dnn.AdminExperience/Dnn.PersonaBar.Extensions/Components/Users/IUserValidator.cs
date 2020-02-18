// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components
{
    public interface IUserValidator
    {
        ConsoleErrorResultModel ValidateUser(int? userId, PortalSettings portalSettings, UserInfo user, out UserInfo userInfo);
    }
}
