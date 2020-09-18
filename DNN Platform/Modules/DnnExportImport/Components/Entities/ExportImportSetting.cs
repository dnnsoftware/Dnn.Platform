// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Entities
{
    using System;

    [Serializable]
    public class ExportImportSetting
    {
        public string SettingName { get; set; }

        public string SettingValue { get; set; }

        public bool SettingIsSecure { get; set; }

        public int CreatedByUserId { get; set; }
    }
}
