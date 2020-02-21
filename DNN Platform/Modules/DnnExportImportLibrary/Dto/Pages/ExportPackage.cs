// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
