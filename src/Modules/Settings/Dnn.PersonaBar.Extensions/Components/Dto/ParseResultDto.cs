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
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Log;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class ParseResultDto : PackageInfoDto
    {
        [JsonProperty("success")]
        public bool Success { get; set; } = true;

        [JsonProperty("azureCompact")]
        public bool AzureCompact { get; set; }

        [JsonProperty("noManifest")]
        public bool NoManifest { get; set; }

        [JsonProperty("legacyError")]
        public string LegacyError { get; set; }

        [JsonProperty("hasInvalidFiles")]
        public bool HasInvalidFiles { get; set; }

        [JsonProperty("alreadyInstalled")]
        public bool AlreadyInstalled { get; set; }

        [JsonProperty("legacySkinInstalled")]
        public bool LegacySkinInstalled { get; set; }

        [JsonProperty("legacyContainerInstalled")]
        public bool LegacyContainerInstalled { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("logs")]
        public IList<InstallerLogEntry> Logs { get; set; }

        public ParseResultDto()
        {

        }

        public ParseResultDto(PackageInfo package) : base(Null.NullInteger, package)
        {

        }

        public void Failed(string message, IList<LogEntry> logs = null)
        {
            Success = false;
            Message = Localization.GetString(message, Constants.SharedResources) ?? message;
            AddLogs(logs);
            // UI devs asked for these to be non-null empty for moving to next step
            Description = Description ?? "";
            Email = Email ?? "";
            FriendlyName = FriendlyName ?? "";
            IsInUse = IsInUse ?? "";
            License = License ?? "";
            Name = Name ?? "";
            Organization = Organization ?? "";
            Owner = Owner ?? "";
            PackageIcon = PackageIcon ?? "";
            PackageType = PackageType ?? "";
            ReleaseNotes = ReleaseNotes ?? "";
            SiteSettingsLink = SiteSettingsLink ?? "";
            UpgradeIndicator = UpgradeIndicator ?? "";
            UpgradeUrl = UpgradeUrl ?? "";
            Url = Url ?? "";
            Version = Version ?? "";

            LegacyError = LegacyError ?? "";
            Message = Message ?? "";
        }

        public void Succeed(IList<LogEntry> logs)
        {
            Success = true;
            Message = string.Empty;
            AddLogs(logs);
        }

        public void AddLogs(IEnumerable<LogEntry> logs)
        {
            if (logs == null)
                Logs = new List<InstallerLogEntry>();
            else
                Logs = logs.Select(l => new InstallerLogEntry { Type = l.Type.ToString(), Description = l.Description }).ToList();
        }
    }
}