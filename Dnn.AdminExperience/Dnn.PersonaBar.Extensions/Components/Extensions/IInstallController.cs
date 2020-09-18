// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components
{
    using System.IO;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    public interface IInstallController
    {
        InstallResultDto InstallPackage(PortalSettings portalSettings, UserInfo user, string legacySkin, string fileName,
            Stream stream, bool isPortalPackage = false);

        ParseResultDto ParsePackage(PortalSettings portalSettings, UserInfo user, string fileName, Stream stream);
    }
}
