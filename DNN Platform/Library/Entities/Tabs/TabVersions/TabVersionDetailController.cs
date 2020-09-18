// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    public class TabVersionDetailController : ServiceLocator<ITabVersionDetailController, TabVersionDetailController>, ITabVersionDetailController
    {
        private static readonly DataProvider Provider = DataProvider.Instance();

        public TabVersionDetail GetTabVersionDetail(int tabVersionDetailId, int tabVersionId, bool ignoreCache = false)
        {
            return this.GetTabVersionDetails(tabVersionId, ignoreCache).SingleOrDefault(tvd => tvd.TabVersionDetailId == tabVersionDetailId);
        }

        public IEnumerable<TabVersionDetail> GetTabVersionDetails(int tabVersionId, bool ignoreCache = false)
        {
            // if we are not using the cache
            if (ignoreCache || Host.Host.PerformanceSetting == Globals.PerformanceSettings.NoCaching)
            {
                return CBO.FillCollection<TabVersionDetail>(Provider.GetTabVersionDetails(tabVersionId));
            }

            return CBO.GetCachedObject<List<TabVersionDetail>>(
                new CacheItemArgs(
                GetTabVersionDetailCacheKey(tabVersionId),
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
            this.SaveTabVersionDetail(tabVersionDetail, tabVersionDetail.CreatedByUserID, tabVersionDetail.LastModifiedByUserID);
        }

        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserID)
        {
            this.SaveTabVersionDetail(tabVersionDetail, createdByUserID, createdByUserID);
        }

        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserID, int modifiedByUserID)
        {
            tabVersionDetail.TabVersionDetailId = Provider.SaveTabVersionDetail(
                tabVersionDetail.TabVersionDetailId,
                tabVersionDetail.TabVersionId, tabVersionDetail.ModuleId, tabVersionDetail.ModuleVersion,
                tabVersionDetail.PaneName, tabVersionDetail.ModuleOrder, (int)tabVersionDetail.Action, createdByUserID,
                modifiedByUserID);
            this.ClearCache(tabVersionDetail.TabVersionId);
        }

        public void DeleteTabVersionDetail(int tabVersionId, int tabVersionDetailId)
        {
            Provider.DeleteTabVersionDetail(tabVersionDetailId);
            this.ClearCache(tabVersionId);
        }

        public void ClearCache(int tabVersionId)
        {
            DataCache.RemoveCache(GetTabVersionDetailCacheKey(tabVersionId));
        }

        protected override System.Func<ITabVersionDetailController> GetFactory()
        {
            return () => new TabVersionDetailController();
        }

        private static string GetTabVersionDetailCacheKey(int tabVersionId)
        {
            return string.Format(DataCache.TabVersionDetailsCacheKey, tabVersionId);
        }
    }
}
