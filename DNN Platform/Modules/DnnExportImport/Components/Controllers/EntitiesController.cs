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
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Providers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using Dnn.ExportImport.Components.Dto.Pages;

namespace Dnn.ExportImport.Components.Controllers
{
    public class EntitiesController : ServiceLocator<IEntitiesController, EntitiesController>, IEntitiesController
    {
        protected override Func<IEntitiesController> GetFactory()
        {
            return () => new EntitiesController();
        }

        public ExportImportJob GetFirstActiveJob()
        {
            return CBO.Instance.FillObject<ExportImportJob>(DataProvider.Instance().GetFirstActiveJob());
        }

        public ExportImportJob GetJobById(int jobId)
        {
            var job = CBO.Instance.FillObject<ExportImportJob>(DataProvider.Instance().GetJobById(jobId));
            System.Diagnostics.Trace.WriteLine($"xxxxxxxxx job id={job.JobId} IsCancelled={job.IsCancelled} xxxxxxxxx");
            return job;
        }

        public IList<ExportImportJobLog> GetJobSummaryLog(int jobId)
        {
            return CBO.Instance.FillCollection<ExportImportJobLog>(DataProvider.Instance().GetJobSummaryLog(jobId));
        }

        public IList<ExportImportJobLog> GetJobFullLog(int jobId)
        {
            return CBO.Instance.FillCollection<ExportImportJobLog>(DataProvider.Instance().GetJobFullLog(jobId));
        }

        public int GetAllJobsCount(int? portalId, int? jobType, string keywords)
        {
            return DataProvider.Instance().GetAllJobsCount(portalId, jobType, keywords);
        }

        public IList<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
        {
            return CBO.Instance.FillCollection<ExportImportJob>(
                DataProvider.Instance().GetAllJobs(portalId, pageSize, pageIndex, jobType, keywords));
        }

        public DateTime GetLastExportTime(int portalId)
        {
            return DataProvider.Instance().GetLastExportTime(portalId) ?? Constants.MinDbTime;
        }

        public void UpdateJobInfo(ExportImportJob job)
        {
            DataProvider.Instance().UpdateJobInfo(job.JobId, job.Name, job.Description);
        }

        public void UpdateJobStatus(ExportImportJob job)
        {
            DataProvider.Instance().UpdateJobStatus(job.JobId, job.JobStatus);
        }

        public void SetJobCancelled(ExportImportJob job)
        {
            DataProvider.Instance().SetJobCancelled(job.JobId);
        }

        public void RemoveJob(ExportImportJob job)
        {
            DataProvider.Instance().RemoveJob(job.JobId);
        }

        public IList<ExportImportChekpoint> GetJobChekpoints(int jobId)
        {
            return CBO.Instance.FillCollection<ExportImportChekpoint>(DataProvider.Instance().GetJobChekpoints(jobId));
        }

        public void UpdateJobChekpoint(ExportImportChekpoint checkpoint)
        {
            DataProvider.Instance().UpsertJobChekpoint(checkpoint);
        }

        public IList<ExportTabInfo> GetPortalTabs(int portalId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabInfo>(
                DataProvider.Instance().GetAllPortalTabs(portalId, includeDeleted, toDate, fromDate));
        }

        public IList<ExportTabSetting> GetTabSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabSetting>(
                DataProvider.Instance().GetAllTabSettings(tabId, toDate, fromDate));
        }

        public IList<ExportTabPermission> GetTabPermissions(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabPermission>(
                DataProvider.Instance().GetAllTabPermissions(tabId, toDate, fromDate));
        }

        public IList<ExportTabUrl> GetTabUrls(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabUrl>(
                DataProvider.Instance().GetAllTabUrls(tabId, toDate, fromDate));
        }

        public IList<ExportTabAliasSkin> GetTabAliasSkins(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabAliasSkin>(
                DataProvider.Instance().GetAllTabAliasSkins(tabId, toDate, fromDate));
        }

        public IList<ExportModule> GetModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportModule>(
                DataProvider.Instance().GetAllModules(tabId, includeDeleted, toDate, fromDate));
        }

        public IList<ExportModuleSetting> GetModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportModuleSetting>(
                DataProvider.Instance().GetAllModuleSettings(moduleId, toDate, fromDate));
        }

        public IList<ExportModulePermission> GetModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportModulePermission>(
                DataProvider.Instance().GetAllModulePermissions(moduleId, toDate, fromDate));
        }

        public IList<ExportTabModule> GetTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabModule>(
                DataProvider.Instance().GetAllTabModules(tabId, includeDeleted, toDate, fromDate));
        }

        public IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabModuleSetting>(
                DataProvider.Instance().GetAllTabModuleSettings(tabId, toDate, fromDate));
        }
    }
}