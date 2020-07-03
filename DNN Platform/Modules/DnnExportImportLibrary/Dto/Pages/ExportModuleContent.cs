// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
