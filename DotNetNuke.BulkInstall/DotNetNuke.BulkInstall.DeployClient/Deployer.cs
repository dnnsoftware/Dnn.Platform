// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

/// <summary>The <see cref="IDeployer"/> implementation.</summary>
public class Deployer : IDeployer
{
    private readonly IRenderer renderer;
    private readonly IPackageFileSource packageFileSource;
    private readonly IInstaller installer;
    private readonly IEncryptor encryptor;
    private readonly IDelayer delayer;

    /// <summary>Initializes a new instance of the <see cref="Deployer"/> class.</summary>
    /// <param name="renderer">The renderer.</param>
    /// <param name="packageFileSource">The package file source.</param>
    /// <param name="installer">The installer.</param>
    /// <param name="encryptor">The encryptor.</param>
    /// <param name="delayer">The delayer.</param>
    public Deployer(IRenderer renderer, IPackageFileSource packageFileSource, IInstaller installer, IEncryptor encryptor, IDelayer delayer)
    {
        this.renderer = renderer;
        this.packageFileSource = packageFileSource;
        this.installer = installer;
        this.encryptor = encryptor;
        this.delayer = delayer;
    }

    /// <inheritdoc/>
    public async Task<ExitCode> StartAsync(DeployInput options)
    {
        try
        {
            this.renderer.Welcome(options.LogLevel);

            var packageFiles = this.packageFileSource.GetPackageFiles(options.PackagesDirectoryPath, options.Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            this.renderer.RenderListOfFiles(options.LogLevel, packageFiles);

            var sessionId = await this.installer.StartSessionAsync(options);

            var uploads = packageFiles.Select(file => (file, this.UploadPackage(sessionId, file, options)));
            await this.renderer.RenderFileUploadsAsync(options.LogLevel, uploads);

            _ = this.installer.InstallPackagesAsync(options, sessionId);

            var hasRenderedOverview = false;
            while (true)
            {
                var session = await this.installer.GetSessionAsync(options, sessionId);
                if (session.Responses != null)
                {
                    if (!hasRenderedOverview)
                    {
                        this.renderer.RenderInstallationOverview(options.LogLevel, session.Responses);
                        hasRenderedOverview = true;
                    }

                    this.renderer.RenderInstallationStatus(options.LogLevel, session.Responses);
                }

                if (session.Status == SessionStatus.Complete)
                {
                    if (session.Responses?.Any(r => !r.Value?.Success == true) == true)
                    {
                        return ExitCode.PackageError;
                    }

                    return ExitCode.Success;
                }

                await this.delayer.Delay(TimeSpan.FromSeconds(1));
            }
        }
        catch (InstallerException e)
        {
            this.renderer.RenderCriticalError(options.LogLevel, e.Message, e.InnerException!);
            return ExitCode.InstallerError;
        }
        catch (Exception e)
        {
            this.renderer.RenderCriticalError(options.LogLevel, "An unexpected error occurred.", e);
            return ExitCode.UnexpectedError;
        }
    }

    private async Task UploadPackage(string sessionId, string packageFile, DeployInput options)
    {
        await using var packageFileStream = this.packageFileSource.GetFileStream(packageFile);
        await using var encryptedPackageStream = await this.encryptor.GetEncryptedStream(options, packageFileStream);

        await this.installer.UploadPackageAsync(options, sessionId, encryptedPackageStream, packageFile);
    }
}
