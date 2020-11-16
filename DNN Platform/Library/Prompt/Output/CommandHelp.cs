// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace DotNetNuke.Prompt
{
    [Serializable]
    [JsonObject]
    public class CommandHelp : ICommandHelp
    {
        /// <inheritdoc/>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "options")]
        public IEnumerable<ICommandOption> Options { get; set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "resultHtml")]
        public string ResultHtml { get; set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
