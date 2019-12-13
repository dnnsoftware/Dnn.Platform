// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Dto.Taxonomy
{
    public class TaxonomyVocabularyType : BasicExportImportDto
    {
        public int VocabularyTypeID { get; set; } // enum VocabularyType
        public string VocabularyType { get; set; }
    }
}
