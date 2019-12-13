#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Analytics.Config
{
    [Serializable]
    public class AnalyticsSetting
    {
        private string _settingName;
        private string _settingValue;

        public string SettingName
        {
            get
            {
                return _settingName;
            }
            set
            {
                _settingName = value;
            }
        }

        public string SettingValue
        {
            get
            {
                return _settingValue;
            }
            set
            {
                _settingValue = value;
            }
        }
    }
}
