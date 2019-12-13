// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using Dnn.ExportImport.Components.Entities;

namespace Dnn.ExportImport.Components.Interfaces
{
    public interface ISettingsController
    {
        IEnumerable<ExportImportSetting> GetAllSettings();
        ExportImportSetting GetSetting(string settingName);
        void AddSetting(ExportImportSetting exportImportSetting);
    }
}
