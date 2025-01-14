// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System.ComponentModel;

using Spectre.Console.Cli;

/// <summary>The input to the <see cref="DeployCommand"/> (i.e. CLI flags).</summary>
public class DeployInput : CommandSettings
{
    private string packagesDirectoryPath = string.Empty;

    /// <summary>Gets or sets the URL of the site to which to deploy packages.</summary>
    [CommandOption("-u|--target-uri")]
    [Description("The URL of the site to which the packages will be deployed.")]
    public string TargetUri { get; set; } = string.Empty;

    /// <summary>Gets or sets the API key with which to authenticate to the site.</summary>
    [CommandOption("-a|--api-key")]
    [Description("The key used to authenticate with the web server.")]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the key to use to encrypt the packages.</summary>
    [CommandOption("-e|--encryption-key")]
    [Description("The key used to encrypt the packages before uploading.")]
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of seconds to try to contact the site's installer before considering it an error.</summary>
    [CommandOption("-t|--installation-status-timeout")]
    [Description("The number of seconds to ignore 404 errors when checking installation status.")]
    [DefaultValue(60)]
    public int InstallationStatusTimeout { get; set; }

    /// <summary>Gets or sets the path to the directory with packages to deploy.</summary>
    [CommandOption("-d|--packages-directory")]
    [Description("Defines the directory that contains the install packages.")]
    public string PackagesDirectoryPath
    {
        get => ValidOrCurrentDirectory(this.packagesDirectoryPath);
        set => this.packagesDirectoryPath = ValidOrCurrentDirectory(value);
    }

    /// <summary>Gets or sets a value indicating whether to search the <see cref="PackagesDirectoryPath"/> recursively.</summary>
    [CommandOption("-r|--recurse")]
    [Description("Whether to search the packages directory recursively.")]
    [DefaultValue(true)]
    public bool Recurse { get; set; }

    /// <summary>Gets or sets the level of logging.</summary>
    [CommandOption("-l|--log-level")]
    [Description("Defines the amount of logging.")]
    [DefaultValue(LogLevel.Information)]
    public LogLevel LogLevel { get; set; }

    /// <summary>Gets or sets a value indicating whether to use the legacy PolyDeploy API.</summary>
    [CommandOption("--legacy-api")]
    [Description("Whether to use the PolyDeploy API instead of the BulkInstall API.")]
    [DefaultValue(false)]
    public bool LegacyApi { get; set; }

    /// <summary>Gets the URI of the site to which to deploy the packages.</summary>
    /// <returns>An absolute <see cref="Uri"/>.</returns>
    public Uri GetTargetUri() => new Uri(this.TargetUri, UriKind.Absolute);

    private static string ValidOrCurrentDirectory(string path)
    {
        return string.IsNullOrEmpty(path) ? Directory.GetCurrentDirectory() : path;
    }
}
