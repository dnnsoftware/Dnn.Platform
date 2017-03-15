using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto.Pages
{
    [JsonObject]
    public class ExportPage : BasicExportImportDto
    {
        public int TabId { get; set; }
        public string TabName { get; set; }
        public int ParentTabId { get; set; }
        public string ParentTabName { get; set; }
        public bool Deleted;
    }
}