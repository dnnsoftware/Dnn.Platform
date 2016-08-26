#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    public class TabVersionController: ServiceLocator<ITabVersionController, TabVersionController>, ITabVersionController
    {
        private static readonly DataProvider Provider = DataProvider.Instance();

        #region Public Methods
        public TabVersion GetTabVersion(int tabVersionId, int tabId, bool ignoreCache = false)
        {
            return GetTabVersions(tabId, ignoreCache).SingleOrDefault(tv => tv.TabVersionId == tabVersionId);
        }
        
        public IEnumerable<TabVersion> GetTabVersions(int tabId, bool ignoreCache = false)
        {
            //if we are not using the cache, then remove from cacehh and re-add loaded items when eeded later
            var tabCacehKey = GetTabVersionsCacheKey(tabId);
            if (ignoreCache || Host.Host.PerformanceSetting == Globals.PerformanceSettings.NoCaching)
            {
                DataCache.RemoveCache(tabCacehKey);
            }
            
            return CBO.GetCachedObject<List<TabVersion>>(new CacheItemArgs(tabCacehKey,
                                                                    DataCache.TabVersionsCacheTimeOut,
                                                                    DataCache.TabVersionsCachePriority),
                                                            c => CBO.FillCollection<TabVersion>(Provider.GetTabVersions(tabId)));            
        }
        public void SaveTabVersion(TabVersion tabVersion)
        {
            SaveTabVersion(tabVersion, tabVersion.CreatedByUserID, tabVersion.LastModifiedByUserID);
        }

        public void SaveTabVersion(TabVersion tabVersion, int createdByUserID)
        {
            SaveTabVersion(tabVersion, createdByUserID, createdByUserID);
        }

        public void SaveTabVersion(TabVersion tabVersion, int createdByUserID, int modifiedByUserID)
        {
            tabVersion.TabVersionId = Provider.SaveTabVersion(tabVersion.TabVersionId, tabVersion.TabId, tabVersion.TimeStamp, tabVersion.Version, tabVersion.IsPublished, createdByUserID, modifiedByUserID);
            ClearCache(tabVersion.TabId);
        }

        public TabVersion CreateTabVersion(int tabId, int createdByUserID, bool isPublished = false)
        {
            var lastTabVersion = GetTabVersions(tabId).OrderByDescending(tv => tv.Version).FirstOrDefault();
            var newVersion = 1;

            if (lastTabVersion != null)
            {
                if (!lastTabVersion.IsPublished && !isPublished)
                {
                    throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionCannotBeCreated_UnpublishedVersionAlreadyExists", Localization.ExceptionsResourceFile)));
                }
                newVersion = lastTabVersion.Version + 1;
            }
            
            var tabVersionId = Provider.SaveTabVersion(0, tabId, DateTime.UtcNow, newVersion, isPublished, createdByUserID, createdByUserID);
            ClearCache(tabId);

            return GetTabVersion(tabVersionId, tabId);
        }

        public void DeleteTabVersion(int tabId, int tabVersionId)
        {
            Provider.DeleteTabVersion(tabVersionId);
            ClearCache(tabId);
        }

        public void DeleteTabVersionDetailByModule(int moduleId)
        {
            Provider.DeleteTabVersionDetailByModule(moduleId);
        }
        #endregion

        #region Private Methods
        private void ClearCache(int tabId)
        {
            DataCache.RemoveCache(GetTabVersionsCacheKey(tabId));
        }

        private static string GetTabVersionsCacheKey(int tabId)
        {
            return string.Format(DataCache.TabVersionsCacheKey, tabId);
        }
        #endregion

        protected override Func<ITabVersionController> GetFactory()
        {
            return () => new TabVersionController();
        }
    }
}
