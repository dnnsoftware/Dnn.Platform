using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using System.Linq;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Common;
using Dnn.ExportImport.Components.Common;
using System;
using Dnn.ExportImport.Dto.PageTemplates;
using DotNetNuke.Collections;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    public class PageTemplatesExportService : AssetsExportService
    {
        private readonly string _templatesFolder =
            $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{{0}}\\{Constants.ExportZipTemplates}";

        public override string Category => Constants.Category_Templates;

        public override string ParentCategory => null;

        public override uint Priority => 10;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (CheckCancelled(exportJob)) return;
            //Skip the export if all the folders have been processed already.
            if (CheckPoint.Stage >= 1)
                return;

            //Create Zip File to hold files
            var skip = GetCurrentSkip();
            var currentIndex = skip;
            var totalTemplatesExported = 0;
            var portalId = exportJob.PortalId;
            try
            {
                var templatesFile = string.Format(_templatesFolder, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));

                if (CheckPoint.Stage == 0)
                {
                    var fromDate = exportDto.FromDate?.DateTime;
                    var toDate = exportDto.ToDate;
                    var portal = PortalController.Instance.GetPortal(portalId);

                    var templates =
                        CBO.FillCollection<ExportPageTemplate>(
                            DataProvider.Instance()
                                .GetFiles(portalId, null, toDate, fromDate))
                            .Where(x => x.Extension == Constants.TemplatesExtension)
                            .ToList();
                    var totalTemplates = templates.Count;

                    //Update the total items count in the check points. This should be updated only once.
                    CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalTemplates : CheckPoint.TotalItems;
                    if (CheckPointStageCallback(this)) return;

                    foreach (var template in templates)
                    {
                        Repository.CreateItem(template, null);
                        totalTemplatesExported += 1;
                        var folderOffset = portal.HomeDirectoryMapPath.Length +
                                           (portal.HomeDirectoryMapPath.EndsWith("\\") ? 0 : 1);

                        var folder = FolderManager.Instance.GetFolder(template.FolderId);
                        CompressionUtil.AddFileToArchive(
                            portal.HomeDirectoryMapPath + folder.FolderPath + GetActualFileName(template), templatesFile,
                            folderOffset);

                        CheckPoint.ProcessedItems++;
                        CheckPoint.Progress = CheckPoint.ProcessedItems * 100.0 / totalTemplates;
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
                Result.AddSummary("Exported Templates", totalTemplatesExported.ToString());
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckCancelled(importJob)) return;
            //Skip the export if all the templates have been processed already.
            if (CheckPoint.Stage >= 2)
                return;

            var portalId = importJob.PortalId;
            var templatesFile = string.Format(_templatesFolder, importJob.Directory.TrimEnd('\\').TrimEnd('/'));
            var totalTemplates = GetImportTotal();

            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalTemplates : CheckPoint.TotalItems;
            if (CheckPointStageCallback(this)) return;

            if (CheckPoint.Stage == 0)
            {
                var portal = PortalController.Instance.GetPortal(portalId);

                CompressionUtil.UnZipArchive(templatesFile, portal.HomeDirectoryMapPath,
                    importDto.CollisionResolution == CollisionResolution.Overwrite);

                Result.AddSummary("Imported templates", totalTemplates.ToString());
                CheckPoint.Stage++;
                CheckPoint.StageData = null;
                CheckPoint.Progress = 90;
                CheckPoint.TotalItems = totalTemplates;
                CheckPoint.ProcessedItems = totalTemplates;
                if (CheckPointStageCallback(this)) return;
            }
            if (CheckPoint.Stage == 1)
            {
                Func<ExportPageTemplate, object> predicate = x => x.Folder;
                var templates = Repository.GetAllItems(predicate).Select(x => x.Folder).Distinct();
                templates.ForEach(x => FolderManager.Instance.Synchronize(importJob.PortalId, x));
                CheckPoint.Stage++;
                CheckPoint.Completed = true;
                CheckPoint.Progress = 100;
                CheckPointStageCallback(this);
            }
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportPageTemplate>();
        }

        private string GetActualFileName(ExportPageTemplate objFile)
        {
            return (objFile.StorageLocation == (int)FolderController.StorageLocationTypes.SecureFileSystem)
                ? objFile.FileName + Globals.glbProtectedExtension
                : objFile.FileName;
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
    }
}
