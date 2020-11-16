// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System;
    using System.Collections.Generic;

    using DNN.Integration.Test.Framework.Helpers;

    public static class TabController
    {
        private static int PortalId => DnnDataHelper.PortalId;

        public static int GetTabIdByTabName(string tabName)
        {
            var query = string.Format(
                "SELECT TOP(1) ISNULL(TabId, -1) FROM {{objectQualifier}}Tabs WHERE TabName = '{0}' AND ISNULL(PortalID,0)={1};",
                tabName, PortalId);

            return DatabaseHelper.ExecuteScalar<int>(query);
        }

        public static string GetTabNameByTabId(int tabId)
        {
            var query = string.Format(
                "SELECT TOP(1) TabName FROM {{objectQualifier}}Tabs WHERE TabId = '{0}' AND ISNULL(PortalID,0)={1};",
                tabId, PortalId);

            return DatabaseHelper.ExecuteScalar<string>(query);
        }

        public static int GetTabIdByPathName(string tabPath)
        {
            return DatabaseHelper.ExecuteScalar<int>(
                string.Format("SELECT TOP(1) TabId FROM {{objectQualifier}}vw_Tabs WHERE TabPath = '{0}' AND ISNULL(PortalID,0)={1};", tabPath, PortalId));
        }

        public static int GetModuleIdInsidePage(int pageId)
        {
            return DatabaseHelper.ExecuteScalar<int>(
                string.Format(
                    @"
                    SELECT TOP(1) ModuleID FROM {{objectQualifier}}vw_TabModules
                    WHERE TabId = '{0}' AND PaneName='contentPane' AND ContainerSrc IS NULL AND ISNULL(PortalID,0)={1};",
                    pageId, PortalId));
        }

        /// <summary>
        /// Returns all the tab related columns from vw_tabinfo.
        /// </summary>
        /// <returns></returns>
        public static dynamic GetTabInfo(int tabId)
        {
            var sql = string.Format(@"SELECT TOP 1 * FROM {{objectQualifier}}vw_Tabs WHERE TabId = '{0}' ORDER BY TabId ASC", tabId);
            return DatabaseHelper.ExecuteDynamicQuery(sql)[0];
        }

        /// <summary>
        /// Gets list of all modules on a page.
        /// </summary>
        /// <param name="pageId">Id of the page.</param>
        /// <returns>List of all the modules on a page.</returns>
        public static List<int> GetAllModuleIdInsidePage(int pageId)
        {
            var squery = string.Format(
                @"
                    SELECT ModuleID FROM {{objectQualifier}}vw_TabModules
                    WHERE TabId = '{0}' AND ISNULL(PortalID,0)={1};",
                pageId, PortalId);
            var result = DatabaseHelper.ExecuteQuery(squery);
            var listOfModules = new List<int>();
            foreach (var item in result)
            {
                object moduleId;
                item.TryGetValue("ModuleID", out moduleId);
                listOfModules.Add(Convert.ToInt32(moduleId));
            }

            return listOfModules;
        }

        /// <summary>
        /// Gets ModuleID of the first module on a page (any pane).
        /// </summary>
        /// <param name="pageId">id of the tab.</param>
        /// <returns>Returns ModuleID of the first module (top(1)) on a page.</returns>
        public static int GetModuleIdInsidePageAnyPane(int pageId)
        {
            return DatabaseHelper.ExecuteScalar<int>(
                string.Format(
                    @"
                    SELECT TOP(1) ModuleID FROM {{objectQualifier}}vw_TabModules
                    WHERE TabId = '{0}' AND ContainerSrc IS NULL AND ISNULL(PortalID,0)={1};",
                    pageId, PortalId));
        }

        public static int GetModuleIdInsidePage(int tabId, string moduleFriendlyName)
        {
            var query1 = string.Format(
                @"
                SELECT TOP(1) ModuleID FROM {{objectQualifier}}vw_TabModules tm
                INNER JOIN {{objectQualifier}}DesktopModules dm ON dm.DesktopModuleId = tm.DesktopModuleId 
                INNER JOIN {{objectQualifier}}ModuleDefinitions md ON md.ModuleDefID = tm.ModuleDefID 
                WHERE TabId = {0} AND ISNULL(PortalID,0)={1} AND (dm.FriendlyName = '{2}' OR md.FriendlyName = '{2}')
                ORDER BY tm.ModuleDefID, ModuleID;", tabId, PortalId, moduleFriendlyName);

            try
            {
                return DatabaseHelper.ExecuteScalar<int>(query1);
            }
            catch (Exception)
            {
                Console.WriteLine(@"Failed running query => {0}", query1);
                throw;
            }
        }

        public static string GetTabPath(string tabPath)
        {
            var parts = tabPath.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var path = string.Empty;
            var name = string.Empty;
            foreach (var s in parts)
            {
                path += "//" + s;
                var query = string.Format("SELECT TOP(1) TabName FROM {{objectQualifier}}Tabs WHERE TabPath = '{0}' AND ISNULL(PortalID,0)={1};", path, PortalId);
                var tabName = DatabaseHelper.ExecuteScalar<string>(query);
                name += "/" + tabName.Replace(" ", "-");
            }

            return name;
        }

        public static int GetTabModuleId(int tabId, int moduledId)
        {
            return DatabaseHelper.ExecuteScalar<int>(
               string.Format(
                   @"
                    SELECT TOP(1) TabModuleID FROM {{objectQualifier}}TabModules
                    WHERE TabID = {0} AND ModuleID = {1};", tabId, moduledId));
        }

        /// <summary>
        /// Returns all the tab urls.
        /// </summary>
        /// <returns></returns>
        public static IList<dynamic> GetTabUrls(int tabId)
        {
            var sql = string.Format(@"SELECT * FROM {{objectQualifier}}TabUrls WHERE TabId = '{0}' ORDER BY SeqNum DESC", tabId);
            return DatabaseHelper.ExecuteDynamicQuery(sql);
        }

        /// <summary>
        /// Adds tab setting. Deletes existing setting first.
        /// </summary>
        public static void SaveTabSetting(int tabId, string settingName, string settingValue)
        {
            DeleteTabSetting(tabId, settingName);
            AddTabSetting(tabId, settingName, settingValue);
        }

        /// <summary>
        /// Adds tab setting. Does not delete existing setting first.
        /// </summary>
        public static void AddTabSetting(int tabId, string settingName, string settingValue)
        {
            DatabaseHelper.ExecuteStoredProcedure(
                "UpdateTabSetting",
                tabId,
                settingName,
                settingValue,
                1); // CreatedByUserID
        }

        /// <summary>
        /// Deletes tab setting.
        /// </summary>
        public static void DeleteTabSetting(int tabId, string settingName)
        {
            DatabaseHelper.ExecuteStoredProcedure("DeleteTabSetting", tabId, settingName);
        }
    }
}
