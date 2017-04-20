using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using System.Linq;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Common;
using Dnn.ExportImport.Components.Common;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Dnn.ExportImport.Dto.Pages;
using Dnn.ExportImport.Dto.PageTemplates;
using DotNetNuke.Collections;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    public class PackagesExportService : BasePortableService
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PackagesExportService));

        private readonly string _packagesZipFile = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{{0}}\\{Constants.ExportZipPackages}";
        private static readonly string ExtensionPackagesBackupFolder = Path.Combine(Globals.ApplicationMapPath, DotNetNuke.Services.Installer.Util.BackupInstallPackageFolder);
        private static readonly Regex ExtensionPackageFilesRegex = new Regex("^(.+?)_(.+?)_(\\d+\\.\\d+\\.\\d+).resources$", RegexOptions.Compiled);

        private ExportImportJob _exportImportJob;

        public override string Category => Constants.Category_Packages;

        public override string ParentCategory => null;

        public override uint Priority => 18; //execute before pages service.

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (CheckCancelled(exportJob)) return;
            //Skip the export if all the folders have been processed already.
            if (CheckPoint.Stage >= 1)
                return;

            //Create Zip File to hold files
            var skip = GetCurrentSkip();
            var currentIndex = skip;
            var totalPackagesExported = 0;
            var portalId = exportJob.PortalId;
            try
            {
                var packagesZipFile = string.Format(_packagesZipFile, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));

                if (CheckPoint.Stage == 0)
                {
                    var fromDate = (exportDto.FromDate?.DateTime).GetValueOrDefault(DateTime.MinValue).ToLocalTime();
                    var toDate = exportDto.ToDate.ToLocalTime();

                    //export skin packages.
                    var skinPackageFiles = Directory.GetFiles(ExtensionPackagesBackupFolder).Where(f => IsValidPackage(f, fromDate, toDate)).ToList();
                    var totalPackages = skinPackageFiles.Count;

                    //Update the total items count in the check points. This should be updated only once.
                    CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalPackages : CheckPoint.TotalItems;
                    if (CheckPointStageCallback(this)) return;

                    foreach (var file in skinPackageFiles)
                    {
                        var exportPackage = GenerateExportPackage(file);
                        if (exportPackage != null)
                        {
                            Repository.CreateItem(exportPackage, null);
                            totalPackagesExported += 1;
                            var folderOffset = Path.GetDirectoryName(file)?.Length + 1;

                            CompressionUtil.AddFileToArchive(file, packagesZipFile, folderOffset.GetValueOrDefault(0));
                        }

                        CheckPoint.ProcessedItems++;
                        CheckPoint.Progress = CheckPoint.ProcessedItems * 100.0 / totalPackages;
                        currentIndex++;
                        //After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                        if (currentIndex % 10 == 0 && CheckPointStageCallback(this)) return;
                    }
                    CheckPoint.Stage++;
                    currentIndex = 0;
                    CheckPoint.Completed = true;
                    CheckPoint.Progress = 100;
                }
            }
            finally
            {
                CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex }) : null;
                CheckPointStageCallback(this);
                Result.AddSummary("Exported Packages", totalPackagesExported.ToString());
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckCancelled(importJob)) return;
            //Skip the export if all the templates have been processed already.
            if (CheckPoint.Stage >= 2)
                return;

            _exportImportJob = importJob;

            ProcessImportModulePackages();
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportPageTemplate>();
        }

        private int GetCurrentSkip()
        {
            if (!string.IsNullOrEmpty(CheckPoint.StageData))
            {
                dynamic stageData = JsonConvert.DeserializeObject(CheckPoint.StageData);
                return Convert.ToInt32(stageData.skip) ?? 0;
            }
            return 0;
        }

        private bool IsValidPackage(string filePath, DateTime fromDate, DateTime toDate)
        {
            var fileInfo = new System.IO.FileInfo(filePath);
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

            return new ExportPackage {PackageFileName = fileName, PackageName = packageName, PackageType = packageType, Version = version};
        }

        private void ProcessImportModulePackages()
        {
            var packageZipFile = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{_exportImportJob.Directory.TrimEnd('\\', '/')}\\{Constants.ExportZipPackages}";
            var tempFolder = $"{Path.GetDirectoryName(packageZipFile)}\\{DateTime.Now.Ticks}";
            CompressionUtil.UnZipArchive(packageZipFile, tempFolder, true);
            var exportPackages = Repository.GetAllItems<ExportPackage>().ToList();

            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? exportPackages.Count() : CheckPoint.TotalItems;
            if (CheckPointStageCallback(this)) return;

            if (CheckPoint.Stage == 0)
            {
                try
                {
                    foreach (var exportPackage in exportPackages)
                    {
                        var filePath = Path.Combine(tempFolder, exportPackage.PackageFileName);
                        if (!File.Exists(filePath))
                        {
                            continue;
                        }

                        var packageType = exportPackage.PackageType;
                        var packageName = exportPackage.PackageName;
                        var version = exportPackage.Version;

                        Logger.Info($"Start Import Package: {packageName} version: {version}");

                        var existPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageType == packageType && p.Name == packageName);
                        if (existPackage != null && existPackage.Version >= version)
                        {
                            Logger.Info($"Import Package: {packageName} has higher version {existPackage.Version} installed, ignore import it");
                            continue;
                        }

                        InstallPackage(filePath);

                        Logger.Info($"Complete Import Package: {packageName}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                finally
                {
                    FileSystemUtils.DeleteFolderRecursive(tempFolder);
                }
            }
        }

        public void InstallPackage(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                try
                {
                    var fileName = Path.GetFileName(filePath);
                    var installer = GetInstaller(stream, fileName, Null.NullInteger);

                    if (installer.IsValid)
                    {
                        //Reset Log
                        installer.InstallerInfo.Log.Logs.Clear();

                        //Set the IgnnoreWhiteList flag
                        installer.InstallerInfo.IgnoreWhiteList = true;

                        //Set the Repair flag
                        installer.InstallerInfo.RepairInstall = true;

                        //Install
                        installer.Install();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private static Installer GetInstaller(Stream stream, string fileName, int portalId)
        {
            var installer = new Installer(stream, Globals.ApplicationMapPath, false, false);
            installer.InstallerInfo.PortalID = Null.NullInteger;

            //Read the manifest
            if (installer.InstallerInfo.ManifestFile != null)
            {
                installer.ReadManifest(true);
            }

            return installer;
        }


    }
}
