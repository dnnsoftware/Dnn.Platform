using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Common;
using Dnn.ExportImport.Components.Common;
using System;
using System.Collections.Generic;
using System.IO;
using Dnn.ExportImport.Dto.Pages;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.UI.Skins;

namespace Dnn.ExportImport.Components.Services
{
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
            if (CheckCancelled(exportJob)) return;
            //Skip the export if all the folders have been processed already.
            if (CheckPoint.Stage >= 1)
                return;

            _exportImportJob = exportJob;
            _portalSettings = new PortalSettings(exportJob.PortalId);

            //Create Zip File to hold files
            var currentIndex = 0;
            var totalThemesExported = 0;
            try
            {
                var packagesZipFileFormat = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{{0}}\\{Constants.ExportZipThemes}";
                var packagesZipFile = string.Format(packagesZipFileFormat, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));

                if (CheckPoint.Stage == 0)
                {
                    //export skin packages.
                    var exportThemes = GetExportThemes();
                    var totalThemes = exportThemes.Count;

                    //Update the total items count in the check points. This should be updated only once.
                    CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalThemes : CheckPoint.TotalItems;
                    if (CheckPointStageCallback(this)) return;

                    foreach (var theme in exportThemes)
                    {
                        var filePath = SkinController.FormatSkinSrc(theme, _portalSettings);
                        var physicalPath = Path.Combine(Globals.ApplicationMapPath, filePath.TrimStart('/'));
                        if (Directory.Exists(physicalPath))
                        {
                            foreach (var file in Directory.GetFiles(physicalPath, "*.*", SearchOption.AllDirectories))
                            {
                                var folderOffset = Path.Combine(Globals.ApplicationMapPath, "Portals").Length + 1;
                                CompressionUtil.AddFileToArchive(file, packagesZipFile, folderOffset);
                            }
                            totalThemesExported += 1;
                        }

                        CheckPoint.ProcessedItems++;
                        CheckPoint.Progress = CheckPoint.ProcessedItems * 100.0 / totalThemes;
                        currentIndex++;
                        //After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                        if (currentIndex % 10 == 0 && CheckPointStageCallback(this)) return;
                    }
                    CheckPoint.Stage++;
                    CheckPoint.Completed = true;
                    CheckPoint.Progress = 100;
                }
            }
            finally
            {
                CheckPointStageCallback(this);
                Result.AddSummary("Exported Themes", totalThemesExported.ToString());
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckCancelled(importJob)) return;
            //Skip the export if all the templates have been processed already.
            if (CheckPoint.Stage >= 1)
                return;

            _exportImportJob = importJob;

            var packageZipFile = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{_exportImportJob.Directory.TrimEnd('\\', '/')}\\{Constants.ExportZipThemes}";
            var tempFolder = $"{Path.GetDirectoryName(packageZipFile)}\\{DateTime.Now.Ticks}";
            if (File.Exists(packageZipFile))
            {
                CompressionUtil.UnZipArchive(packageZipFile, tempFolder);
                var exporeFiles = Directory.Exists(tempFolder) ? new string[0] : Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories);
                var portalSettings = new PortalSettings(importDto.PortalId);
                _importCount = exporeFiles.Length;

                CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? exporeFiles.Length : CheckPoint.TotalItems;
                if (CheckPointStageCallback(this)) return;

                if (CheckPoint.Stage == 0)
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

                                Result.AddLogEntry("Import Theme File completed", targetPath);
                                CheckPoint.ProcessedItems++;
                                CheckPoint.Progress = CheckPoint.ProcessedItems * 100.0 / exporeFiles.Length;
                                CheckPointStageCallback(this); // just to update the counts without exit logic
                            }
                            catch (Exception ex)
                            {
                                Result.AddLogEntry("Import Theme error", file);
                                Logger.Error(ex);
                            }
                        }
                        CheckPoint.Stage++;
                    }
                    finally
                    {
                        FileSystemUtils.DeleteFolderRecursive(tempFolder);
                    }
                }
            }
            else
            {
                Result.AddLogEntry("ThemesFileNotFound", "Themes file not found. Skipping themes import", ReportLevel.Warn);
            }
        }

        public override int GetImportTotal()
        {
            return _importCount;
        }

        private IList<string> GetExportThemes()
        {
            var exportThemes = new List<string>();

            //get site level themes
            exportThemes.Add(_portalSettings.DefaultPortalSkin);
            exportThemes.Add(_portalSettings.DefaultPortalContainer);

            if (!exportThemes.Contains(_portalSettings.DefaultAdminSkin))
            {
                exportThemes.Add(_portalSettings.DefaultAdminSkin);
            }

            if (!exportThemes.Contains(_portalSettings.DefaultAdminContainer))
            {
                exportThemes.Add(_portalSettings.DefaultAdminContainer);
            }

            exportThemes.AddRange(LoadExportThemesForPages());
            exportThemes.AddRange(LoadExportContainersForModules());

            //get the theme packages
            var themePackages = new List<string>();
            foreach (var theme in exportThemes)
            {
                var packageName = theme.Substring(0, theme.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase));
                if (!themePackages.Contains(packageName))
                {
                    themePackages.Add(packageName);
                }
            }

            return themePackages;
        }

        private IList<string> LoadExportThemesForPages()
        {
            var exportThemes = new List<string>();

            foreach (var exportTab in Repository.GetAllItems<ExportTab>())
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

            foreach (var module in Repository.GetAllItems<ExportTabModule>())
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
