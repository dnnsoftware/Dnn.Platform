// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Repository;
    using Newtonsoft.Json;

    public class ImportController : BaseController
    {
        public int QueueOperation(int userId, ImportDto importDto)
        {
            using (var repository = new ExportImportRepository(GetPackageDbPath(importDto.PackageId)))
            {
                var exportDto = repository.GetSingleItem<ExportDto>();
                importDto.ExportDto = exportDto;
            }

            importDto.ExportFileInfo =
                GetExportFileInfo(Path.Combine(ExportFolder, importDto.PackageId, Constants.ExportManifestName));
            var dataObject = JsonConvert.SerializeObject(importDto);
            var jobId = DataProvider.Instance().AddNewJob(
                importDto.PortalId, userId, JobType.Import, null, null, importDto.PackageId, dataObject);

            // Run the scheduler if required.
            if (importDto.RunNow)
            {
                EntitiesController.Instance.RunSchedule();
            }

            this.AddEventLog(importDto.PortalId, userId, jobId, Constants.LogTypeSiteImport);
            return jobId;
        }

        /// <summary>
        /// Get list of packages to import.
        /// </summary>
        /// <param name="total">Total number of packages.</param>
        /// <param name="keyword">Keyword to search the import package. This will look into the package name and description.</param>
        /// <param name="order">Order by which the packages list should be sorted. Allowed values: newest, oldest, name.</param>
        /// <param name="pageIndex">Page index to get.</param>
        /// <param name="pageSize">Page size. Should not be more than 100.</param>
        /// <returns></returns>
        public IEnumerable<ImportPackageInfo> GetImportPackages(out int total, string keyword, string order = "newest",
            int pageIndex = 0, int pageSize = 10)
        {
            pageSize = pageSize > 100 ? 100 : pageSize;
            var directories = Directory.GetDirectories(ExportFolder);
            var importPackages = from directory in directories.Where(IsValidImportFolder)
                                 select GetPackageInfo(Path.Combine(directory, Constants.ExportManifestName));

            var importPackagesList = importPackages as IList<ImportPackageInfo> ?? importPackages.ToList();

            importPackages = !string.IsNullOrEmpty(keyword)
                ? importPackagesList.Where(this.GetImportPackageFilterFunc(keyword))
                : importPackagesList;

            total = importPackages.Count();
            string sortOrder;
            var orderByFunc = this.GetImportPackageOrderByFunc(order, out sortOrder);
            importPackages = sortOrder == "asc"
                ? importPackages.OrderBy(orderByFunc)
                : importPackages.OrderByDescending(orderByFunc);
            return importPackages.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public bool VerifyImportPackage(string packageId, ImportExportSummary summary, out string errorMessage)
        {
            bool isValid;
            errorMessage = string.Empty;
            var importFolder = Path.Combine(ExportFolder, packageId);
            if (!IsValidImportFolder(importFolder))
            {
                return false;
            }

            var dbPath = UnPackDatabase(importFolder);
            try
            {
                using (var ctx = new ExportImportRepository(dbPath))
                {
                    if (summary != null)
                    {
                        BuildJobSummary(packageId, ctx, summary);
                    }

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
            if (!IsValidImportFolder(importFolder))
            {
                return null;
            }

            var dbPath = UnPackDatabase(importFolder);
            return dbPath;
        }

        private static string UnPackDatabase(string folderPath)
        {
            var dbName = Path.Combine(folderPath, Constants.ExportDbName);
            if (File.Exists(dbName))
            {
                return dbName;
            }

            var zipDbName = Path.Combine(folderPath, Constants.ExportZipDbName);
            CompressionUtil.UnZipFileFromArchive(Constants.ExportDbName, zipDbName, folderPath, false);
            return dbName;
        }

        private static bool IsValidImportFolder(string folderPath)
        {
            return File.Exists(Path.Combine(folderPath, Constants.ExportManifestName)) && (File.Exists(Path.Combine(folderPath, Constants.ExportDbName)) ||
                   File.Exists(Path.Combine(folderPath, Constants.ExportZipDbName)));
        }

        private Func<ImportPackageInfo, bool> GetImportPackageFilterFunc(string keyword)
        {
            Func<ImportPackageInfo, bool> keywordFunc =
                packageInfo =>
                    packageInfo.Name.ToLowerInvariant().Contains(keyword.ToLowerInvariant()) ||
                    packageInfo.Description.ToLowerInvariant().Contains(keyword.ToLowerInvariant());
            return keywordFunc;
        }

        private Func<ImportPackageInfo, object> GetImportPackageOrderByFunc(string orderBy, out string order)
        {
            orderBy = orderBy.ToLowerInvariant();
            order = orderBy == "newest" ? "desc" : "asc";
            Func<ImportPackageInfo, object> sortFunc;
            switch (orderBy)
            {
                case "name":
                    sortFunc = packageInfo => packageInfo.Name;
                    break;
                default:
                    sortFunc = packageInfo => packageInfo.ExporTime;
                    break;
            }

            return sortFunc;
        }
    }
}
