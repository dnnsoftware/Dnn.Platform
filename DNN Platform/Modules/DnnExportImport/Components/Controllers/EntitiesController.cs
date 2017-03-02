using System;
using System.Collections.Generic;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Providers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Services.Log.EventLog;

namespace Dnn.ExportImport.Components.Controllers
{
    public class EntitiesController : ServiceLocator<IEntitiesController, EntitiesController>, IEntitiesController
    {
        protected override Func<IEntitiesController> GetFactory()
        {
            return () => new EntitiesController();
        }

        public EntitiesController()
        {
        }

        public ExportImportJob GetFirstActiveJob()
        {
            return CBO.Instance.FillObject<ExportImportJob>(DataProvider.Instance().GetFirstActiveJob());
        }

        public IEnumerable<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex)
        {
            return CBO.Instance.FillCollection<ExportImportJob>(
                DataProvider.Instance().GetAllJobs(portalId, pageSize, pageIndex));
        }

        public void UpdateJobStatus(ExportImportJob job)
        {
            DataProvider.Instance().UpdateJobStatus(job.JobId, job.JobStatus);
        }
    }
}