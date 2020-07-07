// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics.Config
{
    using System;

    [Serializable]
    public class AnalyticsSetting
    {
        private string _settingName;
        private string _settingValue;

        public string SettingName
        {
            get
            {
                return this._settingName;
            }

            set
            {
                this._settingName = value;
            }
        }

        public string SettingValue
        {
            get
            {
                return this._settingValue;
            }

            set
            {
                this._settingValue = value;
            }
        }
    }
}
