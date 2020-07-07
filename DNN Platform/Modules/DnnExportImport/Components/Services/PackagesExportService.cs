// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.Pages;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using Newtonsoft.Json;

    public class PackagesExportService : BasePortableService
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PackagesExportService));

        private static readonly Regex ExtensionPackageFilesRegex = new Regex(@"^(.+?)_(.+?)_(\d+\.\d+\.\d+).resources$", RegexOptions.Compiled);

        private ExportImportJob _exportImportJob;

        public override string Category => Constants.Category_Packages;

        public override string ParentCategory => null;

        public override uint Priority => 18; // execute before pages service.

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            // Skip the export if all the folders have been processed already.
            if (this.CheckPoint.Stage >= 1)
            {
                return;
            }

            // Create Zip File to hold files
            var skip = this.GetCurrentSkip();
            var currentIndex = skip;
            var totalPackagesExported = 0;
            try
            {
                var packagesZipFileFormat = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{{0}}\\{Constants.ExportZipPackages}";
                var packagesZipFile = string.Format(packagesZipFileFormat, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));

                if (this.CheckPoint.Stage == 0)
                {
                    var fromDate = exportDto.FromDateUtc ?? Constants.MinDbTime;
                    var toDate = exportDto.ToDateUtc;

                    // export skin packages.
                    var extensionPackagesBackupFolder = Path.Combine(Globals.ApplicationMapPath, DotNetNuke.Services.Installer.Util.BackupInstallPackageFolder);
                    var skinPackageFiles = Directory.GetFiles(extensionPackagesBackupFolder).Where(f => this.IsValidPackage(f, fromDate, toDate)).ToList();
                    var totalPackages = skinPackageFiles.Count;

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalPackages : this.CheckPoint.TotalItems;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    foreach (var file in skinPackageFiles)
                    {
                        var exportPackage = this.GenerateExportPackage(file);
                        if (exportPackage != null)
                        {
                            this.Repository.CreateItem(exportPackage, null);
                            totalPackagesExported += 1;
                            var folderOffset = Path.GetDirectoryName(file)?.Length + 1;

                            CompressionUtil.AddFileToArchive(file, packagesZipFile, folderOffset.GetValueOrDefault(0));
                        }

                        this.CheckPoint.ProcessedItems++;
                        this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / totalPackages;
                        currentIndex++;

                        // After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                        if (currentIndex % 10 == 0 && this.CheckPointStageCallback(this))
                        {
                            return;
                        }
                    }

                    this.CheckPoint.Stage++;
                    currentIndex = 0;
                    this.CheckPoint.Completed = true;
                    this.CheckPoint.Progress = 100;
                }
            }
            finally
            {
                this.CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex }) : null;
                this.CheckPointStageCallback(this);
                this.Result.AddSummary("Exported Packages", totalPackagesExported.ToString());
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (this.CheckCancelled(importJob))
            {
                return;
            }

            // Skip the export if all the templates have been processed already.
            if (this.CheckPoint.Stage >= 1 || this.CheckPoint.Completed)
            {
                return;
            }

            this._exportImportJob = importJob;

            this.ProcessImportModulePackages(importDto);
        }

        public override int GetImportTotal()
        {
            return this.Repository.GetCount<ExportPackage>();
        }

        public void InstallPackage(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                try
                {
                    var installer = GetInstaller(stream);

                    if (installer.IsValid)
                    {
                        // Reset Log
                        installer.InstallerInfo.Log.Logs.Clear();

                        // Set the IgnnoreWhiteList flag
                        installer.InstallerInfo.IgnoreWhiteList = true;

                        // Set the Repair flag
                        installer.InstallerInfo.RepairInstall = true;

                        // Install
                        installer.Install();
                    }
                }
                catch (Exception ex)
                {
                    this.Result.AddLogEntry("Import Package error", $"{filePath}. ERROR: {ex.Message}");
                    Logger.Error(ex);
                }
            }
        }

        private static Installer GetInstaller(Stream stream)
        {
            var installer = new Installer(stream, Globals.ApplicationMapPath, false, false)
            {
                InstallerInfo = { PortalID = Null.NullInteger },
            };

            // Read the manifest
            if (installer.InstallerInfo.ManifestFile != null)
            {
                installer.ReadManifest(true);
            }

            return installer;
        }

        private int GetCurrentSkip()
        {
            if (!string.IsNullOrEmpty(this.CheckPoint.StageData))
            {
                dynamic stageData = JsonConvert.DeserializeObject(this.CheckPoint.StageData);
                return Convert.ToInt32(stageData.skip) ?? 0;
            }

            return 0;
        }

        private bool IsValidPackage(string filePath, DateTime fromDate, DateTime toDate)
        {
            var fileInfo = new FileInfo(filePath);
            if (string.IsNullOrEmpty(fileInfo.Name) || fileInfo.LastWriteTimeUtc < fromDate || fileInfo.LastWriteTimeUtc > toDate)
            {
                return false;
            }

            return fileInfo.Name.StartsWith("Skin_") || fileInfo.Name.StartsWith("Container_");
        }

        private ExportPackage GenerateExportPackage(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var match = ExtensionPackageFilesRegex.Match(fileName);
            if (!match.Success)
            {
                return null;
            }

            var packageType = match.Groups[1].Value;
            var packageName = match.Groups[2].Value;
            var version = new Version(match.Groups[3].Value);

            return new ExportPackage { PackageFileName = fileName, PackageName = packageName, PackageType = packageType, Version = version };
        }

        private void ProcessImportModulePackage(ExportPackage exportPackage, string tempFolder, CollisionResolution collisionResolution)
        {
            try
            {
                var filePath = Path.Combine(tempFolder, exportPackage.PackageFileName);
                if (!File.Exists(filePath))
                {
                    return;
                }

                var packageType = exportPackage.PackageType;
                var packageName = exportPackage.PackageName;
                var version = exportPackage.Version;

                var existPackage = PackageController.Instance.GetExtensionPackage(
                    Null.NullInteger,
                    p => p.PackageType == packageType && p.Name == packageName);
                if (existPackage != null &&
                    (existPackage.Version > version ||
                     (existPackage.Version == version &&
                      collisionResolution == CollisionResolution.Ignore)))
                {
                    this.Result.AddLogEntry(
                        "Import Package ignores",
                        $"{packageName} has higher version {existPackage.Version} installed, ignore import it");
                    return;
                }

                this.InstallPackage(filePath);
                this.Result.AddLogEntry("Import Package completed", $"{packageName} version: {version}");
            }
            catch (Exception ex)
            {
                this.Result.AddLogEntry(
                    "Import Package error",
                    $"{exportPackage.PackageName} : {exportPackage.Version} - {ex.Message}");
                Logger.Error(ex);
            }
        }

        private void ProcessImportModulePackages(ImportDto importDto)
        {
            var packageZipFile = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{this._exportImportJob.Directory.TrimEnd('\\', '/')}\\{Constants.ExportZipPackages}";
            var tempFolder = $"{Path.GetDirectoryName(packageZipFile)}\\{DateTime.Now.Ticks}";
            if (File.Exists(packageZipFile))
            {
                CompressionUtil.UnZipArchive(packageZipFile, tempFolder);
                var exportPackages = this.Repository.GetAllItems<ExportPackage>().ToList();

                this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? exportPackages.Count : this.CheckPoint.TotalItems;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }

                if (this.CheckPoint.Stage == 0)
                {
                    try
                    {
                        foreach (var exportPackage in exportPackages)
                        {
                            this.ProcessImportModulePackage(exportPackage, tempFolder, importDto.CollisionResolution);

                            this.CheckPoint.ProcessedItems++;
                            this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / exportPackages.Count;
                            if (this.CheckPointStageCallback(this))
                            {
                                break;
                            }
                        }

                        this.CheckPoint.Stage++;
                        this.CheckPoint.Completed = true;
                    }
                    finally
                    {
                        this.CheckPointStageCallback(this);
                        try
                        {
                            FileSystemUtils.DeleteFolderRecursive(tempFolder);
                        }
                        catch (Exception)
                        {
                            // ignore
                        }
                    }
                }
            }
            else
            {
                this.CheckPoint.Completed = true;
                this.CheckPointStageCallback(this);
                this.Result.AddLogEntry("PackagesFileNotFound", "Packages file not found. Skipping packages import", ReportLevel.Warn);
            }
        }
    }
}
