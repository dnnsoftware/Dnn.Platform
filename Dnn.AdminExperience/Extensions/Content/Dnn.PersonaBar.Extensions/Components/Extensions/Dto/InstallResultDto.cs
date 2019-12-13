﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Services.Installer.Log;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class InstallResultDto
    {
        [JsonProperty("newPackageId")]
        public int NewPackageId { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; } = true;

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("logs")]
        public IList<InstallerLogEntry> Logs { get; set; } = new List<InstallerLogEntry>();

        public void Failed(string message)
        {
            Success = false;
            Message = message;
        }

        public void Succeed()
        {
            Success = true;
            Message = string.Empty;
        }

        public void AddLogs(IEnumerable<LogEntry> logs)
        {
            Logs = logs?.Select(
                l => new InstallerLogEntry
                {
                    Type = l.Type.ToString(),
                    Description = l.Description
                }).ToList() ?? new List<InstallerLogEntry>();
        }
    }
}
