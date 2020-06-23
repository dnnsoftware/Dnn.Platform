// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Sites.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using Dnn.PersonaBar.Library.DTO.Tabs;
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [DataContract]
    public class ExportTemplateRequest
    {
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "portalId")]
        public int PortalId { get; set; }

        [DataMember(Name = "pages")]
        public IEnumerable<TabDto> Pages { get; set; }

        [DataMember(Name = "locales")]
        public IEnumerable<string> Locales { get; set; }

        [DataMember(Name = "localizationCulture")]
        public string LocalizationCulture { get; set; }

        [DataMember(Name = "isMultilanguage")]
        public bool IsMultilanguage { get; set; }

        [DataMember(Name = "includeContent")]
        public bool IncludeContent { get; set; }

        [DataMember(Name = "includeFiles")]
        public bool IncludeFiles { get; set; }

        [DataMember(Name = "includeRoles")]
        public bool IncludeRoles { get; set; }

        [DataMember(Name = "includeProfile")]
        public bool IncludeProfile { get; set; }

        [DataMember(Name = "includeModules")]
        public bool IncludeModules { get; set; }
    }
}
