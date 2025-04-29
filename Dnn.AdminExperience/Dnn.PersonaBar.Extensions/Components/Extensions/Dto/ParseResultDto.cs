// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto;

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Log;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;

[JsonObject]
public class ParseResultDto : PackageInfoDto
{
    public ParseResultDto()
    {
    }

    public ParseResultDto(PackageInfo package)
        : base(Null.NullInteger, package)
    {
    }

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

    public void Failed(string message, IList<LogEntry> logs = null)
    {
        this.Success = false;
        this.Message = Localization.GetString(message, Constants.SharedResources) ?? message;
        this.AddLogs(logs);

        // UI devs asked for these to be non-null empty for moving to next step
        this.Description = this.Description ?? string.Empty;
        this.Email = this.Email ?? string.Empty;
        this.FriendlyName = this.FriendlyName ?? string.Empty;
        this.IsInUse = this.IsInUse ?? string.Empty;
        this.License = this.License ?? string.Empty;
        this.Name = this.Name ?? string.Empty;
        this.Organization = this.Organization ?? string.Empty;
        this.Owner = this.Owner ?? string.Empty;
        this.PackageIcon = this.PackageIcon ?? string.Empty;
        this.PackageType = this.PackageType ?? string.Empty;
        this.ReleaseNotes = this.ReleaseNotes ?? string.Empty;
        this.SiteSettingsLink = this.SiteSettingsLink ?? string.Empty;
        this.UpgradeIndicator = this.UpgradeIndicator ?? string.Empty;
        this.UpgradeUrl = this.UpgradeUrl ?? string.Empty;
        this.Url = this.Url ?? string.Empty;
        this.Version = this.Version ?? string.Empty;

        this.LegacyError = this.LegacyError ?? string.Empty;
        this.Message = this.Message ?? string.Empty;
    }

    public void Succeed(IList<LogEntry> logs)
    {
        this.Success = true;
        this.Message = string.Empty;
        this.AddLogs(logs);
    }

    public void AddLogs(IEnumerable<LogEntry> logs)
    {
        if (logs == null)
        {
            this.Logs = new List<InstallerLogEntry>();
        }
        else
        {
            this.Logs = logs.Select(l => new InstallerLogEntry { Type = l.Type.ToString(), Description = l.Description }).ToList();
        }
    }
}
