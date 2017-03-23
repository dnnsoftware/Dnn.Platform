using System;
using System.Data;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Modules;

namespace Dnn.ExportImport.Components.Entities
{
    [Serializable]
    [TableName("ExportImportCheckpoints")]
    [PrimaryKey("CheckpointId")]
    public class ExportImportChekpoint : IHydratable
    {
        private double _progress;

        public int CheckpointId { get; set; }
        public int JobId { get; set; }
        public string Category { get; set; }
        public int Stage { get; set; } // all stages start from 0 and increase
        public string StageData { get; set; } // discretionary data

        public double Progress
        {
            get { return _progress; }
            set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                _progress = value;
            }
        }

        public int KeyID
        {
            get { return CheckpointId; }
            set { CheckpointId = value; }
        }

        public void Fill(IDataReader dr)
        {
            CheckpointId = Null.SetNullInteger(dr[nameof(CheckpointId)]);
            JobId = Null.SetNullInteger(dr[nameof(JobId)]);
            Category = Null.SetNullString(dr[nameof(Category)]);
            Stage = Null.SetNullInteger(dr[nameof(Stage)]);
            StageData = Null.SetNullString(dr[nameof(StageData)]);
            Progress = Null.SetNullInteger(dr[nameof(Progress)]);
        }
    }
}