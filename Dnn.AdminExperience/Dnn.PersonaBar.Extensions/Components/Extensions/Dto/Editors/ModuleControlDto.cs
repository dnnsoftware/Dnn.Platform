// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class ModuleControlDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("definitionId")]
        public int DefinitionId { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }


        [JsonProperty("type")]
        public SecurityAccessLevel Type { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("helpUrl")]
        public string HelpUrl { get; set; }

        [JsonProperty("supportPopups")]
        public bool SupportPopups { get; set; }

        [JsonProperty("supportPartialRendering")]
        public bool SupportPartialRendering { get; set; }

        public ModuleControlDto()
        {
            
        }

        public ModuleControlDto(ModuleControlInfo moduleControl)
        {
            Id = moduleControl.ModuleControlID;
            DefinitionId = moduleControl.ModuleDefID;
            Key = moduleControl.ControlKey;
            Title = moduleControl.ControlTitle;
            Source = moduleControl.ControlSrc;
            Type = moduleControl.ControlType;
            Order = moduleControl.ViewOrder;
            Icon = moduleControl.IconFile;
            HelpUrl = moduleControl.HelpURL;
            SupportPopups = moduleControl.SupportsPopUps;
            SupportPartialRendering = moduleControl.SupportsPartialRendering;
        }

        public ModuleControlInfo ToModuleControlInfo()
        {
            return new ModuleControlInfo
            {
                ModuleControlID = Id,
                ModuleDefID = DefinitionId,
                ControlKey = Key,
                ControlTitle = Title,
                ControlSrc = Source,
                ControlType = Type,
                ViewOrder = Order,
                IconFile = Icon,
                HelpURL = HelpUrl,
                SupportsPartialRendering = SupportPartialRendering,
                SupportsPopUps = SupportPopups
            };
        }
    }
}
