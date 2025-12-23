// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Engines
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Controllers;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Dto.Jobs;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Models;
    using Dnn.ExportImport.Components.Services;
    using Dnn.ExportImport.Dto;
    using Dnn.ExportImport.Dto.Assets;
    using Dnn.ExportImport.Dto.PageTemplates;
    using Dnn.ExportImport.Dto.Portal;
    using Dnn.ExportImport.Dto.ProfileProperties;
    using Dnn.ExportImport.Dto.Users;
    using Dnn.ExportImport.Interfaces;
    using Dnn.ExportImport.Repository;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Scheduling;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using PlatformDataProvider = DotNetNuke.Data.DataProvider;

    /// <summary>The import/export engine.</summary>
    public class ExportImportEngine
    {
        private const StringComparison IgnoreCaseComp = StringComparison.InvariantCultureIgnoreCase;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExportImportEngine));

        private static readonly string ExportFolder;

        private static readonly Tuple<string, Type>[] DatasetColumns =
        {
            new Tuple<string, Type>("JobId", typeof(int)),
            new Tuple<string, Type>("Name", typeof(string)),
            new Tuple<string, Type>("Value", typeof(string)),
            new Tuple<string, Type>("Level", typeof(int)),
            new Tuple<string, Type>("CreatedOnDate", typeof(DateTime)),
        };

        private readonly Stopwatch stopWatch = Stopwatch.StartNew();
        private readonly List<BasePortableService> portableServices;
        private readonly ExportController exportController;
        private int timeoutSeconds;

        static ExportImportEngine()
        {
            ExportFolder = Globals.ApplicationMapPath + Constants.ExportFolder;
            if (!Directory.Exists(ExportFolder))
            {
                Directory.CreateDirectory(ExportFolder);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="ExportImportEngine"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IEnumerable<BasePortableService>. Scheduled removal in v12.0.0.")]
        public ExportImportEngine()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ExportImportEngine"/> class.</summary>
        /// <param name="portableServices">The portable services.</param>
        /// <param name="exportController">The export controller.</param>
        public ExportImportEngine(IEnumerable<BasePortableService> portableServices, ExportController exportController)
        {
            this.exportController = exportController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ExportController>();
            this.portableServices = (portableServices ?? Globals.GetCurrentServiceProvider().GetServices<BasePortableService>()).ToList();
        }

        private static string[] NotAllowedCategoriesInRequestArray => new[]
        {
            Constants.Category_Content,
            Constants.Category_Pages,
            Constants.Category_Portal,
            Constants.Category_Content,
            Constants.Category_Assets,
            Constants.Category_Users,
            Constants.Category_UsersData,
            Constants.Category_Roles,
            Constants.Category_Vocabularies,
            Constants.Category_Templates,
            Constants.Category_ProfileProps,
            Constants.Category_Packages,
            Constants.Category_Workflows,
        };

        private static string[] CleanUpIgnoredClasses => new[]
        {
            nameof(ExportFile),
            nameof(ExportImport.Dto.Assets.ExportFolder),
            nameof(ExportFolderMapping),
            nameof(ExportFolderPermission),
            nameof(ExportPageTemplate),
            nameof(ExportPortalSetting),
            nameof(ExportPortalLanguage),
            nameof(ExportProfileProperty),
            nameof(ExportUser),
            nameof(ExportAspnetUser),
            nameof(ExportAspnetMembership),
            nameof(ExportUserAuthentication),
            nameof(ExportUserPortal),
            nameof(ExportUserProfile),
            nameof(ExportUserRole),
        };

        private bool TimeIsUp => this.stopWatch.Elapsed.TotalSeconds > this.timeoutSeconds;

        /// <summary>Runs the export job.</summary>
        /// <param name="exportJob">The export job.</param>
        /// <param name="result">The result.</param>
        /// <param name="scheduleHistoryItem">The schedule history item.</param>
        public void Export(ExportImportJob exportJob, ExportImportResult result, ScheduleHistoryItem scheduleHistoryItem)
        {
            var exportDto = JsonConvert.DeserializeObject<ExportDto>(exportJob.JobObject);
            if (exportDto == null)
            {
                exportJob.CompletedOnDate = DateUtils.GetDatabaseUtcTime();
                exportJob.JobStatus = JobStatus.Failed;
                return;
            }

            this.timeoutSeconds = GetTimeoutPerSlot();
            var dbName = Path.Combine(ExportFolder, exportJob.Directory, Constants.ExportDbName);
            var finfo = new FileInfo(dbName);
            dbName = finfo.FullName;

            var checkpoints = EntitiesController.Instance.GetJobChekpoints(exportJob.JobId);

            // Delete so we start a fresh export database; only if there is no previous checkpoint exists
            if (checkpoints.Count == 0)
            {
                if (finfo.Directory != null && finfo.Directory.Exists)
                {
                    finfo.Directory.Delete(true);
                }

                // Clear all the files in finfo.Directory. Create if doesn't exists.
                finfo.Directory?.Create();
                result.AddSummary("Starting Exporting Repository", finfo.Name);
            }
            else
            {
                if (finfo.Directory != null && finfo.Directory.Exists)
                {
                    result.AddSummary("Resuming Exporting Repository", finfo.Name);
                }
                else
                {
                    scheduleHistoryItem.AddLogNote("Resuming data not found.");
                    result.AddSummary("Resuming data not found.", finfo.Name);
                    return;
                }
            }

            exportJob.JobStatus = JobStatus.InProgress;

            // there must be one parent implementor at least for this to work
            var parentServices = this.portableServices.Where(imp => string.IsNullOrEmpty(imp.ParentCategory)).ToList();
            var implementors = this.portableServices.Except(parentServices).ToList();
            var nextLevelServices = new List<BasePortableService>();
            var includedItems = GetAllCategoriesToInclude(exportDto, implementors);

            if (includedItems.Count == 0)
            {
                scheduleHistoryItem.AddLogNote("Export NOT Possible");
                scheduleHistoryItem.AddLogNote("<br/>No items selected for exporting");
                result.AddSummary("Export NOT Possible", "No items selected for exporting");
                exportJob.CompletedOnDate = DateUtils.GetDatabaseUtcTime();
                exportJob.JobStatus = JobStatus.Failed;
                return;
            }

            scheduleHistoryItem.AddLogNote($"<br/><b>SITE EXPORT Preparing Check Points. JOB #{exportJob.JobId}: {exportJob.Name}</b>");
            this.PrepareCheckPoints(exportJob.JobId, parentServices, implementors, includedItems, checkpoints);

            scheduleHistoryItem.AddLogNote($"<br/><b>SITE EXPORT Started. JOB #{exportJob.JobId}: {exportJob.Name}</b>");
            scheduleHistoryItem.AddLogNote($"<br/>Between [{exportDto.FromDateUtc ?? Constants.MinDbTime}] and [{exportDto.ToDateUtc:g}]");
            var firstIteration = true;
            AddJobToCache(exportJob);

            using (var ctx = new ExportImportRepository(dbName))
            {
                ctx.AddSingleItem(exportDto);
                do
                {
                    foreach (var service in parentServices.OrderBy(x => x.Priority))
                    {
                        if (exportJob.IsCancelled)
                        {
                            exportJob.JobStatus = JobStatus.Cancelled;
                            break;
                        }

                        if (implementors.Count > 0)
                        {
                            // collect children for next iteration
                            var children =
                                implementors.Where(imp => service.Category.Equals(imp.ParentCategory, IgnoreCaseComp));
                            nextLevelServices.AddRange(children);
                            implementors = implementors.Except(nextLevelServices).ToList();
                        }

                        if ((firstIteration && includedItems.Any(x => x.Equals(service.Category, IgnoreCaseComp))) ||
                            (!firstIteration && includedItems.Any(x => x.Equals(service.ParentCategory, IgnoreCaseComp))))
                        {
                            var serviceAssembly = service.GetType().Assembly.GetName().Name;
                            service.Result = result;
                            service.Repository = ctx;
                            service.CheckCancelled = CheckCancelledCallBack;
                            service.CheckPointStageCallback = this.CheckpointCallback;
                            service.CheckPoint = checkpoints.FirstOrDefault(cp => cp.Category == service.Category && cp.AssemblyName == serviceAssembly);

                            if (service.CheckPoint == null)
                            {
                                service.CheckPoint = new ExportImportChekpoint
                                {
                                    JobId = exportJob.JobId,
                                    Category = service.Category,
                                    AssemblyName = serviceAssembly,
                                    StartDate = DateUtils.GetDatabaseUtcTime(),
                                };

                                // persist the record in db
                                this.CheckpointCallback(service);
                            }
                            else if (service.CheckPoint.StartDate == Null.NullDate)
                            {
                                service.CheckPoint.StartDate = DateUtils.GetDatabaseUtcTime();
                            }

                            try
                            {
                                service.ExportData(exportJob, exportDto);
                            }
                            finally
                            {
                                this.AddLogsToDatabase(exportJob.JobId, result.CompleteLog);
                            }

                            scheduleHistoryItem.AddLogNote("<br/>Exported: " + service.Category);
                        }
                    }

                    firstIteration = false;
                    parentServices = new List<BasePortableService>(nextLevelServices);
                    nextLevelServices.Clear();
                    if (implementors.Count > 0 && parentServices.Count == 0)
                    {
                        // WARN: this is a case where there is a broken parent-children hierarchy
                        //      and/or there are BasePortableService implementations without a known parent.
                        parentServices = implementors;
                        implementors.Clear();
                        scheduleHistoryItem.AddLogNote(
                            "<br/><b>Orphaned services:</b> " + string.Join(",", parentServices.Select(x => x.Category)));
                    }
                }
                while (parentServices.Count > 0 && !this.TimeIsUp);

                RemoveTokenFromCache(exportJob);
            }

            if (this.TimeIsUp)
            {
                result.AddSummary(
                    $"Job time slot ({this.timeoutSeconds} sec) expired",
                    "Job will resume in the next scheduler iteration");
            }
            else if (exportJob.JobStatus == JobStatus.InProgress)
            {
                // Create Export Summary for manifest file.
                var summary = new ImportExportSummary();
                using (var ctx = new ExportImportRepository(dbName))
                {
                    BaseController.BuildJobSummary(this.portableServices, exportJob.Directory, ctx, summary);
                }

                DoPacking(exportJob, dbName);

                // Complete the job.
                exportJob.JobStatus = JobStatus.Successful;
                SetLastJobStartTime(scheduleHistoryItem.ScheduleID, exportJob.CreatedOnDate);

                var exportFileInfo = new ExportFileInfo
                {
                    ExportPath = exportJob.Directory,
                    ExportSize = Util.FormatSize(GetExportSize(Path.Combine(ExportFolder, exportJob.Directory))),
                };

                summary.ExportFileInfo = exportFileInfo;
                this.exportController.CreatePackageManifest(exportJob, exportFileInfo, summary);
            }
        }

        /// <summary>Runs the import job.</summary>
        /// <param name="importJob">The import job.</param>
        /// <param name="result">The result.</param>
        /// <param name="scheduleHistoryItem">The schedule history item.</param>
        public void Import(ExportImportJob importJob, ExportImportResult result, ScheduleHistoryItem scheduleHistoryItem)
        {
            scheduleHistoryItem.AddLogNote($"<br/><b>SITE IMPORT Started. JOB #{importJob.JobId}</b>");
            this.timeoutSeconds = GetTimeoutPerSlot();
            var importDto = JsonConvert.DeserializeObject<ImportDto>(importJob.JobObject);
            if (importDto == null)
            {
                importJob.CompletedOnDate = DateUtils.GetDatabaseUtcTime();
                importJob.JobStatus = JobStatus.Failed;
                return;
            }

            var dbName = Path.Combine(ExportFolder, importJob.Directory, Constants.ExportDbName);
            var finfo = new FileInfo(dbName);

            if (!finfo.Exists)
            {
                DoUnPacking(importJob);
                finfo = new FileInfo(dbName);
            }

            if (!finfo.Exists)
            {
                scheduleHistoryItem.AddLogNote("<br/>Import file not found. Name: " + dbName);
                importJob.CompletedOnDate = DateUtils.GetDatabaseUtcTime();
                importJob.JobStatus = JobStatus.Failed;
                return;
            }

            using (var ctx = new ExportImportRepository(dbName))
            {
                var exportedDto = ctx.GetSingleItem<ExportDto>();
                var exportVersion = new Version(exportedDto.SchemaVersion);
                var importVersion = new Version(importDto.SchemaVersion);
                if (importVersion < exportVersion)
                {
                    importJob.CompletedOnDate = DateUtils.GetDatabaseUtcTime();
                    importJob.JobStatus = JobStatus.Failed;
                    scheduleHistoryItem.AddLogNote("Import NOT Possible");
                    var msg =
                        $"Exported version ({exportedDto.SchemaVersion}) is newer than import engine version ({importDto.SchemaVersion})";
                    result.AddSummary("Import NOT Possible", msg);
                    return;
                }

                var checkpoints = EntitiesController.Instance.GetJobChekpoints(importJob.JobId);
                if (checkpoints.Count == 0)
                {
                    result.AddSummary("Starting Importing Repository", finfo.Name);
                    result.AddSummary("Importing File Size", Util.FormatSize(finfo.Length));
                    CleanupDatabaseIfDirty(ctx);
                }
                else
                {
                    result.AddSummary("Resuming Importing Repository", finfo.Name);
                }

                var parentServices = this.portableServices.Where(imp => string.IsNullOrEmpty(imp.ParentCategory)).ToList();

                importJob.Name = exportedDto.ExportName;
                importJob.Description = exportedDto.ExportDescription;
                importJob.JobStatus = JobStatus.InProgress;

                // there must be one parent implementor at least for this to work
                var implementors = this.portableServices.Except(parentServices).ToList();
                var nextLevelServices = new List<BasePortableService>();
                var includedItems = GetAllCategoriesToInclude(exportedDto, implementors);

                scheduleHistoryItem.AddLogNote($"<br/><b>SITE IMPORT Preparing Check Points. JOB #{importJob.JobId}: {importJob.Name}</b>");
                this.PrepareCheckPoints(importJob.JobId, parentServices, implementors, includedItems, checkpoints);

                var firstIteration = true;
                AddJobToCache(importJob);

                do
                {
                    foreach (var service in parentServices.OrderBy(x => x.Priority))
                    {
                        if (importJob.IsCancelled)
                        {
                            importJob.JobStatus = JobStatus.Cancelled;
                            break;
                        }

                        if (implementors.Count > 0)
                        {
                            // collect children for next iteration
                            var children =
                                implementors.Where(imp => service.Category.Equals(imp.ParentCategory, IgnoreCaseComp));
                            nextLevelServices.AddRange(children);
                            implementors = implementors.Except(nextLevelServices).ToList();
                        }

                        if ((firstIteration && includedItems.Any(x => x.Equals(service.Category, IgnoreCaseComp))) ||
                            (!firstIteration && includedItems.Any(x => x.Equals(service.ParentCategory, IgnoreCaseComp))))
                        {
                            var serviceAssembly = service.GetType().Assembly.GetName().Name;

                            service.Result = result;
                            service.Repository = ctx;
                            service.CheckCancelled = CheckCancelledCallBack;
                            service.CheckPointStageCallback = this.CheckpointCallback;
                            service.CheckPoint = checkpoints.FirstOrDefault(cp => cp.Category == service.Category && cp.AssemblyName == serviceAssembly)
                                                 ?? new ExportImportChekpoint
                                                 {
                                                     JobId = importJob.JobId,
                                                     AssemblyName = serviceAssembly,
                                                     Category = service.Category,
                                                     Progress = 0,
                                                     StartDate = DateUtils.GetDatabaseUtcTime(),
                                                 };
                            if (service.CheckPoint.StartDate == Null.NullDate)
                            {
                                service.CheckPoint.StartDate = DateUtils.GetDatabaseUtcTime();
                            }

                            this.CheckpointCallback(service);

                            try
                            {
                                service.ImportData(importJob, importDto);
                            }
                            finally
                            {
                                this.AddLogsToDatabase(importJob.JobId, result.CompleteLog);
                            }

                            scheduleHistoryItem.AddLogNote("<br/>Imported: " + service.Category);
                        }
                    }

                    firstIteration = false;
                    parentServices = new List<BasePortableService>(nextLevelServices);
                    nextLevelServices.Clear();
                    if (implementors.Count > 0 && parentServices.Count == 0)
                    {
                        // WARN: this is a case where there is a broken parent-children hierarchy
                        //      and/or there are BasePortableService implementations without a known parent.
                        parentServices = implementors;
                        implementors.Clear();
                        scheduleHistoryItem.AddLogNote(
                            "<br/><b>Orphaned services:</b> " + string.Join(",", parentServices.Select(x => x.Category)));
                    }
                }
                while (parentServices.Count > 0 && !this.TimeIsUp);

                RemoveTokenFromCache(importJob);
                if (this.TimeIsUp)
                {
                    result.AddSummary(
                        $"Job time slot ({this.timeoutSeconds} sec) expired",
                        "Job will resume in the next scheduler iteration");
                }
                else if (importJob.JobStatus == JobStatus.InProgress)
                {
                    importJob.JobStatus = JobStatus.Successful;
                    if (importDto.ExportDto.IncludeContent)
                    {
                        PagesExportService.ResetContentsFlag(ctx);
                    }
                }
            }
        }

        /// <summary>Adds the logs to the database.</summary>
        /// <param name="jobId">The ID of the job to log.</param>
        /// <param name="completeLog">A collection of log items.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void AddLogsToDatabase(int jobId, ICollection<LogItem> completeLog)
        {
            if (completeLog == null || completeLog.Count == 0)
            {
                return;
            }

            using (var table = new DataTable("ExportImportJobLogs"))
            {
                // must create the columns from scratch with each iteration
                table.Columns.AddRange(DatasetColumns.Select(
                    column => new DataColumn(column.Item1, column.Item2)).ToArray());

                // batch specific amount of record each time
                const int batchSize = 500;
                var toSkip = 0;
                while (toSkip < completeLog.Count)
                {
                    foreach (var item in completeLog.Skip(toSkip).Take(batchSize))
                    {
                        var row = table.NewRow();
                        row["JobId"] = jobId;
                        row["Name"] = item.Name.TrimToLength(Constants.LogColumnLength);
                        row["Value"] = item.Value.TrimToLength(Constants.LogColumnLength);
                        row["Level"] = (int)item.ReportLevel;
                        row["CreatedOnDate"] = item.CreatedOnDate;
                        table.Rows.Add(row);
                    }

                    PlatformDataProvider.Instance().BulkInsert("ExportImportJobLogs_AddBulk", "@DataTable", table);
                    toSkip += batchSize;
                    table.Rows.Clear();
                }
            }

            completeLog.Clear();
        }

        private static bool CheckCancelledCallBack(ExportImportJob job)
        {
            var job2 = CachingProvider.Instance().GetItem(Util.GetExpImpJobCacheKey(job)) as ExportImportJob;
            if (job2 == null)
            {
                job2 = EntitiesController.Instance.GetJobById(job.JobId);
                job.IsCancelled = job2.IsCancelled;
                AddJobToCache(job2);
            }

            return job2.IsCancelled;
        }

        private static void AddJobToCache(ExportImportJob job)
        {
            CachingProvider.Instance().Insert(Util.GetExpImpJobCacheKey(job), job);
        }

        private static void RemoveTokenFromCache(ExportImportJob job)
        {
            CachingProvider.Instance().Remove(Util.GetExpImpJobCacheKey(job));
        }

        private static HashSet<string> GetAllCategoriesToInclude(
            ExportDto exportDto,
            List<BasePortableService> implementors)
        {
            // add all child items
            var includedItems = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            if (exportDto.ItemsToExport != null)
            {
                foreach (
                    var name in
                        exportDto.ItemsToExport.Where(
                            x => !NotAllowedCategoriesInRequestArray.Contains(x.ToUpperInvariant())))
                {
                    includedItems.Add(name);
                }
            }

            includedItems.Remove(Constants.Category_Content);

            if (exportDto.Pages?.Length > 0)
            {
                includedItems.Add(Constants.Category_Pages);
                includedItems.Add(Constants.Category_Workflows);
            }

            if (exportDto.IncludeContent)
            {
                includedItems.Add(Constants.Category_Content);
            }

            if (exportDto.IncludeFiles)
            {
                includedItems.Add(Constants.Category_Assets);
            }

            if (exportDto.IncludeUsers)
            {
                includedItems.Add(Constants.Category_Users);
            }

            if (exportDto.IncludeRoles)
            {
                includedItems.Add(Constants.Category_Roles);
            }

            if (exportDto.IncludeVocabularies)
            {
                includedItems.Add(Constants.Category_Vocabularies);
            }

            if (exportDto.IncludeTemplates)
            {
                includedItems.Add(Constants.Category_Templates);
            }

            if (exportDto.IncludeProperfileProperties)
            {
                includedItems.Add(Constants.Category_ProfileProps);
            }

            // This might be added always.
            if (exportDto.IncludeExtensions)
            {
                includedItems.Add(Constants.Category_Packages);
            }

            var additionalItems = new List<string>();
            foreach (var includedItem in includedItems)
            {
                BasePortableService basePortableService;
                if (
                    (basePortableService =
                        implementors.FirstOrDefault(x => x.ParentCategory.Equals(includedItem, IgnoreCaseComp))) != null)
                {
                    additionalItems.Add(basePortableService.Category);
                }
            }

            additionalItems.ForEach(i => includedItems.Add(i));

            // must be included always when there is at least one other object to process
            if (includedItems.Count != 0)
            {
                includedItems.Add(Constants.Category_Portal);
            }

            return includedItems;
        }

        private static int GetTimeoutPerSlot()
        {
            var value = 0;
            var setting = SettingsController.Instance.GetSetting(Constants.MaxSecondsToRunJobKey);
            if (setting != null && !int.TryParse(setting.SettingValue, out value))
            {
                // default max time to run a job is 8 hours
                value = (int)TimeSpan.FromHours(8).TotalSeconds;
            }

            // enforce minimum/maximum of 10 minutes/12 hours per slot
            if (value < 600)
            {
                value = 600;
            }
            else if (value > 12 * 60 * 60)
            {
                value = 12 * 60 * 60;
            }

            return value;
        }

        private static void SetLastJobStartTime(int scheduleId, DateTimeOffset time)
        {
            SchedulingProvider.Instance()
                .AddScheduleItemSetting(
                    scheduleId,
                    Constants.LastJobStartTimeKey,
                    time.ToUniversalTime()
                        .DateTime.ToString(Constants.JobRunDateTimeFormat));
        }

        private static void DoPacking(ExportImportJob exportJob, string dbName)
        {
            var exportFileArchive = Path.Combine(ExportFolder, exportJob.Directory, Constants.ExportZipDbName);
            var folderOffset = exportFileArchive.IndexOf(Constants.ExportZipDbName, StringComparison.Ordinal);
            File.Delete(CompressionUtil.AddFileToArchive(dbName, exportFileArchive, folderOffset)
                ? dbName
                : exportFileArchive);
        }

        private static void DoUnPacking(ExportImportJob importJob)
        {
            var extractFolder = Path.Combine(ExportFolder, importJob.Directory);
            var dbName = Path.Combine(extractFolder, Constants.ExportDbName);
            if (File.Exists(dbName))
            {
                return;
            }

            var zipDbName = Path.Combine(extractFolder, Constants.ExportZipDbName);
            CompressionUtil.UnZipFileFromArchive(Constants.ExportDbName, zipDbName, extractFolder, false);
        }

        private static long GetExportSize(string exportFolder)
        {
            var files = Directory.GetFiles(exportFolder);
            return files.Sum(file => new FileInfo(file).Length);
        }

        private static void CleanupDatabaseIfDirty(ExportImportRepository repository)
        {
            var exportDto = repository.GetSingleItem<ExportDto>();
            var isDirty = exportDto.IsDirty;
            exportDto.IsDirty = true;
            repository.UpdateSingleItem(exportDto);
            if (!isDirty)
            {
                return;
            }

            var typeLocator = new TypeLocator();
            var types = typeLocator.GetAllMatchingTypes(
                t => t != null && t.IsClass && !t.IsAbstract && t.IsVisible &&
                     typeof(BasicExportImportDto).IsAssignableFrom(t));

            foreach (var type in from type in types
                                 let typeName = type.Name
                                 where !CleanUpIgnoredClasses.Contains(typeName)
                                 select type)
            {
                try
                {
                    repository.CleanUpLocal(type.Name);
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(
                        "Unable to clear {0} while calling CleanupDatabaseIfDirty. Error: {1}",
                        type.Name,
                        e.Message);
                }
            }
        }

        private void PrepareCheckPoints(
            int jobId,
            List<BasePortableService> parentServices,
            List<BasePortableService> implementors,
            HashSet<string> includedItems,
            IList<ExportImportChekpoint> checkpoints)
        {
            // there must be one parent implementor at least for this to work
            var nextLevelServices = new List<BasePortableService>();
            var firstIteration = true;
            if (checkpoints.Any())
            {
                return;
            }

            do
            {
                foreach (var service in parentServices.OrderBy(x => x.Priority))
                {
                    if (implementors.Count > 0)
                    {
                        // collect children for next iteration
                        var children =
                            implementors.Where(imp => service.Category.Equals(imp.ParentCategory, IgnoreCaseComp));
                        nextLevelServices.AddRange(children);
                        implementors = implementors.Except(nextLevelServices).ToList();
                    }

                    if ((firstIteration && includedItems.Any(x => x.Equals(service.Category, IgnoreCaseComp))) ||
                        (!firstIteration && includedItems.Any(x => x.Equals(service.ParentCategory, IgnoreCaseComp))))
                    {
                        var serviceAssembly = service.GetType().Assembly.GetName().Name;

                        service.CheckPoint = checkpoints.FirstOrDefault(cp => cp.Category == service.Category && cp.AssemblyName == serviceAssembly);

                        if (service.CheckPoint != null)
                        {
                            continue;
                        }

                        service.CheckPoint = new ExportImportChekpoint
                        {
                            JobId = jobId,
                            AssemblyName = serviceAssembly,
                            Category = service.Category,
                            Progress = 0,
                        };

                        // persist the record in db
                        this.CheckpointCallback(service);
                    }
                }

                firstIteration = false;
                parentServices = new List<BasePortableService>(nextLevelServices);
                nextLevelServices.Clear();
            }
            while (parentServices.Count > 0);
        }

        /// <summary>Callback function to provide a checkpoint mechanism for an <see cref="BasePortableService"/> implementation.</summary>
        /// <param name="service">The <see cref="BasePortableService"/> implementation.</param>
        /// <returns>Treu to stop further <see cref="BasePortableService"/> processing; false otherwise.</returns>
        private bool CheckpointCallback(BasePortableService service)
        {
            EntitiesController.Instance.UpdateJobChekpoint(service.CheckPoint);
            return this.TimeIsUp;
        }
    }
}
