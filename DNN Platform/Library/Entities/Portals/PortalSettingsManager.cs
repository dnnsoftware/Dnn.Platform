// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Settings;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;

    /// <inheritdoc />
    public class PortalSettingsManager : IPortalSettingsManager
    {
        /// <inheritdoc />
        public ISettingsService GetPortalSettings(int portalId)
        {
            return this.GetPortalSettings(portalId, string.Empty);
        }

        /// <inheritdoc />
        public ISettingsService GetPortalSettings(int portalId, string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode) && portalId > -1)
            {
                cultureCode = PortalController.GetActivePortalLanguage(portalId);
            }

            var cachedSettings = this.GetCachedSettings(portalId, cultureCode);
            var saveService = new PortalSaveSettingsService(portalId, cultureCode);
            var deleteService = new PortalDeleteSettingsService(portalId, cultureCode);

            return new PortalSettingsService(() => this.GetCachedSettings(portalId, cultureCode), saveService, deleteService);
        }

        private IDictionary<string, string> GetCachedSettings(int portalId, string cultureCode)
        {
            var cacheKey = string.Format(DataCache.PortalSettingsCacheKey, portalId, cultureCode);
            var cacheItemArguments = new CacheItemArgs(
                cacheKey,
                DataCache.PortalSettingsCacheTimeOut,
                DataCache.PortalSettingsCachePriority,
                portalId,
                cultureCode);

            return CBO
                .GetCachedObject<Dictionary<string, string>>(
                    cacheItemArguments,
                    this.CacheExpiredCallback,
                    true);
        }

        private object CacheExpiredCallback(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var dicSettings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (portalId <= -1)
            {
                return dicSettings;
            }

            var cultureCode = Convert.ToString(cacheItemArgs.ParamList[1]);
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = PortalController.GetActivePortalLanguage(portalId);
            }

            var dr = DataProvider.Instance().GetPortalSettings(portalId, cultureCode);
            try
            {
                while (dr.Read())
                {
                    if (dr.IsDBNull(1))
                    {
                        continue;
                    }

                    var key = dr.GetString(0);
                    if (dicSettings.ContainsKey(key))
                    {
                        dicSettings[key] = dr.GetString(1);
                        var log = new LogInfo
                        {
                            LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString(),
                        };
                        log.AddProperty("Duplicate PortalSettings Key", key);
                        LogController.Instance.AddLog(log);
                    }
                    else
                    {
                        dicSettings.Add(key, dr.GetString(1));
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return dicSettings;
        }
    }
}
