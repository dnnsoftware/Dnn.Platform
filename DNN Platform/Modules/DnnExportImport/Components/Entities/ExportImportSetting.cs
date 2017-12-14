using System;

namespace Dnn.ExportImport.Components.Entities
{
    [Serializable]
    public class ExportImportSetting
    {
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public bool SettingIsSecure { get; set; }
        public int CreatedByUserId { get; set; }
    }
}
