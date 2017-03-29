using System;
using System.Collections.Generic;
using Dnn.ExportImport.Components.Dto.Jobs;
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
