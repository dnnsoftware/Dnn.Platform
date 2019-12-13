// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.UI.Services.DTO
{
    [DataContract]
    public class SuggestionDto
    {  
        [DataMember(Name = "value")]
        public int Value { get; set; }

        [DataMember(Name = "label")]
        public string Label { get; set; }
    }
}
