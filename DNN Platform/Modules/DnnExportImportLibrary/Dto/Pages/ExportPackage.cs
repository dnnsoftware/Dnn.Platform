// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace Dnn.ExportImport.Dto.Pages
{
    using System;

    public class ExportPackage : BasicExportImportDto
    {
        public string PackageType { get; set; }

        public string PackageName { get; set; }

        public Version Version { get; set; }

        public string PackageFileName { get; set; }
    }
}
