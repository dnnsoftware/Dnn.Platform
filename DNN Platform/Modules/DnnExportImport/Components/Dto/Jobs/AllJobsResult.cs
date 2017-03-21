using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Jobs
{
    [JsonObject]
    public class AllJobsResult
    {
        public string PortalName { get; set; }
        public int TotalJobs { get; set; }
        public IEnumerable<JobItem> Jobs { get; set; }
    }
}