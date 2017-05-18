﻿#region Copyright
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
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Jobs;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Log.EventLog;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Interfaces;
using DotNetNuke.Common;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Controllers
{
    public class BaseController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BaseController));

        public static readonly string ExportFolder;

        static BaseController()
        {
            ExportFolder = Globals.ApplicationMapPath + Constants.ExportFolder;
            if (!Directory.Exists(ExportFolder))
            {
                Directory.CreateDirectory(ExportFolder);
            }
        }

        protected void AddEventLog(int portalId, int userId, int jobId, string logTypeKey)
        {
            var objSecurity = new PortalSecurity();
            var portalInfo = PortalController.Instance.GetPortal(portalId);
            var userInfo = UserController.Instance.GetUser(portalId, userId);
            var username = objSecurity.InputFilter(userInfo.Username,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);

            var log = new LogInfo
            {
                LogTypeKey = logTypeKey,
                LogPortalID = portalId,
                LogPortalName = portalInfo.PortalName,
                LogUserName = username,
                LogUserID = userId,
            };

            log.AddProperty("JobID", jobId.ToString());
            LogController.Instance.AddLog(log);
        }

        public bool CancelJob(int portalId, int jobId)
        {
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            if (job == null || (job.PortalId != portalId && portalId != -1))
                return false;

            controller.SetJobCancelled(job);
            CachingProvider.Instance().Remove(Util.GetExpImpJobCacheKey(job));
            return true;
        }

        public bool RemoveJob(int portalId, int jobId)
        {
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            if (job == null || (job.PortalId != portalId && portalId != -1))
                return false;

            CachingProvider.Instance().Remove(Util.GetExpImpJobCacheKey(job));
            // if the job is running; then it will create few exceptions in the log file
            controller.RemoveJob(job);
            DeleteJobData(job);
            return true;
        }

        /// <summary>
        /// Retrieves one page of paginated proceessed jobs
        /// </summary>
        public AllJobsResult GetAllJobs(int portalId, int currentPortalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
        {
            if (pageIndex < 0) pageIndex = 0;
            if (pageSize < 1) pageSize = 1;
            else if (pageSize > 100) pageSize = 100;

            var count = EntitiesController.Instance.GetAllJobsCount(portalId, jobType, keywords);
            var jobs = count <= 0
                ? null
                : EntitiesController.Instance.GetAllJobs(portalId, pageSize, pageIndex, jobType, keywords);

            var portal = PortalController.Instance.GetPortal(currentPortalId);
            return new AllJobsResult
            {
                LastExportTime = portalId > -1 ? EntitiesController.Instance.GetLastJobTime(portalId, JobType.Export) : null,
                LastImportTime = portalId > -1 ? EntitiesController.Instance.GetLastJobTime(portalId, JobType.Import) : null,
                PortalId = portalId,
                PortalName = portal.PortalName,
                TotalJobs = count,
                Jobs = jobs?.Select(ToJobItem)
            };
        }

        public JobItem GetJobDetails(int portalId, int jobId)
        {
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            if (portalId != -1 && job?.PortalId != portalId)
                return null;

            var jobItem = ToJobItem(job);
            jobItem.Summary = BuildJobSummary(jobId);
            return jobItem;
        }

        /// <summary>
        /// Get the last time a successful export job has started.
        /// This date/time is in uts and can be used to set the next
        /// differntial date/time to start the job from.
        /// </summary>
        /// <returns></returns>
        public DateTime? GetLastJobTime(int portalId, JobType jobType)
        {
            return EntitiesController.Instance.GetLastJobTime(portalId, jobType);
        }

        protected static ImportExportSummary BuildJobSummary(int jobId)
        {
            var summaryItems = new SummaryList();
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            var exportDto = job.JobType == JobType.Export
                ? JsonConvert.DeserializeObject<ExportDto>(job.JobObject)
                : JsonConvert.DeserializeObject<ImportDto>(job.JobObject).ExportDto;

            var importExportSummary = new ImportExportSummary
            {
                IncludeDeletions = exportDto.IncludeDeletions,
                IncludeExtensions = exportDto.IncludeExtensions,
                IncludePermissions = exportDto.IncludePermissions,
                IncludeProfileProperties = exportDto.IncludeProperfileProperties,
                IncludeContent = exportDto.IncludeContent,
                FromDate = exportDto.FromDateUtc,
                ToDate = exportDto.ToDateUtc,
                ExportMode = exportDto.ExportMode,
                ExportFileInfo = job.JobType == JobType.Export
                    ? GetExportFileInfo(Path.Combine(ExportFolder, job.Directory, Constants.ExportManifestName))
                    : JsonConvert.DeserializeObject<ImportDto>(job.JobObject).ExportFileInfo
            };

            var checkpoints = EntitiesController.Instance.GetJobChekpoints(jobId);
            if (!checkpoints.Any()) return importExportSummary;
            var implementors = Util.GetPortableImplementors();

            summaryItems.AddRange(checkpoints.Select(checkpoint => new SummaryItem
            {
                TotalItems = checkpoint.TotalItems,
                ProcessedItems = checkpoint.ProcessedItems,
                ProgressPercentage = Convert.ToInt32(checkpoint.Progress),
                Category = checkpoint.Category,
                Order = implementors.FirstOrDefault(x => x.Category == checkpoint.Category)?.Priority ?? 0,
                Completed = checkpoint.Completed
            }));
            importExportSummary.SummaryItems = summaryItems;
            return importExportSummary;
        }

        protected internal static void BuildJobSummary(string packageId, IExportImportRepository repository, ImportExportSummary summary)
        {
            var summaryItems = new SummaryList();
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

            summary.ExportFileInfo = GetExportFileInfo(Path.Combine(ExportFolder, packageId, Constants.ExportManifestName));
            summary.FromDate = exportDto.FromDateUtc;
            summary.ToDate = exportDto.ToDateUtc;
            summary.SummaryItems = summaryItems;
            summary.IncludeDeletions = exportDto.IncludeDeletions;
            summary.IncludeContent = exportDto.IncludeContent;
            summary.IncludeExtensions = exportDto.IncludeExtensions;
            summary.IncludePermissions = exportDto.IncludePermissions;
            summary.IncludeProfileProperties = exportDto.IncludeProperfileProperties;
            summary.ExportMode = exportDto.ExportMode;
        }

        protected static ExportFileInfo GetExportFileInfo(string manifestPath)
        {
            ImportPackageInfo packageInfo = null;
            Util.ReadJson(manifestPath, ref packageInfo);
            return packageInfo?.Summary.ExportFileInfo;
        }

        protected static ImportPackageInfo GetPackageInfo(string manifestPath)
        {
            ImportPackageInfo packageInfo = null;
            Util.ReadJson(manifestPath, ref packageInfo);
            return packageInfo;
        }

        private static JobItem ToJobItem(ExportImportJob job)
        {
            var user = UserController.Instance.GetUserById(job.PortalId, job.CreatedByUserId);
            var name = job.JobType == JobType.Import ? JsonConvert.DeserializeObject<ImportDto>(job.JobObject)?.ExportDto?.ExportName : job.Name;

            return new JobItem
            {
                JobId = job.JobId,
                PortalId = job.PortalId,
                User = user?.DisplayName ?? user?.Username ?? job.CreatedByUserId.ToString(),
                JobType = Localization.GetString("JobType_" + job.JobType, Constants.SharedResources),
                Status = (int)job.JobStatus,
                Cancelled = job.IsCancelled,
                JobStatus = Localization.GetString("JobStatus_" + job.JobStatus, Constants.SharedResources),
                Name = name,
                Description = job.Description,
                CreatedOn = job.CreatedOnDate,
                CompletedOn = job.CompletedOnDate,
                ExportFile = job.CompletedOnDate.HasValue ? job.Directory : null
            };
        }

        private static void DeleteJobData(ExportImportJob job)
        {
            if (job.JobType != JobType.Export) return;
            var jobFolder = Path.Combine(ExportFolder, job.Directory);
            try
            {
                if (Directory.Exists(jobFolder))
                {
                    Directory.Delete(jobFolder, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Failed to delete the job data. Error:{ex.Message}. It will need to be deleted manually. Folder Path:{jobFolder}");
            }
        }
    }
}