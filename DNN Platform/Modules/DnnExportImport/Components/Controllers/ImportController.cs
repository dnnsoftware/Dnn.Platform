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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Providers;
using Newtonsoft.Json;
using DotNetNuke.Entities.Portals.Internal;
using Dnn.ExportImport.Components.Repository;

namespace Dnn.ExportImport.Components.Controllers
{
    public class ImportController : BaseController
    {

        public int QueueOperation(int userId, ImportDto importDto)
        {
            using (var repository = new ExportImportRepository(GetPackageDbPath(importDto.PackageId)))
            {
                var exportDto = repository.GetSingleItem<ExportDto>();
                importDto.ExportDto = exportDto;
            }

            importDto.ExportFileInfo = new ExportFileInfo();
            var dataObject = JsonConvert.SerializeObject(importDto);
            var jobId = DataProvider.Instance().AddNewJob(
                importDto.PortalId, userId, JobType.Import, null, null, importDto.PackageId, dataObject);
            AddEventLog(importDto.PortalId, userId, jobId, Constants.LogTypeSiteImport);
            return jobId;
        }

        public IEnumerable<ImportPackageInfo> GetImportPackages()
        {
            var directories = Directory.GetDirectories(ExportFolder);
            return (from directory in directories.Where(IsValidImportFolder)
                let dirInfo = new DirectoryInfo(directory)
                select ParseImportManifest(Path.Combine(directory, Constants.ExportManifestName), dirInfo)).ToList();
        }

        public bool VerifyImportPackage(string packageId, ImportExportSummary summary, out string errorMessage)
        {
            bool isValid;
            errorMessage = string.Empty;
            var importFolder = Path.Combine(ExportFolder, packageId);
            if (!IsValidImportFolder(importFolder)) return false;
            var dbPath = UnPackDatabase(importFolder);
            try
            {
                using (var ctx = new ExportImportRepository(dbPath))
                {
                    //TODO: Build the import info from database.
                    if (summary != null)
                        BuildImportSummary(ctx, summary);
                    isValid = true;
                }
            }
            catch (Exception ex)
            {
                isValid = false;
                errorMessage = "Package is not valid. Technical Details:" + ex.Message;
            }
            return isValid;
        }

        private static string GetPackageDbPath(string packageId)
        {
            var importFolder = Path.Combine(ExportFolder, packageId);
            if (!IsValidImportFolder(importFolder)) return null;
            var dbPath = UnPackDatabase(importFolder);
            return dbPath;
        }

        private static string UnPackDatabase(string folderPath)
        {
            //TODO: Error handling
            var dbName = Path.Combine(folderPath, Constants.ExportDbName);
            if (File.Exists(dbName))
                return dbName;
            var zipDbName = Path.Combine(folderPath, Constants.ExportZipDbName);
            CompressionUtil.UnZipFileFromArchive(Constants.ExportDbName, zipDbName, folderPath, false);
            return dbName;
        }

        private static bool IsValidImportFolder(string folderPath)
        {
            return File.Exists(Path.Combine(folderPath, Constants.ExportManifestName)) &&
                   File.Exists(Path.Combine(folderPath, Constants.ExportZipDbName));
        }

        private static ImportPackageInfo ParseImportManifest(string manifestPath, DirectoryInfo importDirectoryInfo)
        {
            using (var reader = PortalTemplateIO.Instance.OpenTextReader(manifestPath))
            {
                var xmlDoc = XDocument.Load(reader);
                return new ImportPackageInfo
                {
                    PackageId = GetTagValue(xmlDoc, "PackageId") ?? importDirectoryInfo.Name,
                    Name = GetTagValue(xmlDoc, "PackageName") ?? importDirectoryInfo.Name,
                    Description = GetTagValue(xmlDoc, "PackageDescription") ?? importDirectoryInfo.Name
                };
            }
        }

        private static string GetTagValue(XDocument xmlDoc, string name)
        {
            return (from f in xmlDoc.Descendants("package")
                select f.Element(name)?.Value).SingleOrDefault();
        }

        private static void BuildImportSummary(IExportImportRepository repository, ImportExportSummary summary)
        {
            var summaryItems = new List<SummaryItem>();
            var implementors = Util.GetPortableImplementors();
            var exportDto = repository.GetSingleItem<ExportDto>();

            foreach (var implementor in implementors)
            {
                implementor.Repository = repository;
                summaryItems.Add(new SummaryItem
                {
                    TotalItems = implementor.GetImportTotal(),
                    Category = implementor.Category,
                    Order = implementor.Priority
                });
            }
            //TODO: Get export file info.
            summary.ExportFileInfo = new ExportFileInfo();
            summary.FromDate = (exportDto.FromDate ?? Constants.MinDbTime).ToUniversalTime().DateTime;
            summary.ToDate = exportDto.ToDate;
            summary.SummaryItems = summaryItems;
            summary.IncludeDeletions = exportDto.IncludeDeletions;
            summary.ExportMode = exportDto.FromDate == Constants.MinDbTime ? ExportMode.Complete : ExportMode.Differential;

            //summary.IncludeExtensions = exportDto.IncludeExtensions;
            //summary.IncludePermissions = exportDto.IncludePermissions;
            summary.IncludeProfileProperties =
                exportDto.ItemsToExport.ToList().Any(x => x == Constants.Category_ProfileProps);
        }
    }
}