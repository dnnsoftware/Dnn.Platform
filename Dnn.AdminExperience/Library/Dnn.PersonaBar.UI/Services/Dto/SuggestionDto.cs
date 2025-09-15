// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services.DTO
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>Represents a suggestion for an autocomplete.</summary>
    [DataContract]
    [Serializable]
    public class SuggestionDto
    {
        /// <summary>Gets or sets the ID of the suggested item.</summary>
        [DataMember(Name = "value")]
        public int Value { get; set; }

        /// <summary>Gets or sets the label for the suggestion.</summary>
        [DataMember(Name = "label")]
        public string Label { get; set; }
    }
}
