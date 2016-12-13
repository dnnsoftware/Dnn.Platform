#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Data;

using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;

#endregion

namespace DotNetNuke.Entities.Host
{
    [Obsolete("Replaced in DotNetNuke 5.5 by HostController")]
    public class HostSettingsController
    {
        [Obsolete("Replaced in DotNetNuke 5.5 by HostController.GetHostSetting()")]
        public IDataReader GetHostSetting(string SettingName)
        {
            return DataProvider.Instance().GetHostSetting(SettingName);
        }

        [Obsolete("Replaced in DotNetNuke 5.5 by HostController.GetHostSetting()")]
        public IDataReader GetHostSettings()
        {
            return DataProvider.Instance().GetHostSettings();
        }

        [Obsolete("Replaced in DotNetNuke 5.5 by HostController.UpdateHostSetting()")]
        public void UpdateHostSetting(string SettingName, string SettingValue)
        {
            UpdateHostSetting(SettingName, SettingValue, false, true);
        }

        [Obsolete("Replaced in DotNetNuke 5.5 by HostController.UpdateHostSetting()")]
        public void UpdateHostSetting(string SettingName, string SettingValue, bool SettingIsSecure)
        {
            UpdateHostSetting(SettingName, SettingValue, SettingIsSecure, true);
        }

        [Obsolete("Replaced in DotNetNuke 5.5 by HostController.UpdateHostSetting()")]
        public void UpdateHostSetting(string SettingName, string SettingValue, bool SettingIsSecure, bool clearCache)
        {
            HostController.Instance.Update(new ConfigurationSetting {Key = SettingName, Value = SettingValue, IsSecure = SettingIsSecure});
        }
    }
}