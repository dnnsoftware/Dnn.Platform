#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
        [Obsolete("Deprecated in DotNetNuke 7.3. No longer neccessary. Scheduled removal in v10.0.0.")]
        public void CopyTabModuleSettings(ModuleInfo fromModule, ModuleInfo toModule)
        {
            CopyTabModuleSettingsInternal(fromModule, toModule);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use an alternate overload. Scheduled removal in v10.0.0.")]
        public void DeleteAllModules(int moduleId, int tabId, List<TabInfo> fromTabs)
        {
            DeleteAllModules(moduleId, tabId, fromTabs, true, false, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Scheduled removal in v10.0.0.")]
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
        [Obsolete("Deprecated in DNN 7.3. Scheduled removal in v10.0.0.")]
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
        [Obsolete("Deprecated in DNN 7.3. Please use the ModuleSettings property of the ModuleInfo object. Scheduled removal in v10.0.0.")]
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
        [Obsolete("Deprecated in DNN 7.3. Replaced by GetTabModulesByModule(moduleID). Scheduled removal in v10.0.0.")]
        public ArrayList GetModuleTabs(int moduleID)
        {
            return new ArrayList(GetTabModulesByModule(moduleID).ToArray());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Replaced by GetModules(portalId). Scheduled removal in v10.0.0.")]
        public ArrayList GetRecycleModules(int portalID)
        {
            return GetModules(portalID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Please use the TabModuleSettings property of the ModuleInfo object. Scheduled removal in v10.0.0.")]
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
    }
}
