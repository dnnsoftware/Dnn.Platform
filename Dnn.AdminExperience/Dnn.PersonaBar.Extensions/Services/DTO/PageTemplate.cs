// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class PageTemplate
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "includeContent")]
        public bool IncludeContent { get; set; }

        [DataMember(Name = "tabId")]
        public int TabId { get; set; }
    }
}
