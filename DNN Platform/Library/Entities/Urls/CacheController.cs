#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Web.Caching;

using DotNetNuke.Collections.Internal;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Urls
{
    public class CacheController
    {
        private static CacheItemRemovedReason cacheItemRemovedReason;
        private static bool LogRemovedReason;
        private CacheItemRemovedCallback onRemovePageIndex;

        #region Cache Keys

        private const string PageIndexKey = "url_PageIndex";
        private const string PageIndexDepthKey = "url_PageIndexDepth";
        private const string UrlDictKey = "url_UrlDict";
        private const string UrlPortalsKey = "url_UrlPortals";
        private const string CustomAliasTabsKey = "url_CustomAliasTabsKey";
        private const string PortalAliasListKey = "url_PortalAliasList";
        private const string PortalsKey = "url_Portals";
        private const string CustomPortalAliasesKey = "url_CustomAliasList";
        private const string FriendlyUrlSettingsKey = "url_FriendlyUrlSettings";
        private const string RedirectActionsKey = "url_ParameterRedirectActions_{0}";
        private const string RewriteActionsKey = "url_ParameterRewriteActions_{0}";
        private const string PortalAliasRegexesKey = "url_PortalAliasRegex";
        private const string UserProfileActionsKey = "url_UserProfileActions";
        private const string PortalModuleProvidersForTabKey = "url_ModuleProvidersForTab_{0}_{1}";
        private const string PortalModuleProvidersAllTabsKey = "url_ModuleProvidersAllTabs_{0}";
        private const string PortalModuleProviderTabsKey = "url_PortalModuleProviderTabs_{0}";
        private const string AlwaysCallProviderTabsKey = "url_AlwaysCallProviderTabs_{0}";
        private const string HomePageSkinsKey = "url_HomePageSkins_{0}";
        private const string TabPathsKey = "url_TabPathsKey_{0}";

        internal const string VanityUrlLookupKey = "url_VanityUrlLookup_{0}";

        #endregion

        #region Private Methods

        private static CacheDependency GetPortalsCacheDependency()
        {
            var keys = new List<string> {"DNN_PortalDictionary"};
            var portalsDepedency = new CacheDependency(null, keys.ToArray());
            return portalsDepedency;
        }

        private CacheDependency GetTabsCacheDependency(IEnumerable<int> portalIds)
        {
            var keys = new List<string>();
            foreach (int portalId in portalIds)
            {
                const string cacheKey = DataCache.TabCacheKey;
                string key = string.Format(cacheKey, portalId);
                key = "DNN_" + key; //add on the DNN_ prefix
                keys.Add(key);
            }
            //get the portals list dependency
            var portalKeys = new List<string>();
            if (portalKeys.Count > 0)
            {
                keys.AddRange(portalKeys);
            }
            var tabsDependency = new CacheDependency(null, keys.ToArray());
            return tabsDependency;
        }

        private static void SetPageCache(string key, object value, FriendlyUrlSettings settings)
        {
            SetPageCache(key, value, null, settings, null);
        }

        private static void SetPageCache(string key, object value, DNNCacheDependency dependency, FriendlyUrlSettings settings, CacheItemRemovedCallback callback)
        {
            DateTime absoluteExpiration = DateTime.Now.Add(settings.CacheTime);
            DataCache.SetCache(key,
                                value,
                                dependency,
                                absoluteExpiration,
                                Cache.NoSlidingExpiration,
                                CacheItemPriority.AboveNormal,
                                callback);
        }

        private static void SetPortalCache(string key, object value, FriendlyUrlSettings settings)
        {
            var absoluteExpiration = DateTime.Now.Add(new TimeSpan(24, 0, 0));
            if (settings != null)
            {
                absoluteExpiration = DateTime.Now.Add(settings.CacheTime);
            }
            //857 : use cache dependency for portal alias cache
            if (settings != null)
            {
                DataCache.SetCache(key, 
                                    value, 
                                    new DNNCacheDependency(GetPortalsCacheDependency()), 
                                    absoluteExpiration, 
                                    Cache.NoSlidingExpiration);
            }
            else
            {
                DataCache.SetCache(key, value, absoluteExpiration);
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Finds the best match friendlyurlparms.config file path
        /// </summary>
        /// <param name="portalId">The portalId to search for. -1 if all portals required.</param>
        /// <param name="portalSpecificFound"></param>
        /// <returns>If a non-zero length string, a valid file path.  If a zero length string, no file was found.</returns>
        /// <remarks>
        /// First priority is a file called n.friendlyurlparms.config, in the root path
        /// Next priority is a file called portals\n\friendlyurlparms.config, in the portal path
        /// Last priority is the friendlyurlparms.config file, in the root path
        /// Task no 807
        /// </remarks>
        internal static string FindFriendlyUrlParmsConfigFilePath(int portalId, out bool portalSpecificFound)
        {
            string result = "";
            portalSpecificFound = false;
            const string fileName = "friendlyUrlParms.config";
            string rootPath = Globals.ApplicationMapPath + "\\";

            string filePath;
            if (portalId > -1) //specific portal
            {
                //first look in the root folder with the portalid as a prefix
                filePath = rootPath + portalId.ToString() + "." + fileName;
            }
            else //no specific portal
            {
                filePath = rootPath + fileName; // just the pathname
            }
            if (File.Exists(filePath))
            {
                result = filePath;
                if (portalId > -1) //if there was a portal specified, this was a portal specific path
                {
                    portalSpecificFound = true;
                }
            }
            else
            {
                //no portal specific name in the root folder
                if (portalId > -1) // at this point, only interested if a specific portal is requested
                {
                    var pc = new PortalController();
                    PortalInfo portal = pc.GetPortal(portalId);
                    if (portal != null)
                    {
                        //looking for the file in the portal folder
                        string portalPath = portal.HomeDirectoryMapPath;
                        filePath = portalPath + fileName; // just the pathname
                        if (File.Exists(filePath))
                        {
                            result = filePath;
                            portalSpecificFound = true;
                        }
                    }
                }
                if (String.IsNullOrEmpty(result))
                {
                    //nothing matching found : now just looking for the root path
                    filePath = rootPath + fileName;
                    if (File.Exists(filePath))
                    {
                        result = filePath;
                        portalSpecificFound = false;
                    }
                }
            }
            return result;
        }

        internal static List<int> GetAlwaysCallProviderTabs(int portalId)
        {
            List<int> result = null;
            string key = string.Format(AlwaysCallProviderTabsKey, portalId);
            var tabIdsToAlwaysCall = (int[])DataCache.GetCache(key);
            if (tabIdsToAlwaysCall != null)
            {
                result = new List<int>(tabIdsToAlwaysCall);
            }
            return result;
        }

        //770 : customised portal alias per-tab
        internal static List<string> GetCustomAliasesFromCache()
        {
            object raw = DataCache.GetCache(CustomPortalAliasesKey);
            return (raw != null) ? (List<string>)raw : null;
        }

        /// <summary>
        /// Retrieve the Url Dictionary for the installation.
        /// </summary>
        /// <param name="urlDict"></param>
        /// <param name="urlPortals"></param>
        /// <param name="customAliasTabs"></param>
        /// <remarks>
        /// </remarks>
        internal void GetFriendlyUrlIndexFromCache(out SharedDictionary<int, SharedDictionary<string, string>> urlDict,
                                                   out List<int> urlPortals,
                                                   out SharedDictionary<string, string> customAliasTabs)
        {
            urlDict = null;
            urlPortals = null;
            customAliasTabs = null;
            object rawDict = DataCache.GetCache(UrlDictKey); //contains a dictionary of tabs for all portals
            object rawPortals = DataCache.GetCache(UrlPortalsKey);
            //contas a list of portals for which we have retrieved the tabs
            object rawCustomAliasTabs = DataCache.GetCache(CustomAliasTabsKey);
            //contains a dictionary of tabs with custom aliases, for all portals
            if (rawDict != null)
            {
                urlDict = (SharedDictionary<int, SharedDictionary<string, string>>)rawDict;
            }
            if (rawPortals != null)
            {
                urlPortals = (List<int>)rawPortals;
            }
            if (rawCustomAliasTabs != null)
            {
                customAliasTabs = (SharedDictionary<string, string>)rawCustomAliasTabs;
            }
        }

        internal static Hashtable GetHomePageSkinsFromCache(int portalId)
        {
            string key = string.Format(HomePageSkinsKey, portalId);
            var result = (Hashtable)DataCache.GetCache(key);
            return result;
        }

        internal static List<int> GetListOfTabsWithProviders(int portalId, FriendlyUrlSettings settings)
        {
            List<int> result = null;
            string key = string.Format(PortalModuleProviderTabsKey, portalId);
            var tabIdsForPortal = (int[])DataCache.GetCache(key);
            if (tabIdsForPortal != null)
            {
                result = new List<int>(tabIdsForPortal);
            }
            return result;
        }

        internal void GetPageIndexFromCache(out SharedDictionary<string, string> dict,
                                            out SharedDictionary<int, PathSizes> portalDepthInfo,
                                            FriendlyUrlSettings settings)
        {
            object raw = DataCache.GetCache(PageIndexKey);
            if (raw != null)
            {
                dict = (SharedDictionary<string, string>)raw;
                raw = DataCache.GetCache(PageIndexDepthKey);
                portalDepthInfo = (SharedDictionary<int, PathSizes>)raw;
            }
            else
            {
                dict = null;
                portalDepthInfo = null;
            }
        }

        internal static Dictionary<int, List<ParameterRedirectAction>> GetParameterRedirects(FriendlyUrlSettings settings, int portalId, ref List<string> messages)
        {
            string redirectActionKey = string.Format(RedirectActionsKey, portalId); //cached one portal at a time
            if (messages == null)
            {
                messages = new List<string>();
            }
            var redirectActions = (Dictionary<int, List<ParameterRedirectAction>>)DataCache.GetCache(redirectActionKey);
            if (redirectActions == null)
            {
                try
                {
                    redirectActions = new Dictionary<int, List<ParameterRedirectAction>>();
                    //807 : look for portal specific files
                    bool portalSpecific;
                    string fileName = FindFriendlyUrlParmsConfigFilePath(portalId, out portalSpecific);
                    if (fileName != "")
                    {
                        redirectActions.LoadFromXmlFile(fileName, portalId, portalSpecific, ref messages);
                    }
                    CacheDependency fileDependency = null;
                    if (File.Exists(fileName))
                    {
                        fileDependency = new CacheDependency(fileName);
                    }

                    if (settings != null)
                    {
                        DateTime absoluteExpiration = DateTime.Now.Add(settings.CacheTime);
                        DataCache.SetCache(redirectActionKey, redirectActions, new DNNCacheDependency(fileDependency), absoluteExpiration, Cache.NoSlidingExpiration);
                    }
                    else
                    {
                        DataCache.SetCache(redirectActionKey, redirectActions, new DNNCacheDependency(fileDependency));
                    }
                }
                catch (Exception ex)
                {
                    //log the exception
                    Services.Exceptions.Exceptions.LogException(ex);
                    messages.Add("Exception: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
            return redirectActions;
        }

        internal static Dictionary<int, List<ParameterReplaceAction>> GetParameterReplacements(FriendlyUrlSettings settings, int portalId, ref List<string> messages)
        {
            string replaceActionKey = "replaceActions:" + portalId.ToString();
            var replaceActions = (Dictionary<int, List<ParameterReplaceAction>>)DataCache.GetCache(replaceActionKey);
            if (messages == null)
            {
                messages = new List<string>();
            }
            if (replaceActions == null)
            {
                replaceActions = new Dictionary<int, List<ParameterReplaceAction>>();
                try
                {
                    bool portalSpecific;
                    string fileName = FindFriendlyUrlParmsConfigFilePath(portalId, out portalSpecific);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        replaceActions.LoadFromXmlFile(fileName, portalId, portalSpecific, ref messages);
                    }
                    CacheDependency cacheDependency = null;
                    if (replaceActions.Count > 0)
                    {
                        //only set filename dependency if file exists
                        cacheDependency = new CacheDependency(fileName);
                    }

                    DateTime expiration = DateTime.MaxValue;
                    DataCache.SetCache(replaceActionKey, replaceActions, new DNNCacheDependency(cacheDependency), expiration, settings.CacheTime);
                }
                catch (Exception ex)
                {
                    Services.Exceptions.Exceptions.LogException(ex);
                    messages.Add("Exception: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
            return replaceActions;
        }

        internal static Dictionary<int, SharedList<ParameterRewriteAction>> GetParameterRewrites(int portalId, ref List<string> messages, Guid parentTraceId)
        {
            string rewriteActionKey = string.Format(RewriteActionsKey, portalId.ToString());
            if (messages == null)
            {
                messages = new List<string>();
            }
            var rewriteActions = (Dictionary<int, SharedList<ParameterRewriteAction>>)DataCache.GetCache(rewriteActionKey);
            if (rewriteActions == null)
            {
                try
                {
                    rewriteActions = new Dictionary<int, SharedList<ParameterRewriteAction>>();
                    bool portalSpecific;
                    //807 : new change to look for portal rule files in portal specific locations
                    string filename = FindFriendlyUrlParmsConfigFilePath(portalId, out portalSpecific);
                    if (!string.IsNullOrEmpty(filename))
                    {
                        rewriteActions.LoadFromXmlFile(filename, portalId, portalSpecific, ref messages);
                    }
                    CacheDependency fileDependency = null;
                    if (File.Exists(filename))
                    {
                        fileDependency = new CacheDependency(filename);
                    }

                    DataCache.SetCache(rewriteActionKey, rewriteActions, new DNNCacheDependency(fileDependency));
                }
                catch (Exception ex)
                {
                    Services.Exceptions.Exceptions.LogException(ex);
                    messages.Add("Exception: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
            return rewriteActions;
        }

        internal static OrderedDictionary GetPortalAliasesRegexesFromCache()
        {
            object raw = DataCache.GetCache(PortalAliasRegexesKey);
            return (raw != null) ? (OrderedDictionary)raw : null;
        }

        internal static List<ExtensionUrlProvider> GetProvidersForTabAndPortal(int tabId,
                                                        int portalId,
                                                        FriendlyUrlSettings settings,
                                                        out bool noSuchProvider,
                                                        Guid parentTraceId)
        {
            //get list of tabids in this portal that have providers
            noSuchProvider = false;
            List<ExtensionUrlProvider> allCachedProviders = null;

            List<int> tabsWithProviders = GetListOfTabsWithProviders(portalId, settings);
            bool checkThisTab = true, checkAllTabs = true; //going to check all tabs, unless find otherwise in the cache
            if (tabsWithProviders != null)
            {
                checkAllTabs = tabsWithProviders.Contains(RewriteController.AllTabsRewrite);

                checkThisTab = tabsWithProviders.Contains(tabId);

                if (!checkThisTab && !checkAllTabs)
                {
                    noSuchProvider = true; //we got the list of tabs, there is no provider for this tab
                }
            }

            if (checkAllTabs)
            {
                //the portal has an 'all tabs' provider in it
                string allTabsKey = string.Format(PortalModuleProvidersAllTabsKey, portalId);
                var cachedAllTabsProviders = (List<ExtensionUrlProvider>)DataCache.GetCache(allTabsKey);
                if (cachedAllTabsProviders != null)
                {
                    allCachedProviders = new List<ExtensionUrlProvider>();
                    allCachedProviders.AddRange(cachedAllTabsProviders);
                }
            }

            if (checkThisTab) //the specified tab
            {
                //tab exists, get the providers for this tab
                string key = string.Format(PortalModuleProvidersForTabKey, portalId, tabId);
                var cachedTabProviders = (List<ExtensionUrlProvider>)DataCache.GetCache(key);
                if (cachedTabProviders != null)
                {
                    if (allCachedProviders == null)
                    {
                        allCachedProviders = new List<ExtensionUrlProvider>();
                    }
                    allCachedProviders.AddRange(cachedTabProviders);
                }
            }
            return allCachedProviders;
        }

        internal static SharedDictionary<string, string> GetTabPathsFromCache(int portalId)
        {
            return (SharedDictionary<string, string>)DataCache.GetCache(string.Format(TabPathsKey, portalId));
        }

        internal static void StoreAlwaysCallProviderTabs(int portalId, List<int> alwaysCallTabids, FriendlyUrlSettings settings)
        {
            if (alwaysCallTabids != null)
            {
                SetPageCache(string.Format(AlwaysCallProviderTabsKey, portalId), alwaysCallTabids.ToArray(), settings);
            }
        }

        internal static void StoreCustomAliasesInCache(List<string> aliases, FriendlyUrlSettings settings)
        {
            DateTime absoluteExpiration = DateTime.Now.Add(new TimeSpan(24, 0, 0));
            if (settings != null)
            {
                absoluteExpiration = DateTime.Now.Add(settings.CacheTime);
            }

            DataCache.SetCache(CustomPortalAliasesKey, aliases, absoluteExpiration);
        }

        /// <summary>
        /// Store the Url Dictionary (all tab urls / tabids) for the installation
        /// </summary>
        /// <param name="urlDict"></param>
        /// <param name="urlPortals"></param>
        /// <param name="customAliasTabs"></param>
        /// <param name="settings"></param>
        /// <param name="reason"></param>
        /// <remarks>
        /// </remarks>
        internal void StoreFriendlyUrlIndexInCache(SharedDictionary<int, SharedDictionary<string, string>> urlDict,
                                                List<int> urlPortals,
                                                SharedDictionary<string, string> customAliasTabs,
                                                FriendlyUrlSettings settings,
                                                string reason)
        {
            if (settings.LogCacheMessages)
            {
                onRemovePageIndex = RemovedPageIndexCallBack;
            }
            else
            {
                onRemovePageIndex = null;
            }

            LogRemovedReason = settings.LogCacheMessages;

            SetPageCache(UrlDictKey, urlDict, new DNNCacheDependency(GetTabsCacheDependency(urlPortals)), settings, onRemovePageIndex);

            SetPageCache(UrlPortalsKey, urlPortals, settings);
            SetPageCache(CustomAliasTabsKey, customAliasTabs, settings);

            if (settings.LogCacheMessages)
            {
                var elc = new EventLogController();

                var logValue = new LogInfo { LogTypeKey = "HOST_ALERT" };
                logValue.AddProperty("Url Rewriting Caching Message", "Friendly Url Index built and Stored in Cache.");
                logValue.AddProperty("Build Reason", reason);
                logValue.AddProperty("Cache Key", UrlDictKey);
                using (urlDict.GetReadLock())
                {
                    logValue.AddProperty("Item Count", urlDict.Values.Count.ToString());
                }
                logValue.AddProperty("Thread Id", Thread.CurrentThread.ManagedThreadId.ToString());
                logValue.AddProperty("Item added to cache", "Url Portals object added to cache.  Key:" + UrlPortalsKey + "  Items: " + urlPortals.Count.ToString());
                using (customAliasTabs.GetReadLock())
                {
                    logValue.AddProperty("Item added to cache", "Custom Alias Tabs added to cache.  Key:" + CustomAliasTabsKey + " Items: " + customAliasTabs.Count.ToString());
                }
                elc.AddLog(logValue);
            }
        }

        internal static void StoreHomePageSkinsInCache(int portalId, Hashtable homePageSkins)
        {
            if (homePageSkins != null && homePageSkins.Count > 0)
            {
                DataCache.SetCache(string.Format(HomePageSkinsKey, portalId), homePageSkins);
            }
        }

        /// <summary>
        /// This method stores a list of tabIds for the specific portal in the cache
        /// This is used to lookup and see if there are any providers to load for a tab,
        /// without having to store individual tabid/portaldId provider lists for every tab
        /// If a tab doesn't appear on this cached list, then the cache isn't checked 
        /// for that particular tabid/portalId combination
        /// </summary>
        /// <param name="providers"></param>
        /// <param name="portalId"></param>
        /// <param name="settings"></param>
        internal static void StoreListOfTabsWithProviders(List<ExtensionUrlProvider> providers, int portalId, FriendlyUrlSettings settings)
        {
            //go through the list of providers and store all the tabs that they are configured for
            var providersWithTabs = new List<int>();
            var providersWithTabsStr = new List<string>();
            foreach (ExtensionUrlProvider provider in providers)
            {
                if (provider.ProviderConfig.AllTabs)
                {
                    if (providersWithTabs.Contains(RewriteController.AllTabsRewrite) == false)
                    {
                        providersWithTabs.Add(RewriteController.AllTabsRewrite);
                        providersWithTabsStr.Add("AllTabs");
                    }
                }
                if (provider.AlwaysUsesDnnPagePath(portalId) == false)
                {
                    if (providersWithTabs.Contains(RewriteController.SiteRootRewrite) == false)
                    {
                        providersWithTabs.Add(RewriteController.SiteRootRewrite);
                        providersWithTabsStr.Add("NoPath");
                    }
                }
                foreach (int providerTabId in provider.ProviderConfig.TabIds)
                {
                    if (providersWithTabs.Contains(providerTabId) == false)
                    {
                        providersWithTabs.Add(providerTabId);
                        providersWithTabsStr.Add(providerTabId.ToString());
                    }
                }
            }

            //store list as array in cache
            string key = string.Format(PortalModuleProviderTabsKey, portalId);
            SetPageCache(key, providersWithTabs.ToArray(), settings);
            if (settings.LogCacheMessages)
            {
                var elc = new EventLogController();
                var logValue = new LogInfo { LogTypeKey = "HOST_ALERT" };
                logValue.AddProperty("Url Rewriting Caching Message", "Portal Module Providers Tab List stored in cache");
                logValue.AddProperty("Cache Item Key", key);
                logValue.AddProperty("PortalId", portalId.ToString());
                logValue.AddProperty("Provider With Tabs", string.Join(",", providersWithTabsStr.ToArray()));
                logValue.AddProperty("Thread Id", Thread.CurrentThread.ManagedThreadId.ToString());
                elc.AddLog(logValue);
            }
        }

        internal static void StoreModuleProvidersForPortal(int portalId, FriendlyUrlSettings settings, List<ExtensionUrlProvider> providers)
        {
            //get the key for the portal module providers
            string allTabsKey = string.Format(PortalModuleProvidersAllTabsKey, portalId);
            //get the providers that are on all tabs
            var allTabsProviders = new List<ExtensionUrlProvider>();
            //holds all providers, indexed by tabId
            var tabsProviders = new Dictionary<int, List<ExtensionUrlProvider>>();
            var tabIdStr = new List<string>();
            int providerCount = 0;
            foreach (ExtensionUrlProvider provider in providers)
            {
                if (provider.ProviderConfig.AllTabs)
                {
                    allTabsProviders.Add(provider);
                }
                else
                {
                    foreach (int tabId in provider.ProviderConfig.TabIds)
                    {
                        List<ExtensionUrlProvider> thisTabProviders;
                        if (tabsProviders.ContainsKey(tabId))
                        {
                            thisTabProviders = tabsProviders[tabId];
                            thisTabProviders.Add(provider);
                            tabsProviders[tabId] = thisTabProviders; //assign back to position in tabs
                        }
                        else
                        {
                            thisTabProviders = new List<ExtensionUrlProvider> { provider };
                            tabsProviders.Add(tabId, thisTabProviders);
                            tabIdStr.Add(tabId.ToString());
                        }
                    }
                    providerCount++;
                }
                //store the list of providers where the provider might be called with no valid TabId, because
                //the provider allows for Urls with no DNN Page path, which means the TabId can't be identified
                //by the Url Rewriter.  This identifies the Provider as using a 'siteRootRewrite'
                if (provider.AlwaysUsesDnnPagePath(portalId) == false)
                {
                    List<ExtensionUrlProvider> noPathProviders;
                    //add this one 
                    if (tabsProviders.ContainsKey(RewriteController.SiteRootRewrite))
                    {
                        noPathProviders = tabsProviders[RewriteController.SiteRootRewrite];
                        noPathProviders.Add(provider);
                        tabsProviders[RewriteController.SiteRootRewrite] = noPathProviders;
                        //assign back to position in tabs
                    }
                    else
                    {
                        noPathProviders = new List<ExtensionUrlProvider> { provider };
                        tabsProviders.Add(RewriteController.SiteRootRewrite, noPathProviders);
                        tabIdStr.Add("NoPath");
                    }
                }
            }
            //now add the two collections to the cache
            if (allTabsProviders.Count > 0)
            {
                SetPageCache(allTabsKey, allTabsProviders, settings);
            }
            if (tabsProviders.Count > 0)
            {
                foreach (int tabId in tabsProviders.Keys)
                {
                    SetPageCache(string.Format(PortalModuleProvidersForTabKey, portalId, tabId), tabsProviders[tabId], settings);
                }
            }

            if (settings.LogCacheMessages)
            {
                var elc = new EventLogController();
                var logValue = new LogInfo { LogTypeKey = "HOST_ALERT" };
                logValue.AddProperty("Url Rewriting Caching Message", "Extension Url Providers stored in cache");
                logValue.AddProperty("PortalId/TabIds", portalId.ToString() + "/" + string.Join(",", tabIdStr.ToArray()));
                logValue.AddProperty("All Tabs Providers Count", allTabsProviders.Count.ToString());
                logValue.AddProperty("Portal Tabs Providers Count", providerCount.ToString());
                logValue.AddProperty("Thread Id", Thread.CurrentThread.ManagedThreadId.ToString());
                elc.AddLog(logValue);
            }
        }


        internal void StorePageIndexInCache(SharedDictionary<string, string> tabDictionary,
                                             SharedDictionary<int, PathSizes> portalDepthInfo,
                                             FriendlyUrlSettings settings,
                                             string reason)
        {
            onRemovePageIndex = settings.LogCacheMessages ? (CacheItemRemovedCallback) RemovedPageIndexCallBack : null;

            //get list of portal ids for the portals we are storing in the page index
            var portalIds = new List<int>();
            using (portalDepthInfo.GetReadLock())
            {
                portalIds.AddRange(portalDepthInfo.Keys);
            }

            //783 : use cache dependency to manage page index instead of triggerDictionaryRebuild regex.
            SetPageCache(PageIndexKey, tabDictionary, new DNNCacheDependency(GetTabsCacheDependency(portalIds)), settings, onRemovePageIndex);

            SetPageCache(PageIndexDepthKey, portalDepthInfo, settings);

            LogRemovedReason = settings.LogCacheMessages;

            if (settings.LogCacheMessages)
            {
                var elc = new EventLogController();
                var logValue = new LogInfo {LogTypeKey = "HOST_ALERT"};

                logValue.AddProperty("Url Rewriting Caching Message", "Page Index built and Stored in Cache");
                logValue.AddProperty("Reason", reason);
                logValue.AddProperty("Cache Item Key", PageIndexKey);
                using (tabDictionary.GetReadLock())
                {
                    logValue.AddProperty("Item Count", tabDictionary.Count.ToString());
                }
                logValue.AddProperty("Thread Id", Thread.CurrentThread.ManagedThreadId.ToString());
                elc.AddLog(logValue);
            }
        }

        internal static void StorePortalAliasesInCache(SharedDictionary<string, PortalAliasInfo> aliases, FriendlyUrlSettings settings)
        {
            SetPortalCache(PortalAliasListKey, aliases, settings);
        }

        internal static void StorePortalAliasesRegexesInCache(OrderedDictionary regexList, FriendlyUrlSettings settings)
        {
            SetPortalCache(PortalAliasRegexesKey, regexList, settings);
        }

        internal void StoreTabPathsInCache(int portalId, SharedDictionary<string, string> tabPathDictionary, FriendlyUrlSettings settings)
        {
            SetPageCache(string.Format(TabPathsKey, portalId), 
                        tabPathDictionary, 
                        new DNNCacheDependency(GetTabsCacheDependency(new List<int> { portalId })), 
                        settings, 
                        null);
        }

        #endregion

        #region Public Methods

        public static void FlushFriendlyUrlSettingsFromCache()
        {
            DataCache.RemoveCache(FriendlyUrlSettingsKey);
        }

        public static void FlushPageIndexFromCache()
        {
            DataCache.RemoveCache(UrlDictKey);
            DataCache.RemoveCache(PageIndexKey);
            DataCache.RemoveCache(PageIndexDepthKey);
            DataCache.RemoveCache(UrlPortalsKey);
            DataCache.RemoveCache(UserProfileActionsKey);
            DataCache.RemoveCache(PortalAliasListKey);
            DataCache.RemoveCache(PortalAliasRegexesKey);
            DataCache.RemoveCache(TabPathsKey);
        }

        /// <summary>
        /// Returns a portal info object for the portal
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="exceptionOnNull"></param>
        /// <remarks>This method wraps the PortalController.GetPortal method, and adds a check if the reuslt is null.</remarks>.
        /// <returns></returns>
        public static PortalInfo GetPortal(int portalId, bool exceptionOnNull)
        {
            PortalInfo pi = null;
            //775 : change to use threadsafe dictionary
            SharedDictionary<int, PortalInfo> portals = (SharedDictionary<int, PortalInfo>)DataCache.GetCache(PortalsKey) ??
                                                            new SharedDictionary<int, PortalInfo>();

            using (portals.GetWriteLock())
            {
                if (portals.ContainsKey(portalId))
                {
                    //portal found, return
                    pi = portals[portalId];
                }
                else
                {
                    try
                    {
                        //if not found, get from database
                        var pc = new PortalController();
                        pi = pc.GetPortal(portalId);

                        if (pi == null)
                        {
                            // Home page redirect loop when using default language not en-US and first request with secondary language
                            //calls get portal using culture code to support
                            string cultureCode = PortalController.GetActivePortalLanguage(portalId);
                            pi = pc.GetPortal(portalId, cultureCode);
                        }
                        if (pi != null)
                        {
                            // Home page redirect loop when using default language not en-US and first request with secondary language
                            //check for correct, default language code in portal object
                            string portalCultureCode = pi.CultureCode;
                            if (portalCultureCode != null &&
                                String.CompareOrdinal(portalCultureCode, pi.DefaultLanguage) != 0)
                            {
                                //portal culture code and default culture code are not the same.
                                //this means we will get the incorrect home page tab id
                                //call back and get the correct one as per the default language
                                PortalInfo defaultLangPortal = pc.GetPortal(portalId, pi.DefaultLanguage);
                                if (defaultLangPortal != null)
                                {
                                    pi = defaultLangPortal;
                                }
                            }
                        }
                        if (pi != null)
                        {
                            //add to dictionary and re-store in cache
                            portals.Add(pi.PortalID, pi);
                            DataCache.SetCache(PortalsKey, portals); //store back in dictionary
                        }
                    }
// ReSharper disable EmptyGeneralCatchClause
                    catch
// ReSharper restore EmptyGeneralCatchClause
                    {
                        //912: capture as fall back any exception resulting from doing a portal lookup in 6.x
                        //this happens when portalId = -1
                        //no long, no handling, just passonwards with null portal
                    }
                }
            }

            if (exceptionOnNull && pi == null)
            {
                throw new NullReferenceException("No Portal Found for portalid : " + portalId.ToString());
            }
            return pi;
        }

        public void RemovedPageIndexCallBack(string k, object v, CacheItemRemovedReason r)
        {
            cacheItemRemovedReason = r;
#if (DEBUG)
            if (LogRemovedReason)
            {
                var elc = new EventLogController();
                var logValue = new LogInfo { LogTypeKey = "HOST_ALERT" };

                string itemName;
                string count;
                List<string> portalCounts = null;
                switch (k)
                {
                    case "DNN_" + PageIndexKey:
                        itemName = "Page Index";
                        //user profile actions
                        try
                        {
                            DataCache.RemoveCache(UserProfileActionsKey);
                        }
                        catch (ConfigurationErrorsException)
                        {
                            //do nothing, this means the web.config file was overwritten, and thus the cache
                            //was cleared.
                        }
                        if (v != null && v.GetType() == typeof(SharedDictionary<string, string>))
                        {
                            count = "Item Count: " + ((SharedDictionary<string, string>)v).Values.Count.ToString();
                        }
                        else
                        {
                            count = "N/a";
                        }

                        break;
                    case "DNN_"  + UrlDictKey:
                        itemName = "Friendly Url List";
                        if (v != null &&
                            v.GetType() == typeof(SharedDictionary<int, SharedDictionary<string, string>>))
                        {
                            var friendlyUrls = (SharedDictionary<int, SharedDictionary<string, string>>)v;
                            portalCounts = new List<string>();
                            using (friendlyUrls.GetReadLock())
                            {
                                count = "Portal Count: " + friendlyUrls.Count.ToString();
                                foreach (int key in friendlyUrls.Keys)
                                {
                                    SharedDictionary<string, string> portalUrls = friendlyUrls[key];
                                    using (portalUrls.GetReadLock())
                                    {
                                        portalCounts.Add("Portal " + key.ToString() + " Item Count :" + portalUrls.Count.ToString());
                                    }
                                }
                            }
                        }
                        else
                        {
                            count = "N/a";
                        }
                        break;
                    default:
                        itemName = "Url Rewriter Cache Item";
                        count = "";
                        break;
                }
                //add log values
                logValue.AddProperty("Url Rewriting Caching Message", itemName + " Cache item Removed.");
                logValue.AddProperty("Reason", cacheItemRemovedReason.ToString());
                logValue.AddProperty("Cache Item Key", k);
                logValue.AddProperty("Item Count", count);
                if (portalCounts != null)
                {
                    int i = 0;
                    foreach (string item in portalCounts)
                    {
                        logValue.AddProperty("Item " + i.ToString(), item);
                        i++;
                    }
                }
                //System.Diagnostics.Trace.Assert(k != null, "k == " + k);
                elc.AddLog(logValue);
            }
#endif
        }

        #endregion
    }
}