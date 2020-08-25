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

    [JsonObject]
    public class JobItem : IDateTimeConverter
    {
        public string CreatedOnString => Util.GetDateTimeString(this.CreatedOn);

        public string CompletedOnString => Util.GetDateTimeString(this.CompletedOn);
        public int JobId { get; set; }

        public int PortalId { get; set; }

        public string User { get; set; }

        public string JobType { get; set; }

        public int Status { get; set; }

        public bool Cancelled { get; set; }

        public string JobStatus { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public string ExportFile { get; set; }

        // public IEnumerable<LogItem> Summary { get; set; }
        public ImportExportSummary Summary { get; set; }

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
