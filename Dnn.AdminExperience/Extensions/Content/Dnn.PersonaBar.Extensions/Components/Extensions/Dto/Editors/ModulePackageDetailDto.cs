#region Usings



#endregion

using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class ModulePackageDetailDto : ModulePackagePermissionsDto
    {
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
        public IList<ModuleDefinitionDto> ModuleDefinitions { get; set; }  = new List<ModuleDefinitionDto>();

        public ModulePackageDetailDto()
        {
            
        }

        public ModulePackageDetailDto(int portalId, PackageInfo package, DesktopModuleInfo desktopModule) : base(portalId, package)
        {
            DesktopModuleId = desktopModule.DesktopModuleID;
            ModuleName = desktopModule.ModuleName;
            FolderName = desktopModule.FolderName;
            BusinessController = desktopModule.BusinessControllerClass;
            Category = desktopModule.Category;
            Dependencies = desktopModule.Dependencies;
            HostPermissions = desktopModule.Permissions;
            Portable = desktopModule.IsPortable;
            Searchable = desktopModule.IsSearchable;
            Upgradeable = desktopModule.IsUpgradeable;
            PremiumModule = desktopModule.IsPremium;
            Shareable = desktopModule.Shareable;

            if (!desktopModule.IsAdmin)
            {
                var portalDesktopModules =
                    DesktopModuleController.GetPortalDesktopModulesByDesktopModuleID(desktopModule.DesktopModuleID);
                foreach (var portalDesktopModuleInfo in portalDesktopModules)
                {
                    var value = portalDesktopModuleInfo.Value;
                    AssignedPortals.Add(new ListItemDto { Id = value.PortalID, Name = value.PortalName });
                }

                var assignedIds = AssignedPortals.Select(p => p.Id).ToArray();
                var allPortals = PortalController.Instance.GetPortals().OfType<PortalInfo>().Where(p => !assignedIds.Contains(p.PortalID));

                foreach (var portalInfo in allPortals)
                {
                    UnassignedPortals.Add(new ListItemDto { Id = portalInfo.PortalID, Name = portalInfo.PortalName });
                }
            }

            foreach (var moduleDefinition in desktopModule.ModuleDefinitions.Values)
            {
                ModuleDefinitions.Add(new ModuleDefinitionDto(moduleDefinition));
            }
        }
    }
}
