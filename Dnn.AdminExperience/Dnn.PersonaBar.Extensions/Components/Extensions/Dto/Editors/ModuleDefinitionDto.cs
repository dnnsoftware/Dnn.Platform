// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System.Collections.Generic;
using DotNetNuke.Entities.Modules.Definitions;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class ModuleDefinitionDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("desktopModuleId")]
        public int DesktopModuleId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("cacheTime")]
        public int CacheTime { get; set; }

        [JsonProperty("controls")]
        public IList<ModuleControlDto> Controls { get; set; } = new List<ModuleControlDto>();

        public ModuleDefinitionDto()
        {
            
        }

        public ModuleDefinitionDto(ModuleDefinitionInfo definition)
        {
            Id = definition.ModuleDefID;
            DesktopModuleId = definition.DesktopModuleID;
            Name = definition.DefinitionName;
            FriendlyName = definition.FriendlyName;
            CacheTime = definition.DefaultCacheTime;

            foreach (var moduleControlInfo in definition.ModuleControls.Values)
            {
                Controls.Add(new ModuleControlDto(moduleControlInfo));
            }
        }

        public ModuleDefinitionInfo ToModuleDefinitionInfo()
        {
            return new ModuleDefinitionInfo
            {
                ModuleDefID = Id,
                DesktopModuleID = DesktopModuleId,
                DefinitionName = Name,
                FriendlyName = FriendlyName,
                DefaultCacheTime = CacheTime
            };
        }
    }
}
