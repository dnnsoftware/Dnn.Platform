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
        public string TotalJobsString => Util.FormatNumber(TotalJobs);

        public DateTime? LastExportTime { get; set; }
        public DateTime? LastImportTime { get; set; }

        public string LastExportTimeString => Util.GetDateTimeString(LastExportTime);

        public string LastImportTimeString => Util.GetDateTimeString(LastImportTime);

        public IEnumerable<JobItem> Jobs { get; set; }

        public void ConvertToLocal(UserInfo userInfo)
        {
            LastExportTime = Util.ToLocalDateTime(LastExportTime, userInfo);
            LastImportTime = Util.ToLocalDateTime(LastImportTime, userInfo);

            if (userInfo == null) return;
            if (Jobs == null) return;
            var tempJobs = new List<JobItem>();

            foreach (var job in Jobs)
            {
                job.ConvertToLocal(userInfo);
                tempJobs.Add(job);
            }
            Jobs = tempJobs;
        }
    }
}
