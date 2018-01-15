#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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