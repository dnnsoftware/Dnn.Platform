// // DotNetNuke® - http://www.dotnetnuke.com
// // Copyright (c) 2002-2014
// // by DotNetNuke Corporation
// // 
// // Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// // documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// // the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// // to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// // 
// // The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// // of the Software.
// // 
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// // TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// // THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// // CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// // DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;

namespace DotNetNuke.Entities.Tabs.Internal
{
    internal class TabControllerImpl : ITabController
    {
        readonly TabController _legacyController = new TabController();

        private object GetAliasSkinsCallback(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            IDataReader dr = DataProvider.Instance().GetTabAliasSkins(portalID);
            var dic = new Dictionary<int, List<TabAliasSkinInfo>>();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    var tabAliasSkin = CBO.FillObject<TabAliasSkinInfo>(dr, false);

                    //add Tab Alias Skin to dictionary
                    if (dic.ContainsKey(tabAliasSkin.TabId))
                    {
                        //Add Tab Alias Skin to Tab Alias Skin Collection already in dictionary for TabId
                        dic[tabAliasSkin.TabId].Add(tabAliasSkin);
                    }
                    else
                    {
                        //Create new Tab Alias Skin Collection for TabId
                        var collection = new List<TabAliasSkinInfo> { tabAliasSkin };

                        //Add Collection to Dictionary
                        dic.Add(tabAliasSkin.TabId, collection);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                //close datareader
                CBO.CloseDataReader(dr, true);
            }
            return dic;
        }

        private object GetCustomAliasesCallback(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            IDataReader dr = DataProvider.Instance().GetTabCustomAliases(portalID);
            var dic = new Dictionary<int, Dictionary<string, string>>();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    var tabId = (int)dr["TabId"];
                    var customAlias = (string)dr["httpAlias"];
                    var cultureCode = (string)dr["cultureCode"];

                    //add Custom Alias to dictionary
                    if (dic.ContainsKey(tabId))
                    {
                        //Add Custom Alias to Custom Alias Collection already in dictionary for TabId
                        dic[tabId][cultureCode] = customAlias;
                    }
                    else
                    {
                        //Create new Custom Alias Collection for TabId
                        var collection = new Dictionary<string, string> {{cultureCode, customAlias}};

                        //Add Collection to Dictionary
                        dic.Add(tabId, collection);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                //close datareader
                CBO.CloseDataReader(dr, true);
            }
            return dic;
        }

        private object GetTabUrlsCallback(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            IDataReader dr = DataProvider.Instance().GetTabUrls(portalID);
            var dic = new Dictionary<int, List<TabUrlInfo>>();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    var tabRedirect = CBO.FillObject<TabUrlInfo>(dr, false);

                    //add Tab Redirect to dictionary
                    if (dic.ContainsKey(tabRedirect.TabId))
                    {
                        //Add Tab Redirect to Tab Redirect Collection already in dictionary for TabId
                        dic[tabRedirect.TabId].Add(tabRedirect);
                    }
                    else
                    {
                        //Create new Tab Redirect Collection for TabId
                        var collection = new List<TabUrlInfo> { tabRedirect };

                        //Add Collection to Dictionary
                        dic.Add(tabRedirect.TabId, collection);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                //close datareader
                CBO.CloseDataReader(dr, true);
            }
            return dic;
        }

        private Dictionary<int, List<TabAliasSkinInfo>> GetAliasSkins(int portalId)
        {
            string cacheKey = string.Format(DataCache.TabAliasSkinCacheKey, portalId);
            return CBO.GetCachedObject<Dictionary<int, List<TabAliasSkinInfo>>>(new CacheItemArgs(cacheKey,
                                                                                                DataCache.TabAliasSkinCacheTimeOut,
                                                                                                DataCache.TabAliasSkinCachePriority, 
                                                                                                portalId),
                                                                                        GetAliasSkinsCallback);
           
        }

        private Dictionary<int, Dictionary<string, string>> GetCustomAliases(int portalId)
        {
            string cacheKey = string.Format(DataCache.TabCustomAliasCacheKey, portalId);
            return CBO.GetCachedObject<Dictionary<int, Dictionary<string, string>>>(new CacheItemArgs(cacheKey,
                                                                                                DataCache.TabCustomAliasCacheTimeOut,
                                                                                                DataCache.TabCustomAliasCachePriority,
                                                                                                portalId),
                                                                                        GetCustomAliasesCallback);

        }

