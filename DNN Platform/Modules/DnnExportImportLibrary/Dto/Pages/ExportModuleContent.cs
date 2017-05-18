// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Dto.Pages
{
    public class ExportModuleContent : BasicExportImportDto
    {
        public int ModuleID { get; set; }
        public int ModuleDefID { get; set; }
        public bool IsRestored { get; set; }
        public string XmlContent { get; set; }
    }
}