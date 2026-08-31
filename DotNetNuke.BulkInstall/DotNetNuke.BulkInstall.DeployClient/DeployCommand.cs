// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System.IO.Abstractions;

using Spectre.Console;
using Spectre.Console.Cli;

/// <summary>The primary command.</summary>
public class DeployCommand : AsyncCommand<DeployInput>
{
    private readonly IFileSystem fileSystem;
    private readonly IDeployer deployer;

    /// <summary>Initializes a new instance of the <see cref="DeployCommand"/> class.</summary>
    /// <param name="deployer">The deployer.</param>
    /// <param name="fileSystem">The file system.</param>
    public DeployCommand(IDeployer deployer, IFileSystem fileSystem)
    {
        this.deployer = deployer;
        this.fileSystem = fileSystem;
    }

    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, DeployInput input, CancellationToken cancellationToken)
    {
        var exitCode = await this.deployer.StartAsync(input);
        return (int)exitCode;
    }

    /// <inheritdoc/>
    public override ValidationResult Validate(CommandContext context, DeployInput settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.PackagesDirectoryPath) && !this.fileSystem.Directory.Exists(settings.PackagesDirectoryPath))
        {
            return ValidationResult.Error("--packages-directory must be a valid path");
        }

        if (!Uri.TryCreate(settings.TargetUri, UriKind.Absolute, out var targetUri))
        {
            return ValidationResult.Error("--target-uri must be a valid URI");
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return ValidationResult.Error("--api-key must be supplied");
        }

        if (string.IsNullOrWhiteSpace(settings.EncryptionKey))
        {
            return ValidationResult.Error("--encryption-key must be supplied");
        }

        if (settings.InstallationStatusTimeout < 0)
        {
            return ValidationResult.Error("--installation-status-timeout must be non-negative");
        }

        if (!Enum.GetValues<LogLevel>().Contains(settings.LogLevel))
        {
            return ValidationResult.Error("--log-level must be a valid log level");
        }

        return ValidationResult.Success();
    }
}
