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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Modules.Internal;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.ModuleCache;
using DotNetNuke.Services.OutputCache;

#endregion

namespace DotNetNuke.Entities.Modules
{
    public partial class ModuleController
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("The module caching feature has been updated in version 5.2.0.  This method is no longer used.")]
        public static string CacheDirectory()
        {
            return PortalController.Instance.GetCurrentPortalSettings().HomeDirectoryMapPath + "Cache";
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("The module caching feature has been updated in version 5.2.0.  This method is no longer used.")]
        public static string CacheFileName(int TabModuleID)
        {
            string strCacheKey = "TabModule:";
            strCacheKey += TabModuleID + ":";
            strCacheKey += Thread.CurrentThread.CurrentUICulture.ToString();
            return PortalController.Instance.GetCurrentPortalSettings().HomeDirectoryMapPath + "Cache" + "\\" + Globals.CleanFileName(strCacheKey) + ".resources";
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("The module caching feature has been updated in version 5.2.0.  This method is no longer used.")]
        public static string CacheKey(int TabModuleID)
        {
            string strCacheKey = "TabModule:";
            strCacheKey += TabModuleID + ":";
            strCacheKey += Thread.CurrentThread.CurrentUICulture.ToString();
            return strCacheKey;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.0.  Replaced by CopyModule(ModuleInfo, TabInfo, String, Boolean)")]
        public void CopyModule(int moduleId, int fromTabId, List<TabInfo> toTabs, bool includeSettings)
        {
            ModuleInfo objModule = GetModule(moduleId, fromTabId, false);
            //Iterate through collection copying the module to each Tab (except the source)
            foreach (TabInfo objTab in toTabs)
            {
                if (objTab.TabID != fromTabId)
                {
                    CopyModule(objModule, objTab, "", includeSettings);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.5.  Replaced by CopyModule(ModuleInfo, TabInfo, String, Boolean)")]
        public void CopyModule(int moduleId, int fromTabId, int toTabId, string toPaneName, bool includeSettings)
        {
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            ModuleInfo objModule = GetModule(moduleId, fromTabId, false);
            TabInfo objTab = TabController.Instance.GetTab(toTabId, _portalSettings.PortalId, false);
            CopyModule(objModule, objTab, toPaneName, includeSettings);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Replaced in DotNetNuke 5.0 by CopyModule(Integer, integer, List(Of TabInfo), Boolean)")]
        public void CopyModule(int moduleId, int fromTabId, ArrayList toTabs, bool includeSettings)
        {
            ModuleInfo objModule = GetModule(moduleId, fromTabId, false);
            //Iterate through collection copying the module to each Tab (except the source)
            foreach (TabInfo objTab in toTabs)
            {
                if (objTab.TabID != fromTabId)
                {
                    CopyModule(objModule, objTab, "", includeSettings);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3.  No longer neccessary.")]
        public void CopyTabModuleSettings(ModuleInfo fromModule, ModuleInfo toModule)
        {
            CopyTabModuleSettingsInternal(fromModule, toModule);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3.  Use an alternate overload")]
        public void DeleteAllModules(int moduleId, int tabId, List<TabInfo> fromTabs)
        {
            DeleteAllModules(moduleId, tabId, fromTabs, true, false, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1.  Use an alternate overload")]
        public void DeleteAllModules(int moduleId, int tabId, List<TabInfo> fromTabs, bool includeCurrent, bool deleteBaseModule)
        {
            DeleteAllModules(moduleId, tabId, fromTabs, true, includeCurrent, deleteBaseModule);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1.  Use an alternate overload")]
        public void DeleteAllModules(int moduleId, int tabId, ArrayList fromTabs, bool includeCurrent, bool deleteBaseModule)
        {
            var listTabs = fromTabs.Cast<TabInfo>().ToList();
            DeleteAllModules(moduleId, tabId, listTabs, true, includeCurrent, deleteBaseModule);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3.")]
        public void DeleteModuleSettings(int moduleId)
        {
            dataProvider.DeleteModuleSettings(moduleId);
            var log = new LogInfo {LogTypeKey = EventLogController.EventLogType.MODULE_SETTING_DELETED.ToString()};
            log.LogProperties.Add(new LogDetailInfo("ModuleId", moduleId.ToString()));
            LogController.Instance.AddLog(log);
            UpdateTabModuleVersionsByModuleID(moduleId);
            ClearModuleSettingsCache(moduleId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. Replaced by DeleteTabModule(Integer, integer, boolean)")]
        public void DeleteTabModule(int tabId, int moduleId)
        {
            DeleteTabModule(tabId, moduleId, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3.")]
        public void DeleteTabModuleSettings(int tabModuleId)
        {
            dataProvider.DeleteTabModuleSettings(tabModuleId);
            UpdateTabModuleVersion(tabModuleId);
            EventLogController.Instance.AddLog("TabModuleID",
                               tabModuleId.ToString(),
                               PortalController.Instance.GetCurrentPortalSettings(),
                               UserController.Instance.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.TABMODULE_SETTING_DELETED);
            ClearTabModuleSettingsCache(tabModuleId, null);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3.  Please use the ModuleSettings property of the ModuleInfo object")]
        public Hashtable GetModuleSettings(int ModuleId)
        {
            var settings = new Hashtable();
            IDataReader dr = null;
            try
            {
                dr = dataProvider.GetModuleSettings(ModuleId);
                while (dr.Read())
                {
                    if (!dr.IsDBNull(1))
                    {
                        settings[dr.GetString(0)] = dr.GetString(1);
                    }
                    else
                    {
                        settings[dr.GetString(0)] = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            finally
            {
                //Ensure DataReader is closed
                CBO.CloseDataReader(dr, true);
            }
            return settings;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Replaced in DotNetNuke 5.0 by GetTabModules(Integer)")]
        public ArrayList GetPortalTabModules(int portalID, int tabID)
        {
            var arr = new ArrayList();
            foreach (KeyValuePair<int, ModuleInfo> kvp in GetTabModules(tabID))
            {
                arr.Add(kvp.Value);
            }
            return arr;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Replaced in DotNetNuke 5.0 by GetModules(Integer)")]
        public ArrayList GetModules(int portalID, bool includePermissions)
        {
            return CBO.FillCollection(dataProvider.GetModules(portalID), typeof(ModuleInfo));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Replaced by GetTabModulesByModule(moduleID)")]
        public ArrayList GetModuleTabs(int moduleID)
        {
            return new ArrayList(GetTabModulesByModule(moduleID).ToArray());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Replaced by GetModules(portalId)")]
        public ArrayList GetRecycleModules(int portalID)
        {
            return GetModules(portalID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3.  Please use the TabModuleSettings property of the ModuleInfo object")]
        public Hashtable GetTabModuleSettings(int TabModuleId)
        {
            var settings = new Hashtable();
            IDataReader dr = null;
            try
            {
                dr = dataProvider.GetTabModuleSettings(TabModuleId);
                while (dr.Read())
                {
                    if (!dr.IsDBNull(1))
                    {
                        settings[dr.GetString(0)] = dr.GetString(1);
                    }
                    else
                    {
                        settings[dr.GetString(0)] = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
            return settings;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Replaced in DotNetNuke 5.0 by UpdateTabModuleOrder(Integer)")]
        public void UpdateTabModuleOrder(int tabId, int portalId)
        {
            UpdateTabModuleOrder(tabId);
        }
    }
}
