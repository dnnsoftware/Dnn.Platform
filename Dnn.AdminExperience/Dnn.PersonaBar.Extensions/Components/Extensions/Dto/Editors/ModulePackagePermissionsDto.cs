// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors;

using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

[JsonObject]
public class ModulePackagePermissionsDto : PackageInfoDto
{
    /// <summary>Initializes a new instance of the <see cref="ModulePackagePermissionsDto"/> class.</summary>
    public ModulePackagePermissionsDto()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ModulePackagePermissionsDto"/> class.</summary>
    /// <param name="portalId"></param>
    /// <param name="package"></param>
    public ModulePackagePermissionsDto(int portalId, PackageInfo package)
        : base(portalId, package)
    {
    }

    [JsonProperty("desktopModuleId")]
    public int DesktopModuleId { get; set; }

    [JsonProperty("permissions")]
    public PermissionsDto Permissions { get; set; }
}
