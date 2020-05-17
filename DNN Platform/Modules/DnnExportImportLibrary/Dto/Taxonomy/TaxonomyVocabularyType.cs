// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Dto.Taxonomy
{
    public class TaxonomyVocabularyType : BasicExportImportDto
    {
        public int VocabularyTypeID { get; set; } // enum VocabularyType
        public string VocabularyType { get; set; }
    }
}
