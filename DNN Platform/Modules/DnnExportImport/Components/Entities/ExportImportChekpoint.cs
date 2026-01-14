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

    /// <summary>A table of checkpoints in an import/export job.</summary>
    [Serializable]
    [TableName("ExportImportCheckpoints")]
    [PrimaryKey("CheckpointId")]
    public class ExportImportChekpoint : IHydratable
    {
        private double progress;

        /// <summary>Gets or sets the ID of the checkpoint.</summary>
        public int CheckpointId { get; set; }

        /// <summary>Gets or sets the job ID.</summary>
        public int JobId { get; set; }

        /// <summary>Gets or sets the assembly name.</summary>
        public string AssemblyName { get; set; }

        /// <summary>Gets or sets the category.</summary>
        public string Category { get; set; }

        /// <summary>Gets or sets the stage index.</summary>
        /// <remarks>all stages start from 0 and increase.</remarks>
        public int Stage { get; set; }

        /// <summary>Gets or sets any discretionary data for the stage.</summary>
        public string StageData { get; set; }

        /// <summary>Gets or sets the start date.</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Gets or sets the last update date.</summary>
        public DateTime LastUpdateDate { get; set; }

        /// <summary>Gets or sets a value indicating whether the checkpoint is complete.</summary>
        public bool Completed { get; set; }

        /// <summary>Gets or sets the progress (from <c>0</c> to <c>100</c>).</summary>
        public double Progress
        {
            get
            {
                return this.progress;
            }

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

                this.progress = value;
            }
        }

        /// <summary>Gets or sets the total item count.</summary>
        public int TotalItems { get; set; }

        /// <summary>Gets or sets the processed item count.</summary>
        public int ProcessedItems { get; set; }

        /// <inheritdoc />
        public int KeyID
        {
            get { return this.CheckpointId; }
            set { this.CheckpointId = value; }
        }

        /// <inheritdoc />
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
