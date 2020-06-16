// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    [JsonObject]
    public class CommandOption
    {
        /// <summary>
        /// Name of the flag
        /// </summary>
        public string Flag { get; set; }

        /// <summary>
        /// Type of the flag value expected.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Is flag required or not
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Default value of the flag
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Description of flag
        /// </summary>
        public string Description { get; set; }
    }
}
