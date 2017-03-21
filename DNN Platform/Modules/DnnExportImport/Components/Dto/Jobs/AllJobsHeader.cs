using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Jobs
{
    [JsonObject]
    public class AllJobsHeader
    {
        public int PortalId { get; set; }
        public string PortalName { get; set; }
        public int JobsCount { get; set; }
    }
}