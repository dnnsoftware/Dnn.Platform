using System;

// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Dto.Pages
{
    public class ExportPackage : BasicExportImportDto
    {
        public string PackageType { get; set; }

        public string PackageName { get; set; }

        public Version Version { get; set; }

        public string PackageFileName { get; set; }
    }
}
