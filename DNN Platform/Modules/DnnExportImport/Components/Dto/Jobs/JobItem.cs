// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Dto.Jobs
{
    using System;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Interfaces;
    using DotNetNuke.Entities.Users;
    using Newtonsoft.Json;

    /// <summary>A data transfer object with information about an import/export job.</summary>
    [JsonObject]
    public class JobItem : IDateTimeConverter
    {
        /// <summary>Gets a formatted string version of <see cref="CreatedOn"/>.</summary>
        public string CreatedOnString => Util.GetDateTimeString(this.CreatedOn);

        /// <summary>Gets a formatted string version of <see cref="CompletedOn"/>.</summary>
        public string CompletedOnString => Util.GetDateTimeString(this.CompletedOn);

        /// <summary>Gets or sets the job ID.</summary>
        public int JobId { get; set; }

        /// <summary>Gets or sets the portal ID.</summary>
        public int PortalId { get; set; }

        /// <summary>Gets or sets the user's display name or ID.</summary>
        public string User { get; set; }

        /// <summary>Gets or sets the job type.</summary>
        public string JobType { get; set; }

        /// <summary>Gets or sets the status (based on <see cref="JobStatus"/> values).</summary>
        public int Status { get; set; }

        /// <summary>Gets or sets a value indicating whether the job is canceled.</summary>
        public bool Cancelled { get; set; }

        /// <summary>Gets or sets the job status.</summary>
        public string JobStatus { get; set; }

        /// <summary>Gets or sets the job name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the job description.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the date/time the job was created.</summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>Gets or sets the date/time the job was completed, or <see langword="null"/> if it hasn't completed.</summary>
        public DateTime? CompletedOn { get; set; }

        /// <summary>Gets or sets the file/directory exported by the job.</summary>
        public string ExportFile { get; set; }

        ////public IEnumerable<LogItem> Summary { get; set; }

        /// <summary>Gets or sets the job summary.</summary>
        public ImportExportSummary Summary { get; set; }

        /// <inheritdoc />
        public void ConvertToLocal(UserInfo userInfo)
        {
            if (userInfo == null)
            {
                return;
            }

            this.Summary?.ConvertToLocal(userInfo);
            this.CreatedOn = Util.ToLocalDateTime(this.CreatedOn, userInfo);
            if (this.CompletedOn != null)
            {
                this.CompletedOn = Util.ToLocalDateTime(this.CompletedOn.Value, userInfo);
            }
        }
    }
}
