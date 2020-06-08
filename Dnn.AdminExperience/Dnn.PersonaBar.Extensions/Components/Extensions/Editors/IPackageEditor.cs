// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