        internal Dictionary<int, List<TabUrlInfo>> GetTabUrls(int portalId)
        {
            string cacheKey = string.Format(DataCache.TabUrlCacheKey, portalId);
            return CBO.GetCachedObject<Dictionary<int, List<TabUrlInfo>>>(new CacheItemArgs(cacheKey,
                                                                                                DataCache.TabUrlCacheTimeOut,
                                                                                                DataCache.TabUrlCachePriority,
                                                                                                portalId),
                                                                                        GetTabUrlsCallback);

        }

        public void DeleteTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache)
        {
            DataProvider.Instance().DeleteTabUrl(tabUrl.TabId, tabUrl.SeqNum);

            var objEventLog = new EventLogController();
            objEventLog.AddLog("tabUrl.TabId",
                               tabUrl.TabId.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.TABURL_DELETED);
            if (clearCache)
            {
                DataCache.RemoveCache(String.Format(DataCache.TabUrlCacheKey, portalId));
                CacheController.ClearCustomAliasesCache();
                var tab = GetTab(tabUrl.TabId, portalId);
                tab.ClearTabUrls();
            }
        }

        /// <summary>
        /// Gets the tab.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab info.</returns>
        public TabInfo GetTab(int tabId, int portalId)
        {
            return _legacyController.GetTab(tabId, portalId, false);
        }

        public Dictionary<string, string> GetCustomAliases(int tabId, int portalId)
        {
            //Get the Portal CustomAlias Dictionary
            Dictionary<int, Dictionary<string, string>> dicCustomAliases = GetCustomAliases(portalId);

            //Get the Collection from the Dictionary
            Dictionary<string, string> customAliases;
            bool bFound = dicCustomAliases.TryGetValue(tabId, out customAliases);
            if (!bFound)
            {
                //Return empty collection
                customAliases = new Dictionary<string, string>();
            }
            return customAliases;
        }

        public List<TabAliasSkinInfo> GetAliasSkins(int tabId, int portalId)
        {
            //Get the Portal AliasSkin Dictionary
            Dictionary<int, List<TabAliasSkinInfo>> dicTabAliases = GetAliasSkins(portalId);

            //Get the Collection from the Dictionary
            List<TabAliasSkinInfo> tabAliases;
            bool bFound = dicTabAliases.TryGetValue(tabId, out tabAliases);
            if (!bFound)
            {
                //Return empty collection
                tabAliases = new List<TabAliasSkinInfo>();
            }
            return tabAliases;
        }

        public List<TabUrlInfo> GetTabUrls(int tabId, int portalId)
        {
            //Get the Portal TabUrl Dictionary
            Dictionary<int, List<TabUrlInfo>> dicTabUrls = GetTabUrls(portalId);

            //Get the Collection from the Dictionary
            List<TabUrlInfo> tabRedirects;
            bool bFound = dicTabUrls.TryGetValue(tabId, out tabRedirects);
            if (!bFound)
            {
                //Return empty collection
                tabRedirects = new List<TabUrlInfo>();
            }
            return tabRedirects;
        }

        public void SaveTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache)
        {
            var portalAliasId = (tabUrl.PortalAliasUsage == PortalAliasUsageType.Default)
                                  ? Null.NullInteger
                                  : tabUrl.PortalAliasId;

            var saveLog = EventLogController.EventLogType.TABURL_CREATED;

            if (tabUrl.HttpStatus == "200")
            {
                saveLog = EventLogController.EventLogType.TABURL_CREATED;
               
            }
            else
            {
                //need to see if sequence number exists to decide if insert or update
                List<TabUrlInfo> t = GetTabUrls(portalId, tabUrl.TabId);
                var existingSeq = t.FirstOrDefault(r => r.SeqNum == tabUrl.SeqNum);
                if (existingSeq == null)
                {
                    saveLog = EventLogController.EventLogType.TABURL_CREATED;
                }
            }

            DataProvider.Instance().SaveTabUrl(tabUrl.TabId, tabUrl.SeqNum, portalAliasId, (int)tabUrl.PortalAliasUsage, tabUrl.Url, tabUrl.QueryString, tabUrl.CultureCode, tabUrl.HttpStatus, tabUrl.IsSystem,UserController.GetCurrentUserInfo().UserID);

            var objEventLog = new EventLogController();
            objEventLog.AddLog("tabUrl",
                               tabUrl.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               saveLog);

            if (clearCache)
            {
                DataCache.RemoveCache(String.Format(DataCache.TabUrlCacheKey, portalId));
                CacheController.ClearCustomAliasesCache();
                _legacyController.ClearCache(portalId);
                var tab = GetTab(tabUrl.TabId, portalId);                
                tab.ClearTabUrls();
            }
        }
    }
}