using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    [JsonObject]
    public class ExportFileInfo
    {
        public string FileName { get; set; }
        public int FileSizeKb { get; set; }
        public string DownloadLink { get; set; }
    }
}