using System;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Jobs
{
    [JsonObject]
    public class LogItem
    {
        public DateTime CreatedOnDate { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        [JsonIgnore]
        public bool IsSummary { get; set; }
    }
}