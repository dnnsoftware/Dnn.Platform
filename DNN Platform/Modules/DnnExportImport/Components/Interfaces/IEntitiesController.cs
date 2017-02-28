using System.Collections.Generic;
using System.Data;
using Dnn.ExportImport.Components.Entities;

namespace Dnn.ExportImport.Components.Interfaces
{
    public interface IEntitiesController
    {
        ExportImportJob GetFirstActiveJob();
        IEnumerable<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex);
        void UpdateJobStatus(ExportImportJob job);
    }
}
