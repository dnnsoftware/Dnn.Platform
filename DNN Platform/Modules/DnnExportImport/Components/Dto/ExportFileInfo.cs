using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    [JsonObject]
    public class ExportFileInfo
    {
        /// <summary>
        /// Exported file name.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Exported file size in KB.
        /// </summary>
        public int FileSizeKb { get; set; }
    }
}