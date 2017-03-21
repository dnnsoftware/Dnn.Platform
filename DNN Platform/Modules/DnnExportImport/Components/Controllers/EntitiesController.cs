using System;
using System.Collections.Generic;
using Dnn.ExportImport.Components.Dto.Jobs;
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

        public IList<ShortTabInfo> GetPortalTabs(int portalId)
        {
            return CBO.Instance.FillCollection<ShortTabInfo>(DataProvider.Instance().GetAllPortalTabs(portalId));
        }
    }
}