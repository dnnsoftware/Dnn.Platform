// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Dto.Jobs;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Services;
    using Dnn.ExportImport.Interfaces;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    /// <summary>The import/export controller.</summary>
    public class BaseController
    {
        public static readonly string ExportFolder;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BaseController));

        static BaseController()
        {
            ExportFolder = Globals.ApplicationMapPath + Constants.ExportFolder;
            if (!Directory.Exists(ExportFolder))
            {
                Directory.CreateDirectory(ExportFolder);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="BaseController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IEnumerable<BasePortableService>. Scheduled removal in v12.0.0.")]
        public BaseController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BaseController"/> class.</summary>
        /// <param name="portableServices">The portable services.</param>
        public BaseController(IEnumerable<BasePortableService> portableServices)
        {
            this.PortableServices = portableServices ?? Globals.GetCurrentServiceProvider().GetServices<BasePortableService>();
        }

        protected IEnumerable<BasePortableService> PortableServices { get; }

        /// <summary>Cancels the job.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A value indicating whether the job was found.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool CancelJob(int portalId, int jobId)
        {
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            if (job == null || (job.PortalId != portalId && portalId != -1))
            {
                return false;
            }

            controller.SetJobCancelled(job);
            CachingProvider.Instance().Remove(Util.GetExpImpJobCacheKey(job));
            return true;
        }

        /// <summary>Removes the job.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A value indicating whether the job was found.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool RemoveJob(int portalId, int jobId)
        {
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            if (job == null || (job.PortalId != portalId && portalId != -1))
            {
                return false;
            }

            CachingProvider.Instance().Remove(Util.GetExpImpJobCacheKey(job));

            // if the job is running; then it will create few exceptions in the log file
            controller.RemoveJob(job);
            DeleteJobData(job);
            return true;
        }

        /// <summary>Retrieves one page of paginated processed jobs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="currentPortalId">The current portal ID.</param>
        /// <param name="pageSize">The page size (between <c>1</c> and <c>100</c>).</param>
        /// <param name="pageIndex">The page index (zero-based).</param>
        /// <param name="jobType">The job type.</param>
        /// <param name="keywords">Keywords.</param>
        /// <returns>An <see cref="AllJobsResult"/> instance.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public AllJobsResult GetAllJobs(int portalId, int currentPortalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
        {
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }

            if (pageSize < 1)
            {
                pageSize = 1;
            }
            else if (pageSize > 100)
            {
                pageSize = 100;
            }

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
                Jobs = jobs?.Select(ToJobItem),
            };
        }

        /// <summary>Gets details about a job.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A <see cref="JobItem"/> instance.</returns>
        public JobItem GetJobDetails(int portalId, int jobId)
        {
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            if (portalId != -1 && job?.PortalId != portalId)
            {
                return null;
            }

            var jobItem = ToJobItem(job);
            jobItem.Summary = BuildJobSummary(this.PortableServices, jobId);
            return jobItem;
        }

        /// <summary>
        /// Get the last time a successful export job has started.
        /// This date/time is in uts and can be used to set the next
        /// differential date/time to start the job from.
        /// </summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobType">The job type.</param>
        /// <returns>The last job time.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public DateTime? GetLastJobTime(int portalId, JobType jobType)
        {
            return EntitiesController.Instance.GetLastJobTime(portalId, jobType);
        }

        /// <summary>Builds a job summary.</summary>
        /// <param name="portableServices">The <see cref="BasePortableService"/> implementations.</param>
        /// <param name="packageId">The package ID.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="summary">The summary to build.</param>
        protected internal static void BuildJobSummary(IEnumerable<BasePortableService> portableServices, string packageId, IExportImportRepository repository, ImportExportSummary summary)
        {
            var summaryItems = new SummaryList();
            var exportDto = repository.GetSingleItem<ExportDto>();

            foreach (var implementor in portableServices)
            {
                implementor.Repository = repository;
                summaryItems.Add(new SummaryItem
                {
                    TotalItems = implementor.GetImportTotal(),
                    Category = implementor.Category,
                    Order = implementor.Priority,
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

        /// <summary>Builds a job summary.</summary>
        /// <param name="portableServices">The portable service implementations.</param>
        /// <param name="jobId">The job ID.</param>
        /// <returns>An <see cref="ImportExportSummary"/> instance.</returns>
        protected static ImportExportSummary BuildJobSummary(IEnumerable<BasePortableService> portableServices, int jobId)
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
                    : JsonConvert.DeserializeObject<ImportDto>(job.JobObject).ExportFileInfo,
            };

            var checkpoints = EntitiesController.Instance.GetJobChekpoints(jobId);
            if (!checkpoints.Any())
            {
                return importExportSummary;
            }

            summaryItems.AddRange(checkpoints.Select(checkpoint => new SummaryItem
            {
                TotalItems = checkpoint.TotalItems,
                ProcessedItems = checkpoint.ProcessedItems <= checkpoint.TotalItems ? checkpoint.ProcessedItems : checkpoint.TotalItems,
                ProgressPercentage = Convert.ToInt32(checkpoint.Progress),
                Category = checkpoint.Category,
                Order = portableServices.FirstOrDefault(x => x.Category == checkpoint.Category)?.Priority ?? 0,
                Completed = checkpoint.Completed,
            }));
            importExportSummary.SummaryItems = summaryItems;
            return importExportSummary;
        }

        /// <summary>Gets the export file info.</summary>
        /// <param name="manifestPath">The manifest path.</param>
        /// <returns>An <see cref="ExportFileInfo"/> instance.</returns>
        protected static ExportFileInfo GetExportFileInfo(string manifestPath)
        {
            ImportPackageInfo packageInfo = null;
            Util.ReadJson(manifestPath, ref packageInfo);
            return packageInfo?.Summary.ExportFileInfo;
        }

        /// <summary>Gets the package info.</summary>
        /// <param name="manifestPath">The manifest path.</param>
        /// <returns>An <see cref="ImportPackageInfo"/> instance.</returns>
        protected static ImportPackageInfo GetPackageInfo(string manifestPath)
        {
            ImportPackageInfo packageInfo = null;
            Util.ReadJson(manifestPath, ref packageInfo);
            return packageInfo;
        }

        /// <summary>Adds an event log.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="jobId">The job ID.</param>
        /// <param name="logTypeKey">The log type key.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected void AddEventLog(int portalId, int userId, int jobId, string logTypeKey)
        {
            var objSecurity = PortalSecurity.Instance;
            var portalInfo = PortalController.Instance.GetPortal(portalId);
            var userInfo = UserController.Instance.GetUser(portalId, userId);
            var username = objSecurity.InputFilter(
                userInfo.Username,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);

            var log = new LogInfo
            {
                LogTypeKey = logTypeKey,
                LogPortalID = portalId,
                LogPortalName = portalInfo.PortalName,
                LogUserName = username,
                LogUserID = userId,
            };

            log.AddProperty("JobID", jobId.ToString(CultureInfo.InvariantCulture));
            LogController.Instance.AddLog(log);
        }

        private static JobItem ToJobItem(ExportImportJob job)
        {
            var user = UserController.Instance.GetUserById(job.PortalId, job.CreatedByUserId);
            var name = job.JobType == JobType.Import ? JsonConvert.DeserializeObject<ImportDto>(job.JobObject)?.ExportDto?.ExportName : job.Name;

            return new JobItem
            {
                JobId = job.JobId,
                PortalId = job.PortalId,
                User = user?.DisplayName ?? user?.Username ?? job.CreatedByUserId.ToString(CultureInfo.InvariantCulture),
                JobType = Localization.GetString("JobType_" + job.JobType, Constants.SharedResources),
                Status = (int)job.JobStatus,
                Cancelled = job.IsCancelled,
                JobStatus = Localization.GetString("JobStatus_" + job.JobStatus, Constants.SharedResources),
                Name = name,
                Description = job.Description,
                CreatedOn = job.CreatedOnDate,
                CompletedOn = job.CompletedOnDate,
                ExportFile = job.CompletedOnDate.HasValue ? job.Directory : null,
            };
        }

        private static void DeleteJobData(ExportImportJob job)
        {
            if (job.JobType != JobType.Export)
            {
                return;
            }

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
