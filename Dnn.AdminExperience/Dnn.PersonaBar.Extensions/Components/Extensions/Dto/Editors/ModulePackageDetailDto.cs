// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Installer.Packages;
    using Newtonsoft.Json;

    [JsonObject]
    public class ModulePackageDetailDto : ModulePackagePermissionsDto
    {
        public ModulePackageDetailDto()
        {
        }

        public ModulePackageDetailDto(int portalId, PackageInfo package, DesktopModuleInfo desktopModule) : base(portalId, package)
        {
            this.DesktopModuleId = desktopModule.DesktopModuleID;
            this.ModuleName = desktopModule.ModuleName;
            this.FolderName = desktopModule.FolderName;
            this.BusinessController = desktopModule.BusinessControllerClass;
            this.Category = desktopModule.Category;
            this.Dependencies = desktopModule.Dependencies;
            this.HostPermissions = desktopModule.Permissions;
            this.Portable = desktopModule.IsPortable;
            this.Searchable = desktopModule.IsSearchable;
            this.Upgradeable = desktopModule.IsUpgradeable;
            this.PremiumModule = desktopModule.IsPremium;
            this.Shareable = desktopModule.Shareable;

            if (!desktopModule.IsAdmin)
            {
                var portalDesktopModules =
                    DesktopModuleController.GetPortalDesktopModulesByDesktopModuleID(desktopModule.DesktopModuleID);
                foreach (var portalDesktopModuleInfo in portalDesktopModules)
                {
                    var value = portalDesktopModuleInfo.Value;
                    this.AssignedPortals.Add(new ListItemDto { Id = value.PortalID, Name = value.PortalName });
                }

                var assignedIds = this.AssignedPortals.Select(p => p.Id).ToArray();
                var allPortals = PortalController.Instance.GetPortals().OfType<PortalInfo>().Where(p => !assignedIds.Contains(p.PortalID));

                foreach (var portalInfo in allPortals)
                {
                    this.UnassignedPortals.Add(new ListItemDto { Id = portalInfo.PortalID, Name = portalInfo.PortalName });
                }
            }

            foreach (var moduleDefinition in desktopModule.ModuleDefinitions.Values)
            {
                this.ModuleDefinitions.Add(new ModuleDefinitionDto(moduleDefinition));
            }
        }

        [JsonProperty("moduleName")]
        public string ModuleName { get; set; }

        [JsonProperty("folderName")]
        public string FolderName { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("businessController")]
        public string BusinessController { get; set; }

        [JsonProperty("dependencies")]
        public string Dependencies { get; set; }

        [JsonProperty("hostPermissions")]
        public string HostPermissions { get; set; }

        [JsonProperty("portable")]
        public bool Portable { get; set; }

        [JsonProperty("searchable")]
        public bool Searchable { get; set; }

        [JsonProperty("upgradeable")]
        public bool Upgradeable { get; set; }

        [JsonProperty("shareable")]
        public ModuleSharing Shareable { get; set; }

        [JsonProperty("premiumModule")]
        public bool PremiumModule { get; set; }

        [JsonProperty("assignedPortals")]
        public IList<ListItemDto> AssignedPortals { get; set; } = new List<ListItemDto>();

        [JsonProperty("unassignedPortals")]
        public IList<ListItemDto> UnassignedPortals { get; set; } = new List<ListItemDto>();

        [JsonProperty("moduleDefinitions")]
        public IList<ModuleDefinitionDto> ModuleDefinitions { get; set; } = new List<ModuleDefinitionDto>();
    }
}
