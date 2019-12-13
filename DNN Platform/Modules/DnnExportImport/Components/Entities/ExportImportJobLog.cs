using System;
using System.Data;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Modules;

namespace Dnn.ExportImport.Components.Entities
{
    [Serializable]
    [TableName("ExportImportJobLogs")]
    [PrimaryKey("JobLogId")]
    public class ExportImportJobLog : IHydratable
    {
        public int JobLogId { get; set; }
        public int JobId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int Level { get; set; }
        public DateTime CreatedOnDate { get; set; }

        public int KeyID
        {
            get { return JobLogId; }
            set { JobLogId = value; }
        }

        public void Fill(IDataReader dr)
        {
            JobLogId = Null.SetNullInteger(dr[nameof(JobLogId)]);
            JobId = Null.SetNullInteger(dr[nameof(JobId)]);
            Name = Null.SetNullString(dr[nameof(Name)]);
            Value = Null.SetNullString(dr[nameof(Value)]);
            Level = Null.SetNullInteger(dr[nameof(Level)]);
            CreatedOnDate = Null.SetNullDateTime(dr[nameof(CreatedOnDate)]);
        }
    }
}
