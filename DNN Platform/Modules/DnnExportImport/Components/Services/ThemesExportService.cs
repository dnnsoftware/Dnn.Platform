// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.Pages;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.UI.Skins;

    public class ThemesExportService : BasePortableService
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesExportService));

        private ExportImportJob _exportImportJob;
        private PortalSettings _portalSettings;
        private int _importCount;

        public override string Category => Constants.Category_Themes;

        public override string ParentCategory => Constants.Category_Pages;

        public override uint Priority => 10;

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

            this._exportImportJob = exportJob;
            this._portalSettings = new PortalSettings(exportJob.PortalId);

            // Create Zip File to hold files
            var currentIndex = 0;
            var totalThemesExported = 0;
            try
            {
                var packagesZipFileFormat = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{{0}}\\{Constants.ExportZipThemes}";
                var packagesZipFile = string.Format(packagesZipFileFormat, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));

                if (this.CheckPoint.Stage == 0)
                {
                    // export skin packages.
                    var exportThemes = this.GetExportThemes();
                    var totalThemes = exportThemes.Count;

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalThemes : this.CheckPoint.TotalItems;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    using (var archive = CompressionUtil.OpenCreate(packagesZipFile))
                    {
                        foreach (var theme in exportThemes)
                        {
                            var filePath = SkinController.FormatSkinSrc(theme, this._portalSettings);
                            var physicalPath = Path.Combine(Globals.ApplicationMapPath, filePath.TrimStart('/'));
                            if (Directory.Exists(physicalPath))
                            {
                                foreach (var file in Directory.GetFiles(physicalPath, "*.*", SearchOption.AllDirectories))
                                {
                                    var folderOffset = Path.Combine(Globals.ApplicationMapPath, "Portals").Length + 1;
                                    CompressionUtil.AddFileToArchive(archive, file, folderOffset);
                                }

                                totalThemesExported += 1;
                            }

                            this.CheckPoint.ProcessedItems++;
                            this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / totalThemes;
                            currentIndex++;

                            // After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                            if (currentIndex % 10 == 0 && this.CheckPointStageCallback(this))
                            {
                                return;
                            }
                        }
                    }

                    this.CheckPoint.Stage++;
                    this.CheckPoint.Completed = true;
                    this.CheckPoint.Progress = 100;
                }
            }
            finally
            {
                this.CheckPointStageCallback(this);
                this.Result.AddSummary("Exported Themes", totalThemesExported.ToString());
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

            var packageZipFile = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{this._exportImportJob.Directory.TrimEnd('\\', '/')}\\{Constants.ExportZipThemes}";
            var tempFolder = $"{Path.GetDirectoryName(packageZipFile)}\\{DateTime.Now.Ticks}";
            if (File.Exists(packageZipFile))
            {
                CompressionUtil.UnZipArchive(packageZipFile, tempFolder);
                var exporeFiles = Directory.Exists(tempFolder) ? Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories) : new string[0];
                var portalSettings = new PortalSettings(importDto.PortalId);
                this._importCount = exporeFiles.Length;

                this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? exporeFiles.Length : this.CheckPoint.TotalItems;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }

                if (this.CheckPoint.Stage == 0)
                {
                    try
                    {
                        foreach (var file in exporeFiles)
                        {
                            try
                            {
                                var checkFolder = file.Replace(tempFolder + "\\", string.Empty).Split('\\')[0];
                                var relativePath = file.Substring((tempFolder + "\\" + checkFolder + "\\").Length);
                                string targetPath;

                                if (checkFolder == "_default")
                                {
                                    targetPath = Path.Combine(Globals.HostMapPath, relativePath);
                                }
                                else if (checkFolder.EndsWith("-System"))
                                {
                                    targetPath = Path.Combine(portalSettings.HomeSystemDirectoryMapPath, relativePath);
                                }
                                else
                                {
                                    targetPath = Path.Combine(portalSettings.HomeDirectoryMapPath, relativePath);
                                }

                                if (!File.Exists(targetPath) ||
                                    importDto.CollisionResolution == CollisionResolution.Overwrite)
                                {
                                    var directory = Path.GetDirectoryName(targetPath);
                                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                                    {
                                        Directory.CreateDirectory(directory);
                                    }

                                    File.Copy(file, targetPath, true);
                                }

                                this.Result.AddLogEntry("Import Theme File completed", targetPath);
                                this.CheckPoint.ProcessedItems++;
                                this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / exporeFiles.Length;
                                this.CheckPointStageCallback(this); // just to update the counts without exit logic
                            }
                            catch (Exception ex)
                            {
                                this.Result.AddLogEntry("Import Theme error", file);
                                Logger.Error(ex);
                            }
                        }

                        this.CheckPoint.Stage++;
                        this.CheckPoint.Completed = true;
                    }
                    finally
                    {
                        this.CheckPointStageCallback(this);
                        FileSystemUtils.DeleteFolderRecursive(tempFolder);
                    }
                }
            }
            else
            {
                this.CheckPoint.Completed = true;
                this.CheckPointStageCallback(this);
                this.Result.AddLogEntry("ThemesFileNotFound", "Themes file not found. Skipping themes import", ReportLevel.Warn);
            }
        }

        public override int GetImportTotal()
        {
            return this._importCount;
        }

        private IList<string> GetExportThemes()
        {
            var exportThemes = new List<string>();

            // get site level themes
            exportThemes.Add(this._portalSettings.DefaultPortalSkin);
            exportThemes.Add(this._portalSettings.DefaultPortalContainer);

            if (!exportThemes.Contains(this._portalSettings.DefaultAdminSkin))
            {
                exportThemes.Add(this._portalSettings.DefaultAdminSkin);
            }

            if (!exportThemes.Contains(this._portalSettings.DefaultAdminContainer))
            {
                exportThemes.Add(this._portalSettings.DefaultAdminContainer);
            }

            exportThemes.AddRange(this.LoadExportThemesForPages());
            exportThemes.AddRange(this.LoadExportContainersForModules());

            // get the theme packages
            var themePackages = new List<string>();
            foreach (var theme in exportThemes)
            {
                var packageName = theme.Substring(0, theme.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase));
                if (!themePackages.Any(p => p.Equals(packageName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    themePackages.Add(packageName);
                }
            }

            return themePackages;
        }

        private IList<string> LoadExportThemesForPages()
        {
            var exportThemes = new List<string>();

            foreach (var exportTab in this.Repository.GetAllItems<ExportTab>())
            {
                if (!string.IsNullOrEmpty(exportTab.SkinSrc) && !exportThemes.Contains(exportTab.SkinSrc))
                {
                    exportThemes.Add(exportTab.SkinSrc);
                }

                if (!string.IsNullOrEmpty(exportTab.ContainerSrc) && !exportThemes.Contains(exportTab.ContainerSrc))
                {
                    exportThemes.Add(exportTab.ContainerSrc);
                }
            }

            return exportThemes;
        }

        private IList<string> LoadExportContainersForModules()
        {
            var exportThemes = new List<string>();

            foreach (var module in this.Repository.GetAllItems<ExportTabModule>())
            {
                if (!string.IsNullOrEmpty(module.ContainerSrc) && !exportThemes.Contains(module.ContainerSrc))
                {
                    exportThemes.Add(module.ContainerSrc);
                }
            }

            return exportThemes;
        }
    }
}
