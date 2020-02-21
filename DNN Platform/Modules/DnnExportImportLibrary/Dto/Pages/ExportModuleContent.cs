// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
