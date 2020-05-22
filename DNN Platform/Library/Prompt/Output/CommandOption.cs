// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;

namespace DotNetNuke.Prompt.Output
{
    [Serializable]
    [JsonObject]
    public class CommandOption : ICommandOption
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is flag required or not
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Default value of the flag
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Resource key of the description of flag
        /// </summary>
        public string DescriptionKey { get; set; }
    }
}
