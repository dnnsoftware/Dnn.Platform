using System;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Components.Dto.Pages
{
    [JsonObject]
    public class ExportModuleContent : BasicExportImportDto
    {
        public int ModuleID { get; set; }
        public int ModuleDefID { get; set; }
        public string XmlContent { get; set; }
    }
}