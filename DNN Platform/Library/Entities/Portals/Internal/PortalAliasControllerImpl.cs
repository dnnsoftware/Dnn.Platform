#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Log.EventLog;

namespace DotNetNuke.Entities.Portals.Internal
{
    internal class PortalAliasControllerImpl : IPortalAliasController
    {
        private readonly DataProvider _dataProvider = DataProvider.Instance();

        #region Private Methods

        private static void ClearCache(bool refreshServiceRoutes)
        {
            DataCache.RemoveCache(DataCache.PortalAliasCacheKey);
            CacheController.FlushPageIndexFromCache();
            if (refreshServiceRoutes)
            {
                ServicesRoutingManager.ReRegisterServiceRoutesWhileSiteIsRunning();
            }
        }

        private static CacheItemArgs GetCacheArgs()
        {
            return new CacheItemArgs(DataCache.PortalAliasCacheKey, DataCache.PortalAliasCacheTimeOut, DataCache.PortalAliasCachePriority);
        }

        private static object GetPortalAliasesCallBack(CacheItemArgs cacheItemArgs)
        {
            var dic = CBO.FillDictionary<string, PortalAliasInfo>("HTTPAlias", DataProvider.Instance().GetPortalAliasByPortalID(-1));
            return dic.Keys.ToDictionary(key => key.ToLowerInvariant(), key => dic[key]);
        }

        private static void LogEvent(PortalAliasInfo portalAlias, EventLogController.EventLogType logType)
        {
            int userId = UserController.GetCurrentUserInfo().UserID;
            var eventLogController = new EventLogController();
            eventLogController.AddLog(portalAlias, PortalController.GetCurrentPortalSettings(), userId, "", logType);
        }

        #endregion

        #region IPortalAliasController Implementation

        public int AddPortalAlias(PortalAliasInfo portalAlias)
        {
            //Add Alias
            int Id = _dataProvider.AddPortalAlias(portalAlias.PortalID, 
                                            portalAlias.HTTPAlias.ToLower().Trim('/'),
                                            portalAlias.CultureCode,
                                            portalAlias.Skin,
                                            portalAlias.BrowserType.ToString(),
                                            portalAlias.IsPrimary,
                                            UserController.GetCurrentUserInfo().UserID);

            //Log Event
            LogEvent(portalAlias, EventLogController.EventLogType.PORTALALIAS_CREATED);

            //clear portal alias cache
            ClearCache(true);

            return Id;
        }

        public void DeletePortalAlias(PortalAliasInfo portalAlias)
        {
            //Delete Alias
            DataProvider.Instance().DeletePortalAlias(portalAlias.PortalAliasID);

            //Log Event
            LogEvent(portalAlias, EventLogController.EventLogType.PORTALALIAS_DELETED);

            //clear portal alias cache
            ClearCache(false);
        }

        public IEnumerable<PortalAliasInfo> GetPortalAliasesByPortalId(int portalId)
        {
            return GetPortalAliases().Values.Where(alias => alias.PortalID == portalId).ToList();
        }

        public IDictionary<string, PortalAliasInfo> GetPortalAliases()
        {
            return CBO.GetCachedObject<Dictionary<string, PortalAliasInfo>>(GetCacheArgs(), GetPortalAliasesCallBack, true);
        }

        public PortalAliasInfo GetPortalAlias(string alias)
        {
            return PortalAliasController.GetPortalAliasInfo(alias);
        }

        public void UpdatePortalAlias(PortalAliasInfo portalAlias)
        {
            //Update Alias
            DataProvider.Instance().UpdatePortalAliasInfo(portalAlias.PortalAliasID,
                                                            portalAlias.PortalID,
                                                            portalAlias.HTTPAlias.ToLower().Trim('/'),
                                                            portalAlias.CultureCode,
                                                            portalAlias.Skin,
                                                            portalAlias.BrowserType.ToString(),
                                                            portalAlias.IsPrimary,
                                                            UserController.GetCurrentUserInfo().UserID);
            //Log Event
            LogEvent(portalAlias, EventLogController.EventLogType.PORTALALIAS_UPDATED);

            //clear portal alias cache
            ClearCache(false);
        }

        #endregion
    }
}