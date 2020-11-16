// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Services.Installer.Log;
    using Newtonsoft.Json;

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
            this.Success = false;
            this.Message = message;
        }

        public void Succeed()
        {
            this.Success = true;
            this.Message = string.Empty;
        }

        public void AddLogs(IEnumerable<LogEntry> logs)
        {
            this.Logs = logs?.Select(
                l => new InstallerLogEntry
                {
                    Type = l.Type.ToString(),
                    Description = l.Description
                }).ToList() ?? new List<InstallerLogEntry>();
        }
    }
}
