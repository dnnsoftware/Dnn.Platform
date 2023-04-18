// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <content>The deprecated methods for ModuleController.</content>
    public partial class ModuleController
    {
        /// <summary>Deserializes the module.</summary>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="module">ModuleInfo of current module.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public static void DeserializeModule(
            XmlNode nodeModule,
            ModuleInfo module,
            int portalId,
            int tabId)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            DeserializeModule(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), nodeModule, module, portalId, tabId);
        }

        /// <summary>Deserializes the module.</summary>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="nodePane">The node pane.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <param name="hModules">The modules.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public static void DeserializeModule(
            XmlNode nodeModule,
            XmlNode nodePane,
            int portalId,
            int tabId,
            PortalTemplateModuleAction mergeTabs,
            Hashtable hModules)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            DeserializeModule(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), nodeModule, nodePane, portalId, tabId, mergeTabs, hModules);
        }

        /// <summary>Serializes the metadata of a module (and optionally its contents) to an XML node.</summary>
        /// <param name="xmlModule">The XML Document to use for the Module.</param>
        /// <param name="module">The ModuleInfo object to serialize.</param>
        /// <param name="includeContent">A flag that determines whether the content of the module is serialized.</param>
        /// <returns>An <see cref="XmlNode"/> representing the module.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public static XmlNode SerializeModule(XmlDocument xmlModule, ModuleInfo module, bool includeContent)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            return SerializeModule(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), xmlModule, module, includeContent);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. No longer necessary. Scheduled removal in v10.0.0.")]
        public void CopyTabModuleSettings(ModuleInfo fromModule, ModuleInfo toModule)
        {
            this.CopyTabModuleSettingsInternal(fromModule, toModule);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use an alternate overload. Scheduled removal in v10.0.0.")]
        public void DeleteAllModules(int moduleId, int tabId, List<TabInfo> fromTabs)
        {
            this.DeleteAllModules(moduleId, tabId, fromTabs, true, false, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Scheduled removal in v10.0.0.")]
        public void DeleteModuleSettings(int moduleId)
        {
            DataProvider.DeleteModuleSettings(moduleId);
            var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.MODULE_SETTING_DELETED.ToString() };
            log.LogProperties.Add(new LogDetailInfo("ModuleId", moduleId.ToString()));
            LogController.Instance.AddLog(log);
            this.UpdateTabModuleVersionsByModuleID(moduleId);
            ClearModuleSettingsCache(moduleId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Scheduled removal in v10.0.0.")]
        public void DeleteTabModuleSettings(int tabModuleId)
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
        [Obsolete("Deprecated in DNN 7.3. Please use the ModuleSettings property of the ModuleInfo object. Scheduled removal in v10.0.0.")]
        public Hashtable GetModuleSettings(int moduleId)
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
        [Obsolete("Deprecated in DNN 7.3. Replaced by GetTabModulesByModule(moduleID). Scheduled removal in v10.0.0.")]
        public ArrayList GetModuleTabs(int moduleID)
        {
            return new ArrayList(this.GetTabModulesByModule(moduleID).ToArray());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Replaced by GetModules(portalId). Scheduled removal in v10.0.0.")]
        public ArrayList GetRecycleModules(int portalID)
        {
            return this.GetModules(portalID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Please use the TabModuleSettings property of the ModuleInfo object. Scheduled removal in v10.0.0.")]
        public Hashtable GetTabModuleSettings(int tabModuleId)
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
}
