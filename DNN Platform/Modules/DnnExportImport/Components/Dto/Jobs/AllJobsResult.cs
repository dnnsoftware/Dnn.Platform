// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Jobs
{
    [JsonObject]
    public class AllJobsResult : IDateTimeConverter
    {
        public int PortalId { get; set; }
        public string PortalName { get; set; }
        public int TotalJobs { get; set; }
        public string TotalJobsString => Util.FormatNumber(this.TotalJobs);

        public DateTime? LastExportTime { get; set; }
        public DateTime? LastImportTime { get; set; }

        public string LastExportTimeString => Util.GetDateTimeString(this.LastExportTime);

        public string LastImportTimeString => Util.GetDateTimeString(this.LastImportTime);

        public IEnumerable<JobItem> Jobs { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            this.LastExportTime = Util.ToLocalDateTime(this.LastExportTime, userInfo);
            this.LastImportTime = Util.ToLocalDateTime(this.LastImportTime, userInfo);

            if (userInfo == null)
            {
                return;
            }

            if (this.Jobs == null)
            {
                return;
            }

            var tempJobs = new List<JobItem>();

            foreach (var job in this.Jobs)
            {
                job.ConvertToLocal(userInfo);
                tempJobs.Add(job);
            }

            this.Jobs = tempJobs;
        }
    }
}
