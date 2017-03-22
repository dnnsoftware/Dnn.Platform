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
        void UpdateJobInfo(ExportImportJob job);
        void UpdateJobStatus(ExportImportJob job);
        void SetJobCancelled(ExportImportJob job);
        void RemoveJob(ExportImportJob job);
        IList<ExportImportChekpoint> GetJobChekpoints(int jobId);
        void UpdateJobChekpoint(ExportImportChekpoint checkpoint);
        IList<ExportTabInfo> GetPortalTabs(int portalId, DateTime tillDate, DateTime? sinceDate);
        IList<ExportTabSetting> GetTabSettings(int tabId, DateTime tillDate, DateTime? sinceDate);
        IList<ExportTabPermission> GetTabPermissions(int tabId, DateTime tillDate, DateTime? sinceDate);
        IList<ExportTabModule> GetTabModules(int tabId, DateTime tillDate, DateTime? sinceDate);
        IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, DateTime tillDate, DateTime? sinceDate);
    }
}
