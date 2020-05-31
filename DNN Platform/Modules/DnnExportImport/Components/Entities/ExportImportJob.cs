// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Data;
using Dnn.ExportImport.Components.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Modules;

namespace Dnn.ExportImport.Components.Entities
{
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
            get { return JobId; }
            set { JobId = value; }
        }

        public void Fill(IDataReader dr)
        {
            JobId = Null.SetNullInteger(dr[nameof(JobId)]);
            PortalId = Null.SetNullInteger(dr[nameof(PortalId)]);
            JobType = (JobType)Null.SetNullInteger(dr[nameof(JobType)]);
            JobStatus = (JobStatus)Null.SetNullInteger(dr[nameof(JobStatus)]);
            IsCancelled = Null.SetNullBoolean(dr[nameof(IsCancelled)]);
            Name = Null.SetNullString(dr[nameof(Name)]);
            Description = Null.SetNullString(dr[nameof(Description)]);
            CreatedByUserId = Null.SetNullInteger(dr[nameof(CreatedByUserId)]);
            CreatedOnDate = Null.SetNullDateTime(dr[nameof(CreatedOnDate)]);
            LastModifiedOnDate = Null.SetNullDateTime(dr[nameof(LastModifiedOnDate)]);
            CompletedOnDate = Null.SetNullDateTime(dr[nameof(CompletedOnDate)]);
            Directory = Null.SetNullString(dr[nameof(Directory)]);
            JobObject = Null.SetNullString(dr[nameof(JobObject)]);

            if (CreatedOnDate.Kind != DateTimeKind.Utc)
            {
                CreatedOnDate = new DateTime(
                    CreatedOnDate.Year, CreatedOnDate.Month, CreatedOnDate.Day,
                    CreatedOnDate.Hour, CreatedOnDate.Minute, CreatedOnDate.Second,
                    CreatedOnDate.Millisecond, DateTimeKind.Utc);
            }
            if (LastModifiedOnDate.Kind != DateTimeKind.Utc)
            {
                LastModifiedOnDate = new DateTime(
                    LastModifiedOnDate.Year, LastModifiedOnDate.Month, LastModifiedOnDate.Day,
                    LastModifiedOnDate.Hour, LastModifiedOnDate.Minute, LastModifiedOnDate.Second,
                    LastModifiedOnDate.Millisecond, DateTimeKind.Utc);
            }
        }
    }
}
