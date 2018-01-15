#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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