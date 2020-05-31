// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
        public string AssemblyName { get; set; }
        public string Category { get; set; }
        public int Stage { get; set; } // all stages start from 0 and increase
        public string StageData { get; set; } // discretionary data
        public DateTime StartDate { get; set; }
        public DateTime LastUpdateDate { get; set; }

        public bool Completed { get; set; }

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

        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }

        public int KeyID
        {
            get { return CheckpointId; }
            set { CheckpointId = value; }
        }

        public void Fill(IDataReader dr)
        {
            CheckpointId = Null.SetNullInteger(dr[nameof(CheckpointId)]);
            JobId = Null.SetNullInteger(dr[nameof(JobId)]);
            AssemblyName = Null.SetNullString(dr[nameof(AssemblyName)]);
            Category = Null.SetNullString(dr[nameof(Category)]);
            Stage = Null.SetNullInteger(dr[nameof(Stage)]);
            StageData = Null.SetNullString(dr[nameof(StageData)]);
            Progress = Null.SetNullInteger(dr[nameof(Progress)]);
            TotalItems = Null.SetNullInteger(dr[nameof(TotalItems)]);
            ProcessedItems = Null.SetNullInteger(dr[nameof(ProcessedItems)]);
            StartDate = Null.SetNullDateTime(dr[nameof(StartDate)]);
            LastUpdateDate = Null.SetNullDateTime(dr[nameof(LastUpdateDate)]);
            Completed = Null.SetNullBoolean(dr[nameof(Completed)]);
        }
    }
}
