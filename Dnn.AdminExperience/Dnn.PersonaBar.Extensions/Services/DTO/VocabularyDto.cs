// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Vocabularies.Services.Dto
{
    using System;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    [JsonObject]
    public class VocabularyDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("scopeType")]
        public string ScopeType { get; set; }

        [JsonProperty("scopeTypeId")]
        public int ScopeTypeId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("typeId")]
        public int TypeId { get; set; }

        [JsonProperty("vocabularyId")]
        public int VocabularyId { get; set; }
    }
}
