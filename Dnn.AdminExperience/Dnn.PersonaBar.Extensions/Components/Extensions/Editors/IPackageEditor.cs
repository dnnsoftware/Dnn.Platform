// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Extensions.Components.Dto;
using DotNetNuke.Services.Installer.Packages;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public interface IPackageEditor
    {
        PackageInfoDto GetPackageDetail(int portalId, PackageInfo package);

        bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage);
    }
}
