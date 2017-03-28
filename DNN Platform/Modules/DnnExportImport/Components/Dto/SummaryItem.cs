using Dnn.ExportImport.Components.Common;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    [JsonObject]
    public class SummaryItem
    {
        public string Category { get; set; }

        public string InfoText => Localization.GetString(Category + ".InfoText", Constants.SharedResources);

        public int TotalItems { get; set; }

        public int ProcessedItems { get; set; }

        public int ProgressPercentage { get; set; }

        public bool Completed { get; set; }

        public int Order { get; set; }

        public bool ShowItem { get; set; }
    }
}
