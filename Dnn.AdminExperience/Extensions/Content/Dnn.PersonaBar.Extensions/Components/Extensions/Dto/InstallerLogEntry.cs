// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Installer.Log;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class InstallerLogEntry
    {
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
