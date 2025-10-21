// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Services.Installer.Installers;
using DotNetNuke.Services.Installer.Packages;

using Microsoft.Extensions.DependencyInjection;

/// <summary>The default <see cref="ILocalUpgradeService"/> implementation.</summary>
public class LocalUpgradeService : ILocalUpgradeService
{
    private static readonly List<string> InstallFilter = new List<string>
    {
        "App_Data\\Database.mdf",
        "bin\\",
        "Config\\DotNetNuke.config",
        "Install\\InstallWizard",
        "favicon.ico",
        "robots.txt",
        "web.config",
    };

    private readonly IApplicationInfo application;
    private readonly IApplicationStatusInfo appStatus;
    private readonly IDirectory directory;

    /// <summary>Initializes a new instance of the <see cref="LocalUpgradeService"/> class.</summary>
    /// <param name="application">The application.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="directory">The directory.</param>
    public LocalUpgradeService(IApplicationInfo application, IApplicationStatusInfo appStatus, IDirectory directory)
    {
        this.application = application ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationInfo>();
        this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        this.directory = directory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDirectory>();
    }

    private string UpgradeDirectoryPath => Path.Combine(this.appStatus.ApplicationMapPath, "App_Data", "Upgrade");

    /// <inheritdoc />
    public async Task<IReadOnlyList<LocalUpgradeInfo>> GetLocalUpgrades(CancellationToken cancellationToken)
    {
        IReadOnlyList<LocalUpgradeInfo> localUpgrades;
        if (this.directory.Exists(this.UpgradeDirectoryPath))
        {
            var upgradeFiles = this.directory.GetFiles(this.UpgradeDirectoryPath, "*.zip", SearchOption.TopDirectoryOnly);
            localUpgrades = await Task.WhenAll(upgradeFiles.Select(file => this.GetLocalUpgradeInfo(file, cancellationToken)));
        }
        else
        {
            localUpgrades = [];
        }

        return localUpgrades;
    }

    /// <inheritdoc />
    public async Task<LocalUpgradeInfo> GetLocalUpgradeInfo(string file, CancellationToken cancellationToken)
    {
        try
        {
            using var archiveStream = File.OpenRead(file);
            return await this.GetLocalUpgradeInfo(Path.GetFileNameWithoutExtension(file), archiveStream, cancellationToken);
        }
        catch (Exception exception)
        {
            Exceptions.LogException(exception);
            return new LocalUpgradeInfo
            {
                PackageName = Path.GetFileNameWithoutExtension(file),
                Version = null,
                IsValid = false,
                IsOutdated = false,
            };
        }
    }

    /// <inheritdoc />
    public async Task<LocalUpgradeInfo> GetLocalUpgradeInfo(string packageName, Stream archiveStream, CancellationToken cancellationToken)
    {
        try
        {
            var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);
            var mainAssemblyEntry = archive.FileEntries()
                .Where(entry => string.Equals("DotNetNuke.dll", entry.Name, StringComparison.OrdinalIgnoreCase))
                .Where(entry => string.Equals("bin", Path.GetDirectoryName(entry.FullName), StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();

            Version mainAssemblyVersion = null;
            if (mainAssemblyEntry is not null)
            {
                mainAssemblyVersion = await ReadZippedAssemblyVersion(mainAssemblyEntry, cancellationToken);
            }

            return new LocalUpgradeInfo
            {
                PackageName = packageName,
                Version = mainAssemblyVersion,
                IsValid = mainAssemblyVersion is not null,
                IsOutdated = mainAssemblyVersion is not null && mainAssemblyVersion <= this.application.Version,
            };
        }
        catch (Exception exception)
        {
            Exceptions.LogException(exception);
            return new LocalUpgradeInfo
            {
                PackageName = packageName,
                Version = null,
                IsValid = false,
                IsOutdated = false,
            };
        }
    }

    /// <inheritdoc />
    public async Task StartLocalUpgrade(IReadOnlyList<LocalUpgradeInfo> upgrades, CancellationToken cancellationToken)
    {
        var upgrade = upgrades.Where(u => u.IsValid && !u.IsOutdated).OrderBy(u => u.Version).First();
        await this.StartLocalUpgrade(upgrade, cancellationToken);
    }

    /// <inheritdoc />
    public async Task StartLocalUpgrade(LocalUpgradeInfo upgrade, CancellationToken cancellationToken)
    {
        var upgradeZipPath = Path.Combine(this.UpgradeDirectoryPath, upgrade.PackageName + ".zip");
        using var fileStream = File.OpenRead(upgradeZipPath);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        var assemblyEntries = archive.FileEntries()
            .Where(entry => string.Equals(".dll", Path.GetExtension(entry.Name), StringComparison.OrdinalIgnoreCase))
            .Where(entry => string.Equals("bin", Path.GetDirectoryName(entry.FullName), StringComparison.OrdinalIgnoreCase))
            .ToHashSet();
        var installer = new LocalUpgradeAssemblyInstaller(new InstallerInfo(this.appStatus.ApplicationMapPath, InstallMode.Install) { IgnoreWhiteList = true, });
        await installer.AddFiles(assemblyEntries, cancellationToken);
        installer.Install();
        installer.Commit();

        await FileSystemUtils.UnzipResourcesAsync(
            archive.FileEntries()
                .Where(entry => !assemblyEntries.Contains(entry))
                .Where(entry => !InstallFilter.Any(filter => entry.FullName.StartsWith(filter, StringComparison.OrdinalIgnoreCase))),
            this.appStatus.ApplicationMapPath,
            cancellationToken);
    }

    private static async Task<Version> ReadZippedAssemblyVersion(ZipArchiveEntry assemblyEntry, CancellationToken cancellationToken)
    {
        var tempPath = Path.Combine(Globals.InstallMapPath, "Temp", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        var tempAssemblyPath = Path.Combine(tempPath, assemblyEntry.Name);

        Directory.CreateDirectory(tempPath);
        try
        {
            using (var tempAssemblyFileStream = File.Create(tempAssemblyPath))
            using (var assemblyStream = assemblyEntry.Open())
            {
                const int DefaultBufferSize = 81920;
                await assemblyStream.CopyToAsync(
                    tempAssemblyFileStream,
                    DefaultBufferSize,
                    cancellationToken);
            }

            return AssemblyName.GetAssemblyName(tempAssemblyPath).Version;
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }

    private class LocalUpgradeAssemblyInstaller : AssemblyInstaller
    {
        public LocalUpgradeAssemblyInstaller(InstallerInfo installerInfo)
        {
            this.Package = new PackageInfo();
            this.Package.AttachInstallerInfo(installerInfo);
        }

        public async Task AddFiles(IEnumerable<ZipArchiveEntry> assemblyEntries, CancellationToken cancellationToken)
        {
            foreach (var entry in assemblyEntries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var installFile = new InstallFile(entry, this.Package.InstallerInfo);
                installFile.SetVersion(await ReadZippedAssemblyVersion(entry, cancellationToken));
                this.Files.Add(installFile);
            }
        }
    }
}
