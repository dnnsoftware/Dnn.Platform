// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components
{
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    public interface IUserValidator
    {
        ConsoleErrorResultModel ValidateUser(int? userId, PortalSettings portalSettings, UserInfo user, out UserInfo userInfo);
    }
}
