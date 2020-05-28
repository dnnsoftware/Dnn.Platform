// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
