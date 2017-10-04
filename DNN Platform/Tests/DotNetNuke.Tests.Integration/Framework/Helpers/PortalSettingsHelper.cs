// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017, DNN Corp.
// All Rights Reserved

namespace DotNetNuke.Tests.Integration.Framework.Helpers
{
    public static class PortalSettingsHelper
    {
        /// <summary>
        /// Gets a portal setting value
        /// </summary>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="portalId">The optional PortalId, default to 0</param>
        /// <returns>The string value of the setting</returns>
        public static string GetPortalSetting(string settingName, int portalId = 0)
        {
            var query = string.Format(@"SELECT SettingValue 
                                        FROM {{objectQualifier}}PortalSettings 
                                        WHERE SettingName = '{0}' 
                                            AND PortalId = {1}", settingName, portalId);
            
            return DatabaseHelper.ExecuteScalar<string>(query);
        }

        /// <summary>
        /// Sets a portal setting value, adding or updating the setting as required
        /// </summary>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="settingValue">The value of the setting</param>
        /// <param name="portalId">The optional PortalId, default to 0</param>
        public static void SetPortalSetting(string settingName, string settingValue, int portalId = 0)
        {
            var query = string.Format(@"MERGE INTO {{objectQualifier}}PortalSettings s
                    USING (SELECT {2} PortalId, '{0}' SettingName, '{1}' SettingValue) AS v
                    ON s.SettingName = v.SettingName
                    WHEN MATCHED THEN UPDATE SET s.SettingValue = v.SettingValue
                    WHEN NOT MATCHED THEN INSERT (PortalId, SettingName, SettingValue) 
				                          VALUES (v.PortalId, v.SettingName, v.SettingValue);",
                        settingName, settingValue, portalId);

            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();
        }
    }
}
