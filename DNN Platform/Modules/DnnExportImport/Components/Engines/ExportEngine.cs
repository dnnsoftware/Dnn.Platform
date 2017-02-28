using System;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Models;
using DotNetNuke.Services.Scheduling;

namespace Dnn.ExportImport.Components.Engines
{
    public class ExportImportEngine
    {
        public ExportImportEngine()
        {
            //TODO: add initialization parameters
        }

        public int ProgressPercentage
        {
            get
            {
                //TODO:
                throw new NotImplementedException();
            }
        }

        public ExportImportResult Export(ExportImportJob exportJob, ScheduleHistoryItem scheduleHistoryItem)
        {
            //TODO:
            exportJob.JobStatus = JobStatus.InProgress;
            throw new NotImplementedException();
        }

        public ExportImportResult Import(ExportImportJob importJob, ScheduleHistoryItem scheduleHistoryItem)
        {
            //TODO:
            importJob.JobStatus = JobStatus.InProgress;
            throw new NotImplementedException();
        }
    }
}