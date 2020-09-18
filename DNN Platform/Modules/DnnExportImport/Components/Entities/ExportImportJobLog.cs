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
            get { return this.JobLogId; }
            set { this.JobLogId = value; }
        }

        public void Fill(IDataReader dr)
        {
            this.JobLogId = Null.SetNullInteger(dr[nameof(this.JobLogId)]);
            this.JobId = Null.SetNullInteger(dr[nameof(this.JobId)]);
            this.Name = Null.SetNullString(dr[nameof(this.Name)]);
            this.Value = Null.SetNullString(dr[nameof(this.Value)]);
            this.Level = Null.SetNullInteger(dr[nameof(this.Level)]);
            this.CreatedOnDate = Null.SetNullDateTime(dr[nameof(this.CreatedOnDate)]);
        }
    }
}
