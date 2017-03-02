using System;

// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Components.Dto.Taxonomy
{
    public class TaxonomyVocabulary : BasicExportImportDto
    {
        public int VocabularyID { get; set; }
        public int VocabularyTypeID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Weight { get; set; }
        public int? ScopeID { get; set; }
        public int ScopeTypeID { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }
        public bool IsSystem { get; set; }

        public string CreatedByUserName { get; set; }
        public string LastModifiedByUserName { get; set; }
    }
}