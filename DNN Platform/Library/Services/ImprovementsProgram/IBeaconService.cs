// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.ImprovementsProgram
{
    using DotNetNuke.Entities.Users;

    public interface IBeaconService
    {
        string GetBeaconEndpoint();

        string GetBeaconQuery(UserInfo user, string filePath = null);

        string GetBeaconUrl(UserInfo user, string filePath = null);

        bool IsBeaconEnabledForControlBar(UserInfo user);

        bool IsBeaconEnabledForPersonaBar();
    }
}
