// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using System;

    using DotNetNuke.Internal.SourceGenerators;

    using Newtonsoft.Json;

    /// <inheritdoc cref="DotNetNuke.Prompt.CommandOption"/>
    [Serializable]
    [JsonObject]
    [DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project.")]
    public partial class CommandOption
    {
        /// <summary>Gets or sets of sets the name of the flag.</summary>
        public string Flag { get; set; }

        /// <summary>Gets or sets the type of the flag value expected.</summary>
        public string Type { get; set; }

        /// <summary>Gets or sets a value indicating whether the flag is required or not.</summary>
        public bool Required { get; set; }

        /// <summary>Gets or sets the default value of the flag.</summary>
        public string DefaultValue { get; set; }

        /// <summary>Gets or sets of sets the description of the flag.</summary>
        public string Description { get; set; }
    }
}
