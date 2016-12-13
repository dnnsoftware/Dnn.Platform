// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017, DNN Corp.
// All Rights Reserved

namespace DotNetNuke.Tests.Integration.Framework.Helpers
{
    public static class HostSettingsHelper
    {
        public static string GetHostSettingValue(string hostSettingName)
        {
            var query = string.Format("SELECT SettingValue FROM {{objectQualifier}}HostSettings WHERE SettingName='" + hostSettingName +"';");
            return DatabaseHelper.ExecuteScalar<string>(query);
        }
    }
}
