// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Entities
{
    using System;

    /// <summary>An import/export setting.</summary>
    [Serializable]
    public class ExportImportSetting
    {
        /// <summary>Gets or sets the setting name.</summary>
        public string SettingName { get; set; }

        /// <summary>Gets or sets the setting value.</summary>
        public string SettingValue { get; set; }

        /// <summary>Gets or sets a value indicating whether the setting is secure.</summary>
        public bool SettingIsSecure { get; set; }

        /// <summary>Gets or sets the ID of the user that created the setting.</summary>
        public int CreatedByUserId { get; set; }
    }
}
