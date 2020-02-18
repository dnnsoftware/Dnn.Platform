// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.IO;
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Extensions.Components
{
    public interface IInstallController
    {
        InstallResultDto InstallPackage(PortalSettings portalSettings, UserInfo user, string legacySkin, string fileName,
            Stream stream, bool isPortalPackage = false);

        ParseResultDto ParsePackage(PortalSettings portalSettings, UserInfo user, string fileName, Stream stream);
    }
}
