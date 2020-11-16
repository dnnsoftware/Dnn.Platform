// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Dto.Taxonomy
{
    public class TaxonomyVocabularyType : BasicExportImportDto
    {
        public int VocabularyTypeID { get; set; } // enum VocabularyType

        public string VocabularyType { get; set; }
    }
}
