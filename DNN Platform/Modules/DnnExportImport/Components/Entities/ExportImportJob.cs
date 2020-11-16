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

    [Serializable]
    [TableName("ExportImportJobs")]
    [PrimaryKey("JobId")]
    public class ExportImportJob : IHydratable
    {
        public int JobId { get; set; }

        public int PortalId { get; set; }

        public JobType JobType { get; set; }

        public JobStatus JobStatus { get; set; }

        public bool IsCancelled { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int CreatedByUserId { get; set; }

        public DateTime CreatedOnDate { get; set; }

        public DateTime LastModifiedOnDate { get; set; }

        public DateTime? CompletedOnDate { get; set; }

        public string Directory { get; set; }

        public string JobObject { get; set; }

        public int KeyID
        {
            get { return this.JobId; }
            set { this.JobId = value; }
        }

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
                    this.CreatedOnDate.Year, this.CreatedOnDate.Month, this.CreatedOnDate.Day,
                    this.CreatedOnDate.Hour, this.CreatedOnDate.Minute, this.CreatedOnDate.Second,
                    this.CreatedOnDate.Millisecond, DateTimeKind.Utc);
            }

            if (this.LastModifiedOnDate.Kind != DateTimeKind.Utc)
            {
                this.LastModifiedOnDate = new DateTime(
                    this.LastModifiedOnDate.Year, this.LastModifiedOnDate.Month, this.LastModifiedOnDate.Day,
                    this.LastModifiedOnDate.Hour, this.LastModifiedOnDate.Minute, this.LastModifiedOnDate.Second,
                    this.LastModifiedOnDate.Millisecond, DateTimeKind.Utc);
            }
        }
    }
}
