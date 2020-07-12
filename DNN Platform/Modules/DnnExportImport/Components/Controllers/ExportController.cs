// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Controllers
{
    using System;
    using System.IO;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Providers;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using Newtonsoft.Json;

    public class ExportController : BaseController
    {
        public int QueueOperation(int userId, ExportDto exportDto)
        {
            exportDto.ProductSku = DotNetNuke.Application.DotNetNukeContext.Current.Application.SKU;
            exportDto.ProductVersion = Globals.FormatVersion(DotNetNuke.Application.DotNetNukeContext.Current.Application.Version, true);
            var dbTime = DateUtils.GetDatabaseUtcTime();
            exportDto.ToDateUtc = dbTime.AddMilliseconds(-dbTime.Millisecond);
            var directory = dbTime.ToString("yyyy-MM-dd_HH-mm-ss");
            if (exportDto.ExportMode == ExportMode.Differential)
            {
                exportDto.FromDateUtc = this.GetLastJobTime(exportDto.PortalId, JobType.Export);
            }

            var dataObject = JsonConvert.SerializeObject(exportDto);
            exportDto.IsDirty = false; // This should be set to false for new job.
            var jobId = DataProvider.Instance().AddNewJob(exportDto.PortalId, userId,
                JobType.Export, exportDto.ExportName, exportDto.ExportDescription, directory, dataObject);

            // Run the scheduler if required.
            if (exportDto.RunNow)
            {
                EntitiesController.Instance.RunSchedule();
            }

            this.AddEventLog(exportDto.PortalId, userId, jobId, Constants.LogTypeSiteExport);
            return jobId;
        }

        public void CreatePackageManifest(ExportImportJob exportJob, ExportFileInfo exportFileInfo,
            ImportExportSummary summary)
        {
            var filePath = Path.Combine(ExportFolder, exportJob.Directory, Constants.ExportManifestName);
            var portal = PortalController.Instance.GetPortal(exportJob.PortalId);
            var packageInfo = new ImportPackageInfo
            {
                Summary = summary,
                PackageId = exportJob.Directory,
                Name = exportJob.Name,
                Description = exportJob.Description,
                ExporTime = exportJob.CreatedOnDate,
                PortalName = portal?.PortalName,
            };
            Util.WriteJson(filePath, packageInfo);
        }
    }
}
