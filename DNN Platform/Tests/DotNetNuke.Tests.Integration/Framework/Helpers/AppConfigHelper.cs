// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017, DNN Corp.
// All Rights Reserved

using System.Configuration;

namespace DotNetNuke.Tests.Integration.Framework.Helpers
{
    public static class AppConfigHelper
    {
        private const string DefaultWebsiteName = "http://TestSiteCE.lvh.me";
        private const string DefaultHostName = "host";
        private const string DefaultHostPwd = "dnnhost";
        private const string DefaultLoginCookie = ".DOTNETNUKE";

        private static string _siteUrl;
        private static string _hostUserName;
        private static string _hostPassword;
        private static string _objectQualifier;
        private static string _connectionString;
        private static string _loginCookieString;

        public static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static bool GetAppSettingAsBool(string key, bool defaultValue)
        {
            bool value;
            return bool.TryParse(GetAppSetting(key), out value) ? value : defaultValue;
        }

        public static int GetAppSettingAsInt(string key, int defaultValue)
        {
            int value;
            return int.TryParse(GetAppSetting(key), out value) ? value : defaultValue;
        }

        public static string ConnectionString => 
            _connectionString ?? (_connectionString = ConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString);

        public static string ObjectQualifier => 
            _objectQualifier ?? (_objectQualifier = GetAppSetting("objectQualifier"));

        public static string SiteUrl => 
            _siteUrl ?? (_siteUrl = GetAppSetting("siteUrl") ?? DefaultWebsiteName);

        public static string HostUserName => 
            _hostUserName ?? (_hostUserName = GetAppSetting("hostUserName") ?? DefaultHostName);

        public static string HostPassword => 
            _hostPassword ?? (_hostPassword = GetAppSetting("hostPassword") ?? DefaultHostPwd);

        public static string LoginCookie => 
            _loginCookieString ?? (_loginCookieString = GetAppSetting("loginCookie") ?? DefaultLoginCookie);
    }
}
