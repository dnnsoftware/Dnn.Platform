// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Services.ImprovementsProgram
{
    public interface IBeaconService
    {
        string GetBeaconEndpoint();
        string GetBeaconQuery(UserInfo user, string filePath = null);
        string GetBeaconUrl(UserInfo user, string filePath = null);
        bool IsBeaconEnabledForControlBar(UserInfo user);
        bool IsBeaconEnabledForPersonaBar();
    }
}
