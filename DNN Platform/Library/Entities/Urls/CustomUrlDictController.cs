﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;

    internal static class CustomUrlDictController
    {
        /// <summary>returns a tabId indexed dictionary of Friendly Urls.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="forceRebuild">Whether to force a rebuild of the dictionary.</param>
        /// <param name="bypassCache">Whether to bypass the cache for the dictionary.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <param name="customAliasForTabs">A collection of custom alises for each tab.</param>
        /// <param name="parentTraceId">The parent trace ID.</param>
        /// <returns>A <see cref="SharedDictionary{TKey,TValue}"/> where the key is a tab ID and the value is a <see cref="SharedDictionary{TKey,TValue}"/> with keys of URL culture and values of URLs.</returns>
        internal static SharedDictionary<int, SharedDictionary<string, string>> FetchCustomUrlDictionary(
            int portalId,
            bool forceRebuild,
            bool bypassCache,
            FriendlyUrlSettings settings,
            out SharedDictionary<string, string> customAliasForTabs,
            Guid parentTraceId)
        {
            SharedDictionary<int, SharedDictionary<string, string>> urlDict;

            // this contains a list of all tabs for all the portals that have been retrieved
            ConcurrentBag<int> urlPortals; // this contains a list of the portals that have been retrieved

            // get the objects from the cache
            var cc = new CacheController();
            cc.GetFriendlyUrlIndexFromCache(out urlDict, out urlPortals, out customAliasForTabs);

            if (urlDict != null && forceRebuild == false && bypassCache == false)
            {
                if (urlPortals == null)
                {
                    // no portals retrieved from cache, but was a dictionary.  Bit weird, but we'll run with it
                    urlPortals = new ConcurrentBag<int>();
                }

                // check to see if this portal has been included in the dict
                if (urlPortals.Contains(portalId) == false)
                {
                    // ok, there is a url dictionary, but this portal isn't in it, so
                    // put it in and get the urls for this portal
                    // this call appends extra portals to the list
                    urlDict = BuildUrlDictionary(urlDict, portalId, settings, ref customAliasForTabs);
                    urlPortals.Add(portalId);

                    cc.StoreFriendlyUrlIndexInCache(
                        urlDict,
                        urlPortals,
                        customAliasForTabs,
                        settings,
                        "Portal Id " + portalId.ToString() + " added to index.");
                }
            }
            else
            {
                // either values are null (Not in cache) or we want to force the rebuild, or we want to bypass the cache
                // rebuild the dictionary for this portal
                urlDict = BuildUrlDictionary(urlDict, portalId, settings, ref customAliasForTabs);
                urlPortals = new ConcurrentBag<int> { portalId }; // always rebuild the portal list

                // if we are to cache this item (byPassCache = false)
                if (bypassCache == false)
                {
                    // cache these items
                    string reason = forceRebuild ? "Force Rebuild of Index" : "Index not in cache";
                    cc.StoreFriendlyUrlIndexInCache(urlDict, urlPortals, customAliasForTabs, settings, reason);
                }
            }

            return urlDict;
        }

        internal static void InvalidateDictionary()
        {
            CacheController.FlushPageIndexFromCache();
        }

        /// <summary>
        /// Returns a list of tab and redirects from the database, for the specified portal
        /// Assumes that the dictionary should have any existing items replaced if the portal ID is specified
        /// and the portal tabs already exist in the dictionary.
        /// </summary>
        /// <param name="existingTabs">The collection of tab URLs.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <param name="customAliasTabs">A collection of custom aliases for the tabs.</param>
        /// <remarks>
        ///    Each dictionary entry in the return value is a complex data type of another dictionary that is indexed by the url culture.  If there is
        ///    only one culture for the Url, it will be that culture.
        /// </remarks>
        /// <returns>A <see cref="SharedDictionary{TKey,TValue}"/> where the key is a tab ID and the value is a <see cref="SharedDictionary{TKey,TValue}"/> with keys of URL culture and values of URLs.</returns>
        private static SharedDictionary<int, SharedDictionary<string, string>> BuildUrlDictionary(
            SharedDictionary<int, SharedDictionary<string, string>> existingTabs,
            int portalId,
            FriendlyUrlSettings settings,
            ref SharedDictionary<string, string> customAliasTabs)
        {
            // fetch tabs with redirects
            var tabs = FriendlyUrlController.GetTabs(portalId, false, null, settings);
            if (existingTabs == null)
            {
                existingTabs = new SharedDictionary<int, SharedDictionary<string, string>>();
            }

            if (customAliasTabs == null)
            {
                customAliasTabs = new SharedDictionary<string, string>();
            }

            // go through each tab in the found list
            foreach (TabInfo tab in tabs.Values)
            {
                // check the custom alias tabs collection and add to the dictionary where necessary
                foreach (var customAlias in tab.CustomAliases)
                {
                    string key = tab.TabID.ToString() + ":" + customAlias.Key;
                    using (customAliasTabs.GetWriteLock()) // obtain write lock on custom alias Tabs
                    {
                        if (customAliasTabs.ContainsKey(key) == false)
                        {
                            customAliasTabs.Add(key, customAlias.Value);
                        }
                    }
                }

                foreach (TabUrlInfo redirect in tab.TabUrls)
                {
                    if (redirect.HttpStatus == "200")
                    {
                        string url = redirect.Url;

                        // 770 : add in custom alias into the tab path for the custom Urls
                        if (redirect.PortalAliasUsage != PortalAliasUsageType.Default && redirect.PortalAliasId > 0)
                        {
                            // there is a custom http alias specified for this portal alias
                            PortalAliasInfo alias = PortalAliasController.Instance.GetPortalAliasByPortalAliasID(redirect.PortalAliasId);
                            if (alias != null)
                            {
                                string customHttpAlias = alias.HTTPAlias;
                                url = customHttpAlias + "::" + url;
                            }
                        }

                        string cultureKey = redirect.CultureCode.ToLowerInvariant();
                        var locales = LocaleController.Instance.GetLocales(portalId).Values;
                        if (string.IsNullOrEmpty(cultureKey))
                        {
                            // Add entry for each culture
                            foreach (Locale locale in locales)
                            {
                                AddEntryToDictionary(existingTabs, portalId, tab, locale.Code.ToLowerInvariant(), url);
                            }
                        }
                        else
                        {
                            AddEntryToDictionary(existingTabs, portalId, tab, cultureKey, url);
                        }
                    }
                }
            }

            return existingTabs;
        }

        private static void AddEntryToDictionary(SharedDictionary<int, SharedDictionary<string, string>> existingTabs, int portalId, TabInfo tab, string cultureKey, string url)
        {
            int tabid = tab.TabID;
            using (existingTabs.GetWriteLock())
            {
                if (existingTabs.ContainsKey(tabid) == false)
                {
                    var entry = new SharedDictionary<string, string>();
                    using (entry.GetWriteLock())
                    {
                        entry.Add(cultureKey, url);
                    }

                    // 871 : use lower case culture code as key
                    existingTabs.Add(tab.TabID, entry);
                }
                else
                {
                    SharedDictionary<string, string> entry = existingTabs[tabid];

                    // replace tab if existing but was retreieved from tabs call
                    if (tab.PortalID == portalId || portalId == -1)
                    {
                        using (entry.GetWriteLock())
                        {
                            if (entry.ContainsKey(cultureKey) == false)
                            {
                                // add the culture and set in parent dictionary
                                // 871 : use lower case culture code as key
                                entry.Add(cultureKey, url);
                                existingTabs[tabid] = entry;
                            }
                        }
                    }
                }
            }
        }
    }
}
