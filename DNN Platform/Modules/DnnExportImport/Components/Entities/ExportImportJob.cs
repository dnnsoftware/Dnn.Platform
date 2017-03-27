#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
#endregion

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
                    DateTimeKind.Utc);
            }
        }
    }
}