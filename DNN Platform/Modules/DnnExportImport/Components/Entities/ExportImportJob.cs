using System;
using System.Data;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

namespace Dnn.ExportImport.Components.Entities
{
    [Serializable]
    public class ExportImportJob : IHydratable
    {
        public int JobId { get; set; }
        public int PortalId { get; set; }
        public JobType JobType { get; set; }
        public JobStatus JobStatus { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string ExportFile { get; set; }
        public string JobObject { get; set; }

        public int KeyID
        {
            get { return JobId; }
            set { JobId = value; }
        }

        public void Fill(IDataReader dr)
        {
            JobId = Null.SetNullInteger(dr["JobId"]);
            PortalId = Null.SetNullInteger(dr["PortalId"]);
            JobType = (JobType)Null.SetNullInteger(dr["JobType"]);
            JobStatus = (JobStatus)Null.SetNullInteger(dr["JobStatus"]);

            CreatedOn = Null.SetNullDateTime(dr["CreatedOn"]);
            UpdatedOn = Null.SetNullDateTime(dr["UpdatedOn"]);
            CompletedOn = Null.SetNullDateTime(dr["CompletedOn"]);

            ExportFile = Null.SetNullString(dr["ExportFile"]);
            JobObject = Null.SetNullString(dr["JobObject"]);
        }
    }
}