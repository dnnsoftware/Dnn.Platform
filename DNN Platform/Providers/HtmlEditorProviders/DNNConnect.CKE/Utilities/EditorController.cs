using System;
using System.Collections.Generic;
using DNNConnect.CKEditorProvider.Objects;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Host;

namespace DNNConnect.CKEditorProvider.Utilities
{
    public static class EditorController
    {
        /// <summary>
        /// Deletes all module settings of the Editor, for the specified <paramref name="tabId"/>.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        public static void DeleteAllModuleSettingsById(int tabId)
        {
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllModuleSettingsByTab", tabId);
        }

        /// <summary>
        /// Deletes all module settings of the Editor, for the Current Portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllModuleSettings(int portalId)
        {
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllModuleSettings", portalId.ToString());
        }

        /// <summary>
        /// Deletes all page settings of the Editor, for the Current Portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllPageSettings(int portalId)
        {
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllPageSettings", portalId.ToString());

            // Finally Clear Cache
            ClearEditorCache();
        }

        /// <summary>
        /// Deletes current page settings of the Editor, for the Current Portal.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        public static void DeleteCurrentPageSettings(int tabId)
        {
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteCurrentPageSettings", tabId.ToString());

            // Finally Clear Cache
            ClearEditorCache();
        }

        /// <summary>
        /// Deletes all page settings of the Editor, for the specified child tabs from the specified <paramref name="tabId"/>.
        /// </summary>
        /// <param name="tabId">
        /// The tab Id.
        /// </param>
        public static void DeleteAllChildPageSettings(int tabId)
        {
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllChildPageSettings", tabId);

            // Finally Clear Cache
            ClearEditorCache();
        }

        /// <summary>
        /// Deletes all portal settings of the Editor, for the Current Portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllPortalSettings(int portalId)
        {
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllPortalSettings", portalId.ToString());

            // Finally Clear Cache
            ClearEditorCache();
        }

        /// <summary>
        /// Deletes all Host settings of the Editor.
        /// </summary>
        public static void DeleteAllHostSettings()
        {
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllHostSettings");

            // Finally Clear Cache
            ClearEditorCache();
        }

        /// <summary>
        /// Gets the editor host settings.
        /// </summary>
        /// <returns>Returns the list of all Editor Host Settings</returns>
        public static List<EditorHostSetting> GetEditorHostSettings()
        {
            var editorHostSettings = new List<EditorHostSetting>();

            var cache = DataCache.GetCache("CKE_Host");

            if (cache == null)
            {
                var timeOut = 20 * Convert.ToInt32(Host.PerformanceSetting);

                using (var dr = DataProvider.Instance().ExecuteReader("CKE_GetEditorHostSettings"))
                {
                    while (dr.Read())
                    {
                        editorHostSettings.Add(
                            new EditorHostSetting(Convert.ToString(dr["SettingName"]), Convert.ToString(dr["SettingValue"])));
                    }
                }

                if (timeOut > 0)
                {
                    DataCache.SetCache("CKE_Host", editorHostSettings, TimeSpan.FromMinutes(timeOut));
                }
            }
            else
            {
                editorHostSettings = cache as List<EditorHostSetting>;
            }

            return editorHostSettings;
        }

        /// <summary>
        /// Adds or update's the editor host setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void AddOrUpdateEditorHostSetting(string settingName, string settingValue)
        {
            DataProvider.Instance().ExecuteNonQuery("CKE_AddOrUpdateEditorHostSetting", settingName, settingValue);
        }

        public static void ClearEditorCache()
        {
            DataCache.RemoveCache("CKE_Host");
        }
    }
}