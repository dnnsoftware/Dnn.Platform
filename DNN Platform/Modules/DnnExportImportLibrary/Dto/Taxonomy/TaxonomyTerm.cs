// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace Dnn.ExportImport.Dto.Taxonomy
{
    using System;

    public class TaxonomyTerm : BasicExportImportDto
    {
        public int TermID { get; set; }

        public int VocabularyID { get; set; }

        public int? ParentTermID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Weight { get; set; }

        // public int TermLeft { get; set; }
        // public int TermRight { get; set; }
        public int? CreatedByUserID { get; set; }

        public DateTime? CreatedOnDate { get; set; }

        public int? LastModifiedByUserID { get; set; }

        public DateTime? LastModifiedOnDate { get; set; }

        public string CreatedByUserName { get; set; }

        public string LastModifiedByUserName { get; set; }
    }
}
