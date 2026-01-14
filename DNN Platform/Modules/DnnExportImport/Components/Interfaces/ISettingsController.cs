// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Interfaces
{
    using System.Collections.Generic;

    using Dnn.ExportImport.Components.Entities;

    /// <summary>A contract specifying the ability to manage import/export settings.</summary>
    public interface ISettingsController
    {
        /// <summary>Gets all the settings.</summary>
        /// <returns>A sequence of settings.</returns>
        IEnumerable<ExportImportSetting> GetAllSettings();

        /// <summary>Gets a setting.</summary>
        /// <param name="settingName">The setting name.</param>
        /// <returns>The setting or <see langword="null"/>.</returns>
        ExportImportSetting GetSetting(string settingName);

        /// <summary>Add a setting.</summary>
        /// <param name="exportImportSetting">The setting to add.</param>
        void AddSetting(ExportImportSetting exportImportSetting);
    }
}
