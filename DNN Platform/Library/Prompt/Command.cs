// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;
using System;

namespace DotNetNuke.Prompt
{
    [Serializable]
    [JsonObject]
    public class Command : ICommand
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }

        [JsonIgnore]
        public Type CommandType { get; set; }
    }
}
