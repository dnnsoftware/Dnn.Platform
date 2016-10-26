#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings



#endregion

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Installer.Packages;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [DataContract]
    public class ModulePackageDetailDto : ModulePackagePermissionsDto
    {
        [DataMember(Name = "moduleName")]
        public string ModuleName { get; set; }

        [DataMember(Name = "folderName")]
        public string FolderName { get; set; }

        [DataMember(Name = "category")]
        public string Category { get; set; }

        [DataMember(Name = "businessController")]
        public string BusinessController { get; set; }

        [DataMember(Name = "dependencies")]
        public string Dependencies { get; set; }

        [DataMember(Name = "hostPermissions")]
        public string HostPermissions { get; set; }

        [DataMember(Name = "portable")]
        public bool Portable { get; set; }

        [DataMember(Name = "searchable")]
        public bool Searchable { get; set; }

        [DataMember(Name = "upgradeable")]
        public bool Upgradeable { get; set; }

        [DataMember(Name = "shareable")]
        public ModuleSharing Shareable { get; set; }

        [DataMember(Name = "premiumModule")]
        public bool PremiumModule { get; set; }

        [DataMember(Name = "assignedPortals")]
        public IList<IdNameDto> AssignedPortals { get; set; } = new List<IdNameDto>();

        [DataMember(Name = "unassignedPortals")]
        public IList<IdNameDto> UnassignedPortals { get; set; } = new List<IdNameDto>();

        [DataMember(Name = "moduleDefinitions")]
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
                    AssignedPortals.Add(new IdNameDto { Id = value.PortalID, Name = value.PortalName });
                }

                var assignedIds = AssignedPortals.Select(p => p.Id).ToArray();
                var allPortals = PortalController.Instance.GetPortals().OfType<PortalInfo>().Where(p => !assignedIds.Contains(p.PortalID));

                foreach (var portalInfo in allPortals)
                {
                    UnassignedPortals.Add(new IdNameDto { Id = portalInfo.PortalID, Name = portalInfo.PortalName });
                }
            }

            foreach (var moduleDefinition in desktopModule.ModuleDefinitions.Values)
            {
                ModuleDefinitions.Add(new ModuleDefinitionDto(moduleDefinition));
            }
        }
    }
}