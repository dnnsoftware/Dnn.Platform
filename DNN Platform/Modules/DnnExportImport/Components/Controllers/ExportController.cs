#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Providers;
using Newtonsoft.Json;
using System.IO;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

namespace Dnn.ExportImport.Components.Controllers
{
    public class ExportController : BaseController
    {
        public int QueueOperation(int userId, ExportDto exportDto)
        {
            exportDto.ToDate = DateUtils.GetDatabaseTime();
            var directory = DateUtils.GetDatabaseTime().ToString("yyyy-MM-dd-HH-mm-ss");
            if (exportDto.ExportMode == ExportMode.Differential)
            {
                exportDto.FromDate = GetLastJobTime(exportDto.PortalId, JobType.Export);
            }
            var dataObject = JsonConvert.SerializeObject(exportDto);

            var jobId = DataProvider.Instance().AddNewJob(exportDto.PortalId, userId,
                JobType.Export, exportDto.ExportName, exportDto.ExportDescription, directory, dataObject);

            AddEventLog(exportDto.PortalId, userId, jobId, Constants.LogTypeSiteExport);
            return jobId;
        }

        public void CreatePackageManifest(ExportImportJob exportJob, ExportFileInfo exportFileInfo)
        {
            var filePath = Path.Combine(ExportFolder, exportJob.Directory, Constants.ExportManifestName);
            var portal = PortalController.Instance.GetPortal(exportJob.PortalId);
            var tagsToWrite = new Dictionary<string, string>
            {
                {Constants.Manifest_PortalName, portal?.PortalName},
                {Constants.Manifest_ExportTime, exportJob.CreatedOnDate.ToString(CultureInfo.InvariantCulture)},
                {Constants.Manifest_PackageId, exportJob.Directory},
                {Constants.Manifest_PackageName, exportJob.Name},
                {
                    Constants.Manifest_PackageDescription,
                    !string.IsNullOrEmpty(exportJob.Description) ? exportJob.Description : exportJob.Name
                },
                {Constants.Manifest_ExportPath, exportFileInfo.ExportPath},
                {
                    Constants.Manifest_ExportSize,
                    Convert.ToString(exportFileInfo.ExportSizeKb, CultureInfo.InvariantCulture)
                }
            };
            Util.WriteXml(filePath, tagsToWrite, Constants.Manifest_RootTag);
        }
    }
}