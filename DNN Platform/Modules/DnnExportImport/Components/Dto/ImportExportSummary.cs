using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    /// <summary>
    /// Import/Export summary class to provide information about what will happen with this job.
    /// </summary>
    [JsonObject]
    public class ImportExportSummary
    {
        public ImportExportSummary()
        {
            SummaryItems = new List<SummaryItem>();
        }
        /// <summary>
        /// Does this import/export includes the properties definitions or not.
        /// </summary>
        public bool IncludeProfileProperties { get; set; }
        /// <summary>
        /// Does this import/export includes the permission or not.
        /// </summary>
        public bool IncludePermission { get; set; }
        /// <summary>
        /// Does this import/export includes the modules or not.
        /// </summary>
        public bool IncludeExtensions { get; set; }
        /// <summary>
        /// Does this import/export includes the deleted items or not.
        /// </summary>
        public bool IncludeDeletions { get; set; }
        /// <summary>
        /// Summary of each item export.
        /// </summary>
        public IEnumerable<SummaryItem> SummaryItems { get; set; }
        /// <summary>
        /// Exported file information.
        /// </summary>
        public ExportFileInfo ExportFileInfo { get; set; }
    }
}
