using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    [JsonObject]
    public class ImportExportSummary
    {
        public ImportExportSummary()
        {
            SummaryItems = new List<SummaryItem>();
        }
        public bool IncludeProfileProperties { get; set; }
        public bool IncludePermission { get; set; }
        public bool IncludeExtensions { get; set; }
        public bool IncludeDeletions { get; set; }
        public IEnumerable<SummaryItem> SummaryItems { get; set; }
        public ExportFileInfo ExportFileInfo { get; set; }
    }
}
