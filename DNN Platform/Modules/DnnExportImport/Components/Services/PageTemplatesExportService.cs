// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.IO;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.PageTemplates;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.FileSystem;
    using Newtonsoft.Json;

    using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

    public class PageTemplatesExportService : AssetsExportService
    {
        private readonly string _templatesFolder =
            $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{{0}}\\{Constants.ExportZipTemplates}";

        public override string Category => Constants.Category_Templates;

        public override string ParentCategory => null;

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

            // Create Zip File to hold files
            var skip = this.GetCurrentSkip();
            var currentIndex = skip;
            var totalTemplatesExported = 0;
            var portalId = exportJob.PortalId;
            try
            {
                var templatesFile = string.Format(this._templatesFolder, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));

                if (this.CheckPoint.Stage == 0)
                {
                    var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
                    var toDate = exportDto.ToDateUtc.ToLocalTime();
                    var portal = PortalController.Instance.GetPortal(portalId);

                    var templates =
                        CBO.FillCollection<ExportPageTemplate>(
                            DataProvider.Instance()
                                .GetFiles(portalId, null, toDate, fromDate))
                            .Where(x => x.Extension == Constants.TemplatesExtension)
                            .ToList();
                    var totalTemplates = templates.Count;

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalTemplates : this.CheckPoint.TotalItems;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    foreach (var template in templates)
                    {
                        this.Repository.CreateItem(template, null);
                        totalTemplatesExported += 1;
                        var folderOffset = portal.HomeDirectoryMapPath.Length +
                                           (portal.HomeDirectoryMapPath.EndsWith("\\") ? 0 : 1);

                        var folder = FolderManager.Instance.GetFolder(template.FolderId);
                        CompressionUtil.AddFileToArchive(
                            portal.HomeDirectoryMapPath + folder.FolderPath + this.GetActualFileName(template), templatesFile,
                            folderOffset);

                        this.CheckPoint.ProcessedItems++;
                        this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / totalTemplates;
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
                this.Result.AddSummary("Exported Templates", totalTemplatesExported.ToString());
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (this.CheckCancelled(importJob))
            {
                return;
            }

            // Skip the export if all the templates have been processed already.
            if (this.CheckPoint.Stage >= 2 || this.CheckPoint.Completed)
            {
                return;
            }

            var portalId = importJob.PortalId;
            var templatesFile = string.Format(this._templatesFolder, importJob.Directory.TrimEnd('\\').TrimEnd('/'));
            var totalTemplates = this.GetImportTotal();

            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalTemplates : this.CheckPoint.TotalItems;
            if (this.CheckPointStageCallback(this))
            {
                return;
            }

            if (this.CheckPoint.Stage == 0)
            {
                if (!File.Exists(templatesFile))
                {
                    this.Result.AddLogEntry("TemplatesFileNotFound", "Templates file not found. Skipping templates import",
                        ReportLevel.Warn);
                    this.CheckPoint.Completed = true;
                    this.CheckPointStageCallback(this);
                }
                else
                {
                    var portal = PortalController.Instance.GetPortal(portalId);

                    CompressionUtil.UnZipArchive(templatesFile, portal.HomeDirectoryMapPath,
                        importDto.CollisionResolution == CollisionResolution.Overwrite);

                    this.Result.AddSummary("Imported templates", totalTemplates.ToString());
                    this.CheckPoint.Stage++;
                    this.CheckPoint.StageData = null;
                    this.CheckPoint.Progress = 90;
                    this.CheckPoint.TotalItems = totalTemplates;
                    this.CheckPoint.ProcessedItems = totalTemplates;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }
                }
            }

            if (this.CheckPoint.Stage == 1)
            {
                Func<ExportPageTemplate, object> predicate = x => x.Folder;
                var templates = this.Repository.GetAllItems(predicate).Select(x => x.Folder).Distinct();
                templates.ForEach(x => FolderManager.Instance.Synchronize(importJob.PortalId, x));
                this.CheckPoint.Stage++;
                this.CheckPoint.Completed = true;
                this.CheckPoint.Progress = 100;
                this.CheckPointStageCallback(this);
            }
        }

        public override int GetImportTotal()
        {
            return this.Repository.GetCount<ExportPageTemplate>();
        }

        private string GetActualFileName(ExportPageTemplate objFile)
        {
            return (objFile.StorageLocation == (int)FolderController.StorageLocationTypes.SecureFileSystem)
                ? objFile.FileName + Globals.glbProtectedExtension
                : objFile.FileName;
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
    }
}
