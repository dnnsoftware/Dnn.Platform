// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings



#endregion

using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class ModulePackagePermissionsDto : PackageInfoDto
    {
        [JsonProperty("desktopModuleId")]
        public int DesktopModuleId { get; set; }


        [JsonProperty("permissions")]
        public PermissionsDto Permissions { get; set; }

        public ModulePackagePermissionsDto()
        {
            
        }

        public ModulePackagePermissionsDto(int portalId, PackageInfo package) : base(portalId, package)
        {
            
        }
    }
}
