using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    [JsonObject]
    public class ExportFileInfo
    {
        /// <summary>
        /// Path for exported files.
        /// </summary>
        public string ExportPath { get; set; }
        /// <summary>
        /// Total size of exported files in KB.
        /// </summary>
        public double ExportSizeKb { get; set; }
    }
}