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

    /// <summary>A table with log items about an import/export job.</summary>
    [Serializable]
    [TableName("ExportImportJobLogs")]
    [PrimaryKey("JobLogId")]
    public class ExportImportJobLog : IHydratable
    {
        /// <summary>Gets or sets the job log ID.</summary>
        public int JobLogId { get; set; }

        /// <summary>Gets or sets the job ID.</summary>
        public int JobId { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the value.</summary>
        public string Value { get; set; }

        /// <summary>Gets or sets the level.</summary>
        public int Level { get; set; }

        /// <summary>Gets or sets the date/time on which the log was created.</summary>
        public DateTime CreatedOnDate { get; set; }

        /// <inheritdoc />
        public int KeyID
        {
            get { return this.JobLogId; }
            set { this.JobLogId = value; }
        }

        /// <inheritdoc />
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
