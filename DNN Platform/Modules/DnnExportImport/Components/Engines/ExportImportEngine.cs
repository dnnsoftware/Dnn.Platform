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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Controllers;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Models;
using Dnn.ExportImport.Components.Repository;
using DotNetNuke.Common;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Scheduling;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Engines
{
    public class ExportImportEngine
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExportImportEngine));

        private const StringComparison IgnoreCaseComp = StringComparison.InvariantCultureIgnoreCase;

        private static readonly string DbFolder;

        static ExportImportEngine()
        {
            DbFolder = Globals.ApplicationMapPath + Constants.ExportFolder;
            if (!Directory.Exists(DbFolder))
            {
                Directory.CreateDirectory(DbFolder);
            }
        }

        private readonly Stopwatch _stopWatch = Stopwatch.StartNew();
        private int _timeoutSeconds;

        public int ProgressPercentage { get; private set; } = 1;

        public ExportImportResult Export(ExportImportJob exportJob, ScheduleHistoryItem scheduleHistoryItem)
        {
            var result = new ExportImportResult
            {
                JobId = exportJob.JobId,
            };

            var exportDto = JsonConvert.DeserializeObject<ExportDto>(exportJob.JobObject);
            if (exportDto == null)
            {
                exportJob.CompletedOnDate = DateTime.UtcNow;
                exportJob.JobStatus = JobStatus.DoneFailure;
                return result;
            }

            _timeoutSeconds = GetTimeoutPerSlot(scheduleHistoryItem.ScheduleID);

            var dbName = Path.Combine(DbFolder, exportJob.ExportFile + Constants.ExportDbExt);
            var finfo = new FileInfo(dbName);

            var checkpoints = EntitiesController.Instance.GetJobChekpoints(exportJob.JobId);

            //Delete so we start a fresh export database; only if there is no previous checkpoint exists
            if (checkpoints.Count == 0)
            {
                if (finfo.Exists) finfo.Delete();
                result.AddSummary("Starting Exporting Repository", finfo.Name);
            }
            else
            {
                result.AddSummary("Resuming Exporting Repository", finfo.Name);
            }

            exportJob.JobStatus = JobStatus.InProgress;

            // there must be one parent implementor at least for this to work
            var implementors = GetPortableImplementors().ToList();
            var parentServices = implementors.Where(imp => string.IsNullOrEmpty(imp.ParentCategory)).ToList();
            implementors = implementors.Except(parentServices).ToList();
            var nextLevelServices = new List<IPortable2>();
            var includedItems = GetAllCategoriesToInclude(exportDto, implementors);

            if (includedItems.Count == 0)
            {
                scheduleHistoryItem.AddLogNote("Export NOT Possible");
                scheduleHistoryItem.AddLogNote("<br/>No items selected for exporting");
                result.AddSummary("Export NOT Possible", "No items selected for exporting");
                exportJob.CompletedOnDate = DateTime.UtcNow;
                exportJob.JobStatus = JobStatus.DoneFailure;
                return result;
            }

            scheduleHistoryItem.AddLogNote($"<br/><b>SITE EXPORT Started. JOB #{exportJob.JobId}</b>");
            scheduleHistoryItem.AddLogNote($"<br/>Between [{exportDto.SinceTime}] and [{exportJob.CreatedOnDate:g}]");
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
                            var children = implementors.Where(imp => service.Category.Equals(imp.ParentCategory, IgnoreCaseComp));
                            nextLevelServices.AddRange(children);
                            implementors = implementors.Except(nextLevelServices).ToList();
                        }

                        if ((firstIteration && includedItems.Any(x => x.Equals(service.Category, IgnoreCaseComp))) ||
                            (!firstIteration && includedItems.Any(x => x.Equals(service.ParentCategory, IgnoreCaseComp))))
                        {
                            service.Result = result;
                            service.Repository = ctx;
                            service.CheckCancelled = CheckCancelledCallBack;
                            service.CheckPointStageCallback = CheckpointCallback;
                            service.CheckPoint = checkpoints.FirstOrDefault(cp => cp.Category == service.Category);

                            if (service.CheckPoint == null)
                            {
                                service.CheckPoint = new ExportImportChekpoint
                                {
                                    JobId = exportJob.JobId,
                                    Category = service.Category
                                };

                                // persist the record in db
                                CheckpointCallback(service);
                            }

                            service.ExportData(exportJob, exportDto);
                            scheduleHistoryItem.AddLogNote("<br/>Exported: " + service.Category);
                        }
                    }

                    firstIteration = false;
                    parentServices = new List<IPortable2>(nextLevelServices);
                    nextLevelServices.Clear();
                    if (implementors.Count > 0 && parentServices.Count == 0)
                    {
                        //WARN: this is a case where there is a broken parent-children hierarchy
                        //      and/or there are IPortable2 implementations without a known parent.
                        parentServices = implementors;
                        implementors.Clear();
                        scheduleHistoryItem.AddLogNote(
                            "<br/><b>Orphaned services:</b> " + string.Join(",", parentServices.Select(x => x.Category)));
                    }
                } while (parentServices.Count > 0 && !TimeIsUp);

                //TODO: zip files when any

                RemoveTokenFromCache(exportJob);
            }

            if (TimeIsUp)
            {
                result.AddSummary($"Job time slot ({_timeoutSeconds} sec) expired", "Job will resume in the next scheduler iteration");
            }
            else
            {
                if (exportJob.JobStatus == JobStatus.InProgress)
                {
                    exportJob.JobStatus = JobStatus.DoneSuccess;
                    SetLastJobStartTime(scheduleHistoryItem.ScheduleID, exportJob.CreatedOnDate);
                }

                finfo = new FileInfo(finfo.FullName); // refresh to get new size
                result.AddSummary("Exported File Size", Util.FormatSize(finfo.Length));
            }

            return result;
        }

        public ExportImportResult Import(ExportImportJob importJob, ScheduleHistoryItem scheduleHistoryItem)
        {
            scheduleHistoryItem.AddLogNote($"<br/><b>SITE IMPORT Started. JOB #{importJob.JobId}</b>");
            _timeoutSeconds = GetTimeoutPerSlot(scheduleHistoryItem.ScheduleID);
            var result = new ExportImportResult
            {
                JobId = importJob.JobId,
            };

            var importDto = JsonConvert.DeserializeObject<ImportDto>(importJob.JobObject);
            if (importDto == null)
            {
                importJob.CompletedOnDate = DateTime.UtcNow;
                importJob.JobStatus = JobStatus.DoneFailure;
                return result;
            }

            var dbName = Path.Combine(DbFolder, importDto.FileName);
            var finfo = new FileInfo(dbName);
            if (!finfo.Exists)
            {
                scheduleHistoryItem.AddLogNote("<br/>Import file not found. Name: " + dbName);
                importJob.CompletedOnDate = DateTime.UtcNow;
                importJob.JobStatus = JobStatus.DoneFailure;
                return result;
            }

            //TODO: unzip files first

            using (var ctx = new ExportImportRepository(dbName))
            {
                var exportedDto = ctx.GetSingleItem<ExportDto>();
                var exportVersion = new Version(exportedDto.SchemaVersion);
                var importVersion = new Version(importDto.SchemaVersion);
                if (importVersion < exportVersion)
                {
                    importJob.CompletedOnDate = DateTime.UtcNow;
                    importJob.JobStatus = JobStatus.DoneFailure;
                    scheduleHistoryItem.AddLogNote("Import NOT Possible");
                    var msg = $"Exported version ({exportedDto.SchemaVersion}) is newer than import engine version ({importDto.SchemaVersion})";
                    result.AddSummary("Import NOT Possible", msg);
                    return result;
                }

                var checkpoints = EntitiesController.Instance.GetJobChekpoints(importJob.JobId);
                if (checkpoints.Count == 0)
                {
                    result.AddSummary("Starting Importing Repository", finfo.Name);
                    result.AddSummary("Importing File Size", Util.FormatSize(finfo.Length));
                }
                else
                {
                    result.AddSummary("Resuming Importing Repository", finfo.Name);
                }

                var implementors = GetPortableImplementors().ToList();
                var parentServices = implementors.Where(imp => string.IsNullOrEmpty(imp.ParentCategory)).ToList();
                importJob.JobStatus = JobStatus.InProgress;

                // there must be one parent implementor at least for this to work
                implementors = implementors.Except(parentServices).ToList();
                var nextLevelServices = new List<IPortable2>();
                var includedItems = GetAllCategoriesToInclude(exportedDto, implementors);
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
                            var children = implementors.Where(imp => service.Category.Equals(imp.ParentCategory, IgnoreCaseComp));
                            nextLevelServices.AddRange(children);
                            implementors = implementors.Except(nextLevelServices).ToList();
                        }

                        if ((firstIteration && includedItems.Any(x => x.Equals(service.Category, IgnoreCaseComp))) ||
                            (!firstIteration && includedItems.Any(x => x.Equals(service.ParentCategory, IgnoreCaseComp))))
                        {
                            service.Result = result;
                            service.Repository = ctx;
                            service.CheckCancelled = CheckCancelledCallBack;
                            service.CheckPointStageCallback = CheckpointCallback;
                            service.CheckPoint = checkpoints.FirstOrDefault(cp => cp.Category == service.Category)
                                     ?? new ExportImportChekpoint
                                     {
                                         JobId = importJob.JobId,
                                         Category = service.Category
                                     };
                            CheckpointCallback(service);

                            service.ImportData(importJob, exportedDto);
                            scheduleHistoryItem.AddLogNote("<br/>Imported: " + service.Category);
                        }
                    }

                    firstIteration = false;
                    parentServices = new List<IPortable2>(nextLevelServices);
                    nextLevelServices.Clear();
                    if (implementors.Count > 0 && parentServices.Count == 0)
                    {
                        //WARN: this is a case where there is a broken parent-children hierarchy
                        //      and/or there are IPortable2 implementations without a known parent.
                        parentServices = implementors;
                        implementors.Clear();
                        scheduleHistoryItem.AddLogNote(
                            "<br/><b>Orphaned services:</b> " + string.Join(",", parentServices.Select(x => x.Category)));
                    }
                } while (parentServices.Count > 0 && !TimeIsUp);

                RemoveTokenFromCache(importJob);
                if (TimeIsUp)
                {
                    result.AddSummary($"Job time slot ({_timeoutSeconds} sec) expired", "Job will resume in the next scheduler iteration");
                }
                else if (importJob.JobStatus == JobStatus.InProgress && !TimeIsUp)
                    importJob.JobStatus = JobStatus.DoneSuccess;
            }

            return result;
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

        /// <summary>
        /// Callback function to provide a checkpoint mechanism for an <see cref="IPortable2"/> implementation.
        /// </summary>
        /// <param name="service">The <see cref="IPortable2"/> implementation</param>
        /// <returns>Treu to stop further <see cref="IPortable2"/> processing; false otherwise</returns>
        private bool CheckpointCallback(IPortable2 service)
        {
            EntitiesController.Instance.UpdateJobChekpoint(service.CheckPoint);
            return TimeIsUp;
        }

        private bool TimeIsUp => _stopWatch.Elapsed.TotalSeconds > _timeoutSeconds;

        private static void AddJobToCache(ExportImportJob job)
        {
            CachingProvider.Instance().Insert(Util.GetExpImpJobCacheKey(job), job);
        }

        private static void RemoveTokenFromCache(ExportImportJob job)
        {
            CachingProvider.Instance().Remove(Util.GetExpImpJobCacheKey(job));
        }

        private static IEnumerable<IPortable2> GetPortableImplementors()
        {
            var typeLocator = new TypeLocator();
            var types = typeLocator.GetAllMatchingTypes(
                t => t != null && t.IsClass && !t.IsAbstract && t.IsVisible &&
                     typeof(IPortable2).IsAssignableFrom(t));

            foreach (var type in types)
            {
                IPortable2 portable2Type;
                try
                {
                    portable2Type = Activator.CreateInstance(type) as IPortable2;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("Unable to create {0} while calling IPortable2 implementors. {1}",
                                       type.FullName, e.Message);
                    portable2Type = null;
                }

                if (portable2Type != null)
                {
                    yield return portable2Type;
                }
            }
        }

        private static HashSet<string> GetAllCategoriesToInclude(ExportDto exportDto, List<IPortable2> implementors)
        {
            // add all child items
            var includedItems = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var name in exportDto.ItemsToExport)
            {
                includedItems.Add(name);
                foreach (var impl in implementors)
                {
                    if (name.Equals(impl.ParentCategory, IgnoreCaseComp))
                        includedItems.Add(impl.Category);
                }
            }

            if (exportDto.Pages.Length > 0)
                includedItems.Add(Constants.Category_Pages);

            // must be included always when there is at least one other object to process
            if (includedItems.Count > 0)
                includedItems.Add(Constants.Category_Portal);

            return includedItems;
        }

        private static int GetTimeoutPerSlot(int scheduleId)
        {
            var provider = SchedulingProvider.Instance();
            var nseedsUpdate = false;
            int value;
            var settings = provider.GetScheduleItemSettings(scheduleId);
            if (!int.TryParse(settings[Constants.MaxTimeToRunJobKey] as string ?? "", out value))
            {
                // max time to run a job is 2 hours
                value = (int)TimeSpan.FromHours(2).TotalSeconds;
                nseedsUpdate = true;
            }

            // enforce minimum of 60 seconds per slot
            if (value < 60)
            {
                value = 60;
                nseedsUpdate = true;
            }

            if (nseedsUpdate)
            {
                provider.AddScheduleItemSetting(scheduleId, Constants.MaxTimeToRunJobKey, value.ToString());
            }

            return value;
        }

        private static void SetLastJobStartTime(int scheduleId, DateTimeOffset time)
        {
            SchedulingProvider.Instance().AddScheduleItemSetting(
                scheduleId, Constants.LastJobStartTimeKey,
                time.ToUniversalTime().DateTime.ToString(Constants.JobRunDateTimeFormat));
        }
    }
}