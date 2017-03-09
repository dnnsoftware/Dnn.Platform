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
using System.Threading;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Models;
using Dnn.ExportImport.Components.Repository;
using DotNetNuke.Common;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Scheduling;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Engines
{
    public class ExportImportEngine
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExportImportEngine));

        const StringComparison IgnoreCaseComp = StringComparison.InvariantCultureIgnoreCase;

        private static readonly string _dbFolder;

        static ExportImportEngine()
        {
            _dbFolder = Globals.ApplicationMapPath + Constants.ExportFolder;
            if (!Directory.Exists(_dbFolder))
            {
                Directory.CreateDirectory(_dbFolder);
            }
        }

        public int ProgressPercentage { get; private set; } = 1;

        public ExportImportResult Export(ExportImportJob exportJob, ScheduleHistoryItem scheduleHistoryItem)
        {
            var result = new ExportImportResult
            {
                JobId = exportJob.JobId,
                Status = JobStatus.DoneFailure,
            };

            var exportDto = JsonConvert.DeserializeObject<ExportDto>(exportJob.JobObject);
            if (exportDto == null)
            {
                exportJob.CompletedOn = DateTime.UtcNow;
                exportJob.JobStatus = JobStatus.DoneFailure;
                return result;
            }

            if (!exportDto.ItemsToExport.Any())
            {
                exportJob.CompletedOn = DateTime.UtcNow;
                exportJob.JobStatus = JobStatus.DoneFailure;
                scheduleHistoryItem.AddLogNote("<br/>No items selected for exporting");
                return result;
            }

            var dbName = Path.Combine(_dbFolder, exportJob.ExportFile + Constants.ExportDbExt);
            var finfo = new FileInfo(dbName);

            //Delete so we start a fresh database
            if (finfo.Exists) finfo.Delete();

            using (var ctx = new ExportImportRepository(dbName))
            {
                result.AddSummary("Repository", finfo.Name);
                ctx.AddSingleItem(exportDto);
                var implementors = GetPortableImplementors().ToList();
                if (implementors.Any())
                {
                    // there must be one parent implementor at least for this to work
                    var parents = implementors.Where(imp => string.IsNullOrEmpty(imp.ParentCategory)).ToList();
                    implementors = implementors.Except(parents).ToList();
                    var nextLevelImplementors = new List<IPortable2>();

                    do
                    {
                        foreach (var service in parents.OrderBy(x => x.Priority))
                        {
                            if (implementors.Count > 0)
                            {
                                // collect children for next iteration
                                var children = implementors.Where(imp => service.Category.Equals(imp.ParentCategory, IgnoreCaseComp));
                                nextLevelImplementors.AddRange(children);
                                implementors = implementors.Except(nextLevelImplementors).ToList();
                            }

                            if (exportDto.ItemsToExport.Any(x => x.Equals(service.Category, IgnoreCaseComp)))
                            {
                                service.ExportData(exportJob, ctx, result, exportDto.ExportTime.UtcDateTime);
                                scheduleHistoryItem.AddLogNote("<br/>Exported: " + service.Category);
                                result.Status = JobStatus.InProgress;
                                result.ProcessedCount += 1;
                            }
                        }

                        parents = new List<IPortable2>(nextLevelImplementors);
                        nextLevelImplementors.Clear();
                        if (implementors.Count > 0 && parents.Count == 0)
                        {
                            //WARN: this is a case where there is a broken parnets-children hierarchy
                            //      and/or there are IPortable2 implementations without a known parent.
                            parents = implementors;
                            implementors.Clear();
                            scheduleHistoryItem.AddLogNote(
                                "<br/><b>Orphaned services:</b> " + string.Join(",", parents.Select(x => x.Category)));
                        }
                    } while (parents.Count > 0);
                }

                foreach (var page in exportDto.Pages)
                {
                    //TODO: export pages
                    if (page != null)
                    {
                    }
                    result.Status = JobStatus.InProgress;
                }

                //result.Status = JobStatus.DoneSuccess; // TODO:
            }

            // wait for the file to be flushed as finfo.Length will through exception
            while (!finfo.Exists) { Thread.Sleep(1); }
            result.AddSummary("Total Export File Size", Util.FormatSize(finfo.Length));
            exportJob.JobStatus = result.Status;

            return result;
        }

        public ExportImportResult Import(ExportImportJob importJob, ScheduleHistoryItem scheduleHistoryItem)
        {
            var result = new ExportImportResult
            {
                JobId = importJob.JobId,
                Status = JobStatus.DoneFailure,
            };

            var importDto = JsonConvert.DeserializeObject<ImportDto>(importJob.JobObject);
            if (importDto == null)
            {
                importJob.CompletedOn = DateTime.UtcNow;
                importJob.JobStatus = JobStatus.DoneFailure;
                return result;
            }

            var dbName = Path.Combine(_dbFolder, importDto.FileName);
            var finfo = new FileInfo(dbName);
            if (!finfo.Exists)
            {
                scheduleHistoryItem.AddLogNote("<br/>Import file not found. Name: " + dbName);
                importJob.CompletedOn = DateTime.UtcNow;
                importJob.JobStatus = JobStatus.DoneFailure;
                return result;
            }

            result.AddSummary("Repository", finfo.Name);
            result.AddSummary("Total Import File Size", Util.FormatSize(finfo.Length));
            using (var ctx = new ExportImportRepository(dbName))
            {
                var exporedtDto = ctx.GetSingleItem<ExportDto>();

                var implementors = GetPortableImplementors().ToList();
                if (implementors.Any())
                {
                    // there must be one parent implementor at least for this to work
                    var parents = implementors.Where(imp => string.IsNullOrEmpty(imp.ParentCategory)).ToList();
                    implementors = implementors.Except(parents).ToList();
                    var nextLevelImplementors = new List<IPortable2>();

                    do
                    {
                        foreach (var service in parents.OrderBy(x => x.Priority))
                        {
                            if (implementors.Count > 0)
                            {
                                // collect children for next iteration
                                var children = implementors.Where(imp => service.Category.Equals(imp.ParentCategory, IgnoreCaseComp));
                                nextLevelImplementors.AddRange(children);
                                implementors = implementors.Except(nextLevelImplementors).ToList();
                            }

                            if (exporedtDto.ItemsToExport.Any(x => x.Equals(service.Category, IgnoreCaseComp)))
                            {
                                service.ImportData(importJob, exporedtDto, ctx, result);
                                scheduleHistoryItem.AddLogNote("<br/>Imported: " + service.Category);
                                result.Status = JobStatus.InProgress;
                                result.ProcessedCount += 1;
                            }
                        }

                        parents = new List<IPortable2>(nextLevelImplementors);
                        nextLevelImplementors.Clear();
                        if (implementors.Count > 0 && parents.Count == 0)
                        {
                            //WARN: this is a case where there is a broken parnets-children hierarchy
                            //      and/or there are IPortable2 implementations without a known parent.
                            parents = implementors;
                            implementors.Clear();
                            scheduleHistoryItem.AddLogNote(
                                "<br/><b>Orphaned services:</b> " + string.Join(",", parents.Select(x => x.Category)));
                        }
                    } while (parents.Count > 0);
                }

                foreach (var page in exporedtDto.Pages)
                {
                    //TODO: import pages
                    if (page != null)
                    {
                    }
                    result.Status = JobStatus.InProgress;
                }

                //result.Status = JobStatus.DoneSuccess; // TODO:
            }

            importJob.JobStatus = result.Status;
            return result;
        }

        private static IEnumerable<IPortable2> GetPortableImplementors()
        {
            var types = GetAllAppStartEventTypes();

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

        private static IEnumerable<Type> GetAllAppStartEventTypes()
        {
            var typeLocator = new TypeLocator();
            return typeLocator.GetAllMatchingTypes(
                t => t != null && t.IsClass && !t.IsAbstract && t.IsVisible &&
                     typeof(IPortable2).IsAssignableFrom(t));
        }
    }
}