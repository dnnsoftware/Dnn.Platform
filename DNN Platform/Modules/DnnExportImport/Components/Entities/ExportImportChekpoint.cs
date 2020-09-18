// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Entities
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Modules;

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
            get { return this._progress; }

            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                else if (value > 100)
                {
                    value = 100;
                }

                this._progress = value;
            }
        }

        public int TotalItems { get; set; }

        public int ProcessedItems { get; set; }

        public int KeyID
        {
            get { return this.CheckpointId; }
            set { this.CheckpointId = value; }
        }

        public void Fill(IDataReader dr)
        {
            this.CheckpointId = Null.SetNullInteger(dr[nameof(this.CheckpointId)]);
            this.JobId = Null.SetNullInteger(dr[nameof(this.JobId)]);
            this.AssemblyName = Null.SetNullString(dr[nameof(this.AssemblyName)]);
            this.Category = Null.SetNullString(dr[nameof(this.Category)]);
            this.Stage = Null.SetNullInteger(dr[nameof(this.Stage)]);
            this.StageData = Null.SetNullString(dr[nameof(this.StageData)]);
            this.Progress = Null.SetNullInteger(dr[nameof(this.Progress)]);
            this.TotalItems = Null.SetNullInteger(dr[nameof(this.TotalItems)]);
            this.ProcessedItems = Null.SetNullInteger(dr[nameof(this.ProcessedItems)]);
            this.StartDate = Null.SetNullDateTime(dr[nameof(this.StartDate)]);
            this.LastUpdateDate = Null.SetNullDateTime(dr[nameof(this.LastUpdateDate)]);
            this.Completed = Null.SetNullBoolean(dr[nameof(this.Completed)]);
        }
    }
}
