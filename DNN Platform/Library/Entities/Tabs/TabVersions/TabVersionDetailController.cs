﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    public class TabVersionDetailController: ServiceLocator<ITabVersionDetailController, TabVersionDetailController>, ITabVersionDetailController
    {
        private static readonly DataProvider Provider = DataProvider.Instance();

        #region Public Methods
        public TabVersionDetail GetTabVersionDetail(int tabVersionDetailId, int tabVersionId, bool ignoreCache = false)
        {
            return GetTabVersionDetails(tabVersionId, ignoreCache).SingleOrDefault(tvd => tvd.TabVersionDetailId == tabVersionDetailId);
        }
        
        public IEnumerable<TabVersionDetail> GetTabVersionDetails(int tabVersionId, bool ignoreCache = false)
        {
            //if we are not using the cache
            if (ignoreCache || Host.Host.PerformanceSetting == Globals.PerformanceSettings.NoCaching)
            {
                return CBO.FillCollection<TabVersionDetail>(Provider.GetTabVersionDetails(tabVersionId));
            }

            return CBO.GetCachedObject<List<TabVersionDetail>>(new CacheItemArgs(GetTabVersionDetailCacheKey(tabVersionId),
                                                                    DataCache.TabVersionDetailsCacheTimeOut,
                                                                    DataCache.TabVersionDetailsCachePriority),
                                                            c => CBO.FillCollection<TabVersionDetail>(Provider.GetTabVersionDetails(tabVersionId)));            
        }

        public IEnumerable<TabVersionDetail> GetVersionHistory(int tabId, int version)
        {
            return CBO.FillCollection<TabVersionDetail>(Provider.GetTabVersionDetailsHistory(tabId, version));
        }

        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail)
        {
            SaveTabVersionDetail(tabVersionDetail, tabVersionDetail.CreatedByUserID, tabVersionDetail.LastModifiedByUserID);            
        }

        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserID)
        {
            SaveTabVersionDetail(tabVersionDetail, createdByUserID, createdByUserID);
        }
        
        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserID, int modifiedByUserID)
        {
            tabVersionDetail.TabVersionDetailId = Provider.SaveTabVersionDetail(tabVersionDetail.TabVersionDetailId,
                tabVersionDetail.TabVersionId, tabVersionDetail.ModuleId, tabVersionDetail.ModuleVersion,
                tabVersionDetail.PaneName, tabVersionDetail.ModuleOrder, (int)tabVersionDetail.Action, createdByUserID,
                modifiedByUserID);
            ClearCache(tabVersionDetail.TabVersionId);
        }

        public void DeleteTabVersionDetail(int tabVersionId, int tabVersionDetailId)
        {
            Provider.DeleteTabVersionDetail(tabVersionDetailId);
            ClearCache(tabVersionId);
        }

        public void ClearCache(int tabVersionId)
        {
            DataCache.RemoveCache(GetTabVersionDetailCacheKey(tabVersionId));
        }
        #endregion

        #region Private Methods
        private static string GetTabVersionDetailCacheKey(int tabVersionId)
        {
            return string.Format(DataCache.TabVersionDetailsCacheKey, tabVersionId);
        }

        #endregion

        protected override System.Func<ITabVersionDetailController> GetFactory()
        {
            return () => new TabVersionDetailController();
        }
    }
}
