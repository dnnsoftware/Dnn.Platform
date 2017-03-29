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
using Dnn.ExportImport.Components.Dto.Pages;
using Dnn.ExportImport.Components.Entities;

namespace Dnn.ExportImport.Components.Interfaces
{
    public interface IEntitiesController
    {
        ExportImportJob GetFirstActiveJob();
        ExportImportJob GetJobById(int jobId);
        IList<ExportImportJobLog> GetJobSummaryLog(int jobId);
        IList<ExportImportJobLog> GetJobFullLog(int jobId);
        int GetAllJobsCount(int? portalId, int? jobType, string keywords);
        IList<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords);
        DateTime GetLastExportTime(int portalId);
        void UpdateJobInfo(ExportImportJob job);
        void UpdateJobStatus(ExportImportJob job);
        void SetJobCancelled(ExportImportJob job);
        void RemoveJob(ExportImportJob job);
        IList<ExportImportChekpoint> GetJobChekpoints(int jobId);
        void UpdateJobChekpoint(ExportImportChekpoint checkpoint);

        IList<ExportTabInfo> GetPortalTabs(int portalId, bool includeDeleted, DateTime toDate, DateTime? fromDate);
        IList<ExportTabSetting> GetTabSettings(int tabId, DateTime toDate, DateTime? fromDate);
        IList<ExportTabPermission> GetTabPermissions(int tabId, DateTime toDate, DateTime? fromDate);
        IList<ExportTabUrl> GetTabUrls(int tabId, DateTime toDate, DateTime? fromDate);
        IList<ExportTabAliasSkin> GetTabAliasSkins(int tabId, DateTime toDate, DateTime? fromDate);

        IList<ExportModule> GetModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate);
        IList<ExportModuleSetting> GetModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate);
        IList<ExportModulePermission> GetModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate);

        IList<ExportTabModule> GetTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate);
        IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, DateTime toDate, DateTime? fromDate);
    }
}
