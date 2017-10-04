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
