// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;

public partial class ModuleController
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "No longer necessary", RemovalVersion = 10)]
    public partial void CopyTabModuleSettings(ModuleInfo fromModule, ModuleInfo toModule)
    {
        this.CopyTabModuleSettingsInternal(fromModule, toModule);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use an alternate overload", RemovalVersion = 10)]
    public partial void DeleteAllModules(int moduleId, int tabId, List<TabInfo> fromTabs)
    {
        this.DeleteAllModules(moduleId, tabId, fromTabs, true, false, false);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "No replacement", RemovalVersion = 10)]
    public partial void DeleteModuleSettings(int moduleId)
    {
        DataProvider.DeleteModuleSettings(moduleId);
        var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.MODULE_SETTING_DELETED.ToString() };
        log.LogProperties.Add(new LogDetailInfo("ModuleId", moduleId.ToString()));
        LogController.Instance.AddLog(log);
        this.UpdateTabModuleVersionsByModuleID(moduleId);
        ClearModuleSettingsCache(moduleId);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "No replacement", RemovalVersion = 10)]
    public partial void DeleteTabModuleSettings(int tabModuleId)
    {
        DataProvider.DeleteTabModuleSettings(tabModuleId);
        UpdateTabModuleVersion(tabModuleId);
        EventLogController.Instance.AddLog(
            "TabModuleID",
            tabModuleId.ToString(),
            PortalController.Instance.GetCurrentPortalSettings(),
            UserController.Instance.GetCurrentUserInfo().UserID,
            EventLogController.EventLogType.TABMODULE_SETTING_DELETED);
        ClearTabModuleSettingsCache(tabModuleId, null);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Please use the ModuleSettings property of the ModuleInfo object", RemovalVersion = 10)]
    public partial Hashtable GetModuleSettings(int moduleId)
    {
        var settings = new Hashtable();
        IDataReader dr = null;
        try
        {
            dr = DataProvider.GetModuleSettings(moduleId);
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
            // Ensure DataReader is closed
            CBO.CloseDataReader(dr, true);
        }

        return settings;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Replaced by GetTabModulesByModule(moduleID)", RemovalVersion = 10)]
    public partial ArrayList GetModuleTabs(int moduleID)
    {
        return new ArrayList(this.GetTabModulesByModule(moduleID).ToArray());
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Replaced by GetModules(portalId)", RemovalVersion = 10)]
    public partial ArrayList GetRecycleModules(int portalID)
    {
        return this.GetModules(portalID);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Please use the TabModuleSettings property of the ModuleInfo object", RemovalVersion = 10)]
    public partial Hashtable GetTabModuleSettings(int tabModuleId)
    {
        var settings = new Hashtable();
        IDataReader dr = null;
        try
        {
            dr = DataProvider.GetTabModuleSettings(tabModuleId);
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
