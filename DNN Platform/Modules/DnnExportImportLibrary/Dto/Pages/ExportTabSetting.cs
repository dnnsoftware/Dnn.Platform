// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace Dnn.ExportImport.Dto.Pages
{
    using System;

    public class ExportTabSetting : BasicExportImportDto
    {
        public int TabID { get; set; }

        public string SettingName { get; set; }

        public string SettingValue { get; set; }

        public int? CreatedByUserID { get; set; }

        public DateTime? CreatedOnDate { get; set; }

        public int? LastModifiedByUserID { get; set; }

        public DateTime? LastModifiedOnDate { get; set; }

        public string CreatedByUserName { get; set; }

        public string LastModifiedByUserName { get; set; }
    }
}
