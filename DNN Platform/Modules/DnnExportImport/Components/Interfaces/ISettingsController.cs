// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Interfaces
{
    using System.Collections.Generic;

    using Dnn.ExportImport.Components.Entities;

    public interface ISettingsController
    {
        IEnumerable<ExportImportSetting> GetAllSettings();

        ExportImportSetting GetSetting(string settingName);

        void AddSetting(ExportImportSetting exportImportSetting);
    }
}
