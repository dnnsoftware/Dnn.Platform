// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;

using System;

namespace DotNetNuke.Prompt
{
    [Serializable]
    [JsonObject]
    public class Command : ICommand
    {
        /// <inheritdoc/>
        public string Key { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public string Category { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public string Version { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public string TypeFullName { get; set; }
    }
}
