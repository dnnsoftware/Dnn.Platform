// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Dto.Workflow
{
    public class ExportContentWorkflowSource : BasicExportImportDto
    {
        public int SourceID { get; set; }
        public int WorkflowID { get; set; }
        public string SourceName { get; set; }
        public string SourceType { get; set; }
    }
}