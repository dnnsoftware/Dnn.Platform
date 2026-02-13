// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.DataAccess.Models
{
    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>A database entity representing a setting.</summary>
    [TableName("Cantarus_PolyDeploy_Settings")]
    [PrimaryKey("SettingID")]
    public class Setting
    {
        /// <summary>Gets or sets the setting ID.</summary>
        public int SettingId { get; set; }

        /// <summary>Gets or sets the setting group.</summary>
        public string Group { get; set; }

        /// <summary>Gets or sets the setting key.</summary>
        public string Key { get; set; }

        /// <summary>Gets or sets the setting value.</summary>
        public string Value { get; set; }
    }
}
