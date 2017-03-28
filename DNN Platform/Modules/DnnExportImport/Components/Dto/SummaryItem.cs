using Dnn.ExportImport.Components.Common;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    /// <summary>
    /// Details of the summary item to show in the export/import summary and progress.
    /// </summary>
    [JsonObject]
    public class SummaryItem
    {
        /// <summary>
        /// Category of the import/export. Also identifier for localization
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Localized text to show on UI. 
        /// </summary>
        //TODO: Note: This item might be removed and always derived from localization on UI.
        public string InfoText => Localization.GetString(Category + ".InfoText", Constants.SharedResources);

        /// <summary>
        /// Total items to import/export.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Items processed.
        /// </summary>
        public int ProcessedItems { get; set; }

        /// <summary>
        /// Progress in percentage.
        /// </summary>
        public int ProgressPercentage { get; set; }

        /// <summary>
        /// Order to show on UI.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Whether to show this item on UI or not.
        /// </summary>
        //TODO: Note: This property is not used as such and might be removed.
        public bool ShowItem { get; set; }
    }
}
