// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;
    using Newtonsoft.Json;

    [JsonObject]
    public class ModuleControlDto
    {
        public ModuleControlDto()
        {
        }

        public ModuleControlDto(ModuleControlInfo moduleControl)
        {
            this.Id = moduleControl.ModuleControlID;
            this.DefinitionId = moduleControl.ModuleDefID;
            this.Key = moduleControl.ControlKey;
            this.Title = moduleControl.ControlTitle;
            this.Source = moduleControl.ControlSrc;
            this.Type = moduleControl.ControlType;
            this.Order = moduleControl.ViewOrder;
            this.Icon = moduleControl.IconFile;
            this.HelpUrl = moduleControl.HelpURL;
            this.SupportPopups = moduleControl.SupportsPopUps;
            this.SupportPartialRendering = moduleControl.SupportsPartialRendering;
        }

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

        public ModuleControlInfo ToModuleControlInfo()
        {
            return new ModuleControlInfo
            {
                ModuleControlID = this.Id,
                ModuleDefID = this.DefinitionId,
                ControlKey = this.Key,
                ControlTitle = this.Title,
                ControlSrc = this.Source,
                ControlType = this.Type,
                ViewOrder = this.Order,
                IconFile = this.Icon,
                HelpURL = this.HelpUrl,
                SupportsPartialRendering = this.SupportPartialRendering,
                SupportsPopUps = this.SupportPopups
            };
        }
    }
}
