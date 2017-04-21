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
