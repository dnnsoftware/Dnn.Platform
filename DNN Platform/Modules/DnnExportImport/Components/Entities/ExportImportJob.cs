// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Entities
{
    using System;
    using System.Data;

    using Dnn.ExportImport.Components.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Modules;

    /// <summary>A table of import/export jobs.</summary>
    [Serializable]
    [TableName("ExportImportJobs")]
    [PrimaryKey("JobId")]
    public class ExportImportJob : IHydratable
    {
        /// <summary>Gets or sets the job ID.</summary>
        public int JobId { get; set; }

        /// <summary>Gets or sets the portal ID.</summary>
        public int PortalId { get; set; }

        /// <summary>Gets or sets the job type.</summary>
        public JobType JobType { get; set; }

        /// <summary>Gets or sets the job status.</summary>
        public JobStatus JobStatus { get; set; }

        /// <summary>Gets or sets a value indicating whether the job is canceled.</summary>
        public bool IsCancelled { get; set; }

        /// <summary>Gets or sets the job name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the job description.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the ID of the user that created the job.</summary>
        public int CreatedByUserId { get; set; }

        /// <summary>Gets or sets the date/time the job was created.</summary>
        public DateTime CreatedOnDate { get; set; }

        /// <summary>Gets or sets the date/time the job was last modified.</summary>
        public DateTime LastModifiedOnDate { get; set; }

        /// <summary>Gets or sets the date/time the job was completed (or <see langword="null"/> if the job is not complete).</summary>
        public DateTime? CompletedOnDate { get; set; }

        /// <summary>Gets or sets the directory.</summary>
        public string Directory { get; set; }

        /// <summary>Gets or sets the job object serialized as JSON.</summary>
        public string JobObject { get; set; }

        /// <inheritdoc />
        public int KeyID
        {
            get { return this.JobId; }
            set { this.JobId = value; }
        }

        /// <inheritdoc />
        public void Fill(IDataReader dr)
        {
            this.JobId = Null.SetNullInteger(dr[nameof(this.JobId)]);
            this.PortalId = Null.SetNullInteger(dr[nameof(this.PortalId)]);
            this.JobType = (JobType)Null.SetNullInteger(dr[nameof(this.JobType)]);
            this.JobStatus = (JobStatus)Null.SetNullInteger(dr[nameof(this.JobStatus)]);
            this.IsCancelled = Null.SetNullBoolean(dr[nameof(this.IsCancelled)]);
            this.Name = Null.SetNullString(dr[nameof(this.Name)]);
            this.Description = Null.SetNullString(dr[nameof(this.Description)]);
            this.CreatedByUserId = Null.SetNullInteger(dr[nameof(this.CreatedByUserId)]);
            this.CreatedOnDate = Null.SetNullDateTime(dr[nameof(this.CreatedOnDate)]);
            this.LastModifiedOnDate = Null.SetNullDateTime(dr[nameof(this.LastModifiedOnDate)]);
            this.CompletedOnDate = Null.SetNullDateTime(dr[nameof(this.CompletedOnDate)]);
            this.Directory = Null.SetNullString(dr[nameof(this.Directory)]);
            this.JobObject = Null.SetNullString(dr[nameof(this.JobObject)]);

            if (this.CreatedOnDate.Kind != DateTimeKind.Utc)
            {
                this.CreatedOnDate = new DateTime(
                    this.CreatedOnDate.Year,
                    this.CreatedOnDate.Month,
                    this.CreatedOnDate.Day,
                    this.CreatedOnDate.Hour,
                    this.CreatedOnDate.Minute,
                    this.CreatedOnDate.Second,
                    this.CreatedOnDate.Millisecond,
                    DateTimeKind.Utc);
            }

            if (this.LastModifiedOnDate.Kind != DateTimeKind.Utc)
            {
                this.LastModifiedOnDate = new DateTime(
                    this.LastModifiedOnDate.Year,
                    this.LastModifiedOnDate.Month,
                    this.LastModifiedOnDate.Day,
                    this.LastModifiedOnDate.Hour,
                    this.LastModifiedOnDate.Minute,
                    this.LastModifiedOnDate.Second,
                    this.LastModifiedOnDate.Millisecond,
                    DateTimeKind.Utc);
            }
        }
    }
}
