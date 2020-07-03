// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Log.EventLog;

    internal static class TabIndexController
    {
        private static readonly object tabPathDictBuildLock = new object();

        public static void InvalidateDictionary(string reason, PageIndexData rebuildData, int portalId)
        {
            // if supplied, store the rebuildData for when the dictionary gets rebuilt
            // this is a way of storing data between dictionary rebuilds
            if (rebuildData != null)
            {
                DataCache.SetCache("rebuildData", rebuildData);
            }

            // add log entry for cache clearance
            var log = new LogInfo { LogTypeKey = "HOST_ALERT" };
            try
            {
                // 817 : not clearing items correctly from dictionary
                CacheController.FlushPageIndexFromCache();
            }
            catch (Exception ex)
            {
                // do nothing ; can be from trying to access cache after system restart
                Services.Exceptions.Exceptions.LogException(ex);
            }

            log.AddProperty("Url Rewriting Caching Message", "Page Index Cache Cleared.  Reason: " + reason);
            log.AddProperty("Thread Id", Thread.CurrentThread.ManagedThreadId.ToString());
            LogController.Instance.AddLog(log);
        }

        internal static string CreateRewritePath(int tabId, string cultureCode, params string[] keyValuePair)
        {
            string rewritePath = "?TabId=" + tabId.ToString();

            // 736 : 5.5 compatibility - identify tab rewriting at source by tab culture code
            RewriteController.AddLanguageCodeToRewritePath(ref rewritePath, cultureCode);
            return keyValuePair.Aggregate(rewritePath, (current, keyValue) => current + ("&" + keyValue));
        }

        internal static string CreateRewritePath(int tabId, string cultureCode, ActionType action, RedirectReason reason)
        {
            string rewritePath = CreateRewritePath(tabId, cultureCode);
            rewritePath = RedirectTokens.AddRedirectReasonToRewritePath(rewritePath, action, reason);
            return rewritePath;
        }

        /// <summary>
        /// Gets the Tab Dictionary from the DataCache memory location, if it's empty or missing, builds a new one.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="minTabPathDepth">ByRef parameter to return the minimum tab path depth (the number of '/' in the tab path).</param>
        /// <param name="maxTabPathDepth">ByRef parameter to return the maximum tab path depth (the number of '/' in the tab path).</param>
        /// <param name="minAliasPathDepth">ByRef parameter to return the minimum alias path depth (the number of '/' in the alias path.</param>
        /// <param name="maxAliasPathDepth">ByRef parameter to return the maximum alias path depth (the number of '/' in the alias path).</param>
        /// <param name="settings"></param>
        /// <param name="forceRebuild"></param>
        /// <param name="bypassCache"></param>
        /// <param name="parentTraceId"></param>
        /// <returns>Dictionary (string, string) of Tab paths in tab key, with the rewrite path as the value.</returns>
        /// <remarks>
        /// Changes
        /// Task 608 : Incrementally build tab dictionary instead of building entire dicitionary all at once
        /// Task 609 : If usePortalAlias is specified, only build dictionary with specific portal alias : ignore others
        /// Task 791 : allow for specification of true/false for using thread locking to prevent multiple rebuilds on threads.
        /// </remarks>
        internal static SharedDictionary<string, string> FetchTabDictionary(
            int portalId,
            out int minTabPathDepth,
            out int maxTabPathDepth,
            out int minAliasPathDepth,
            out int maxAliasPathDepth,
            FriendlyUrlSettings settings,
            bool forceRebuild,
            bool bypassCache,
            Guid parentTraceId)
        {
            PathSizes depthInfo;
            SharedDictionary<int, PathSizes> portalDepths = null;
            SharedDictionary<string, string> dict = null;
            SharedDictionary<string, string> portalTabPathDictionary = null;
            string reason = string.Empty;

            var cc = new CacheController();
            if (bypassCache == false)
            {
                cc.GetPageIndexFromCache(out dict, out portalDepths, settings);
                portalTabPathDictionary = FetchTabPathDictionary(portalId);
            }

            if (dict == null || portalDepths == null || portalTabPathDictionary == null || !PortalExistsInIndex(portalDepths, portalId) || forceRebuild)
            {
                // place threadlock to prevent two threads getting a null object. Use the same lock object that is used to
                lock (tabPathDictBuildLock)
                {
                    // check for the tab dictionary in the DataCache again as it could have been cached by another thread
                    // while waiting for the lock to become available.
                    if (bypassCache == false)
                    {
                        cc.GetPageIndexFromCache(out dict, out portalDepths, settings);
                        portalTabPathDictionary = FetchTabPathDictionary(portalId);
                    }

                    if (dict == null || portalDepths == null || portalTabPathDictionary == null || !PortalExistsInIndex(portalDepths, portalId) || forceRebuild)
                    {
                        Hashtable homePageSkins; // keeps a list of skins set per home page and culture

                        if (!bypassCache && dict == null)
                        {
                            reason += "No Page index in cache;";
                        }

                        if (forceRebuild)
                        {
                            reason += "Force Rebuild;";
                        }

                        if (bypassCache)
                        {
                            reason += "Bypass Cache;";
                        }

                        // PathSizes depthInfo;
                        // the cached dictionary was null or forceRebuild = true or bypassCache = true, so go get a new dictionary
                        dict = BuildTabDictionary(
                            out depthInfo,
                            settings,
                            portalId,
                            dict,
                            out homePageSkins,
                            out portalTabPathDictionary,
                            parentTraceId);

                        if (portalDepths == null || forceRebuild)
                        {
                            portalDepths = new SharedDictionary<int, PathSizes>();
                        }

                        // store the fact that this portal has been built
                        using (portalDepths.GetWriteLock())
                        {
                            // depthInfo may already exist in index so use indexer to Add/Update rather than using Add method which
                            // would throw an exception if the portal already existed in the dictionary.
                            portalDepths[portalId] = depthInfo;
                        }

                        if (bypassCache == false) // only cache if bypass not switched on
                        {
                            reason += "Portal " + portalId + " added to index;";
                            using (dict.GetReadLock())
                            {
                                reason += "Existing Page Index=" + dict.Count + " items;";
                            }

                            cc.StorePageIndexInCache(dict, portalDepths, settings, reason);
                            cc.StoreTabPathsInCache(portalId, portalTabPathDictionary, settings);
                            CacheController.StoreHomePageSkinsInCache(portalId, homePageSkins);
                        }
                    }
                }
            }

            if (PortalExistsInIndex(portalDepths, portalId))
            {
                using (portalDepths.GetReadLock())
                {
                    depthInfo = portalDepths[portalId];
                    minTabPathDepth = depthInfo.MinTabPathDepth;
                    maxTabPathDepth = depthInfo.MaxTabPathDepth;
                    minAliasPathDepth = depthInfo.MinAliasDepth;
                    maxAliasPathDepth = depthInfo.MaxAliasDepth;
                }
            }
            else
            {
                // fallback values, should never get here: mainly for compiler wranings
                minTabPathDepth = 1;
                maxTabPathDepth = 10;
                minAliasPathDepth = 1;
                maxAliasPathDepth = 4;
            }

            return dict;
        }

        /// <summary>
        /// Returns a list of aliases that are used in custom tab/alias association.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal static List<string> GetCustomPortalAliases(FriendlyUrlSettings settings)
        {
            List<string> aliases = CacheController.GetCustomAliasesFromCache();
            if (aliases == null)
            {
                aliases = FriendlyUrlController.GetCustomAliasesForTabs();
                CacheController.StoreCustomAliasesInCache(aliases, settings);
            }

            return aliases;
        }

        /// <summary>
        /// Gets the portal alias by portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        internal static PortalAliasInfo GetPortalAliasByPortal(int portalId, string portalAlias)
        {
            PortalAliasInfo retValue = null;

            // get the portal alias collection from the cache
            var portalAliasCollection = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).ToList();

            bool foundAlias = false;

            // Do a specified PortalAlias check first
            PortalAliasInfo portalAliasInfo = portalAliasCollection.SingleOrDefault(a => a.HTTPAlias == portalAlias.ToLowerInvariant());
            if (portalAliasInfo != null)
            {
                if (portalAliasInfo.PortalID == portalId)
                {
                    // set the alias
                    retValue = portalAliasInfo;
                    foundAlias = true;
                }
            }

            if (!foundAlias)
            {
                // collection to hold aliases sorted by length, longest first
                var aliases = (from p in portalAliasCollection
                               select p.HTTPAlias).ToList();

                // get aliases sorted by length of alias
                aliases.Sort(new StringLengthComparer());

                // searching from longest to shortest alias ensures that the most specific portal is matched first
                // In some cases this method has been called with "portalaliases" that were not exactly the real portal alias
                // the startswith behaviour is preserved here to support those non-specific uses
                // IEnumerable<String> aliases = portalAliasCollection.Keys.Cast<String>().OrderByDescending(k => k.Length);
                foreach (string currentAlias in aliases)
                {
                    // check if the alias key starts with the portal alias value passed in - we use
                    // StartsWith because child portals are redirected to the parent portal domain name
                    // eg. child = 'www.domain.com/child' and parent is 'www.domain.com'
                    // this allows the parent domain name to resolve to the child alias ( the tabid still identifies the child portalid )
                    portalAliasInfo = portalAliasCollection.SingleOrDefault(a => a.HTTPAlias == currentAlias);
                    if (portalAliasInfo != null)
                    {
                        string httpAlias = portalAliasInfo.HTTPAlias.ToLowerInvariant();
                        if (httpAlias.StartsWith(portalAlias.ToLowerInvariant()) && portalAliasInfo.PortalID == portalId)
                        {
                            retValue = portalAliasInfo;
                            break;
                        }

                        httpAlias = httpAlias.StartsWith("www.") ? httpAlias.Replace("www.", string.Empty) : string.Concat("www.", httpAlias);
                        if (httpAlias.StartsWith(portalAlias.ToLowerInvariant()) && portalAliasInfo.PortalID == portalId)
                        {
                            retValue = portalAliasInfo;
                            break;
                        }
                    }
                }
            }

            return retValue;
        }

        /// <summary>
        /// Returns an ordered dictionary of alias regex patterns.  These patterns are used to identify a portal alias by getting a match.
        /// </summary>
        /// <returns></returns>
        internal static OrderedDictionary GetPortalAliases(FriendlyUrlSettings settings)
        {
            // object to return
            OrderedDictionary aliasList = CacheController.GetPortalAliasesFromCache();
            if (aliasList == null)
            {
                aliasList = BuildPortalAliasesDictionary();
                CacheController.StorePortalAliasesInCache(aliasList, settings);
            }

            return aliasList;
        }

        /// <summary>
        /// Returns the tab path of the base DNN tab.  Ie /Home or /Somepage/SomeOtherPage.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="options"></param>
        /// <param name="parentTraceId"></param>
        /// <remarks>Will remove // from the tabPath as stored in the Tabs object/table.</remarks>
        /// <returns></returns>
        internal static string GetTabPath(TabInfo tab, FriendlyUrlOptions options, Guid parentTraceId)
        {
            string tabPath = null;
            if (options.CanGenerateNonStandardPath)
            {
                var tpd = FetchTabPathDictionary(tab.PortalID);

                if (tpd != null)
                {
                    using (tpd.GetReadLock())
                    {
                        if (tpd.Count > 0)
                        {
                            // get the path from the dictionary
                            string tabKey = tab.TabID.ToString();
                            if (tpd.ContainsKey(tabKey))
                            {
                                tabPath = tpd[tabKey];
                            }
                        }
                    }
                }
            }

            return tabPath ?? TabPathHelper.GetFriendlyUrlTabPath(tab, options, parentTraceId);
        }

        private static void AddCustomRedirectsToDictionary(
            SharedDictionary<string, string> tabIndex,
            Dictionary<string, DupKeyCheck> dupCheck,
            string httpAlias,
            TabInfo tab,
            FriendlyUrlSettings settings,
            FriendlyUrlOptions options,
            ref string rewritePath,
            out int tabPathDepth,
            ref List<string> customHttpAliasesUsed,
            bool isDeleted,
            Guid parentTraceId)
        {
            tabPathDepth = 1;
            var duplicateHandlingPreference = UrlEnums.TabKeyPreference.TabRedirected;
            bool checkForDupUrls = settings.CheckForDuplicateUrls;

            // 697 : custom url rewrites with large number of path depths fail because of incorrect path depth calculation
            int maxTabPathDepth = 1;
            string origRewritePath = rewritePath;
            string newRewritePath = rewritePath;
            string aliasCulture = null;

            // get the culture for this alias
            var primaryAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(tab.PortalID).ToList();

            if (primaryAliases.Count > 0)
            {
                aliasCulture = primaryAliases.GetCultureByPortalIdAndAlias(tab.PortalID, httpAlias);
            }

            foreach (var redirect in tab.TabUrls)
            {
                rewritePath = origRewritePath;

                // allow for additional qs parameters
                if (!string.IsNullOrEmpty(redirect.QueryString))
                {
                    rewritePath += redirect.QueryString.StartsWith("&") ? redirect.QueryString : "&" + redirect.QueryString;
                }

                string redirectTabPath = redirect.Url;
                string redirectedRewritePath = rewritePath;

                // 770 : allow for custom portal aliases
                string redirectAlias = httpAlias;
                if (redirect.PortalAliasId > 0)
                {
                    // has a custom portal alias
                    PortalAliasInfo customAlias = PortalAliasController.Instance.GetPortalAliasByPortalAliasID(redirect.PortalAliasId);
                    if (customAlias != null)
                    {
                        // this will be used to add the Url to the dictionary
                        redirectAlias = customAlias.HTTPAlias;

                        // add to the list of custom aliases used by the portal
                        if (customHttpAliasesUsed == null)
                        {
                            customHttpAliasesUsed = new List<string>();
                        }

                        if (!customHttpAliasesUsed.Contains(redirectAlias))
                        {
                            customHttpAliasesUsed.Add(redirectAlias);
                        }
                    }
                }

                // set the redirect status using the httpStatus
                switch (redirect.HttpStatus)
                {
                    case "301":
                        redirectedRewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                            rewritePath,
                            ActionType.Redirect301,
                            RedirectReason.Custom_Redirect);
                        break;
                    case "302":
                        redirectedRewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                            rewritePath,
                            ActionType.Redirect302,
                            RedirectReason.Custom_Redirect);
                        break;
                    case "404":
                        redirectedRewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                            rewritePath,
                            ActionType.Output404,
                            RedirectReason.Custom_Redirect);
                        break;
                    case "200":
                        // when there is a 200, then replace the 'standard' path
                        newRewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                            newRewritePath,
                            ActionType.CheckFor301,
                            RedirectReason.Custom_Redirect);

                        // 672 : replacement urls have preference over all redirects, deleted tabs and standard urls
                        duplicateHandlingPreference = UrlEnums.TabKeyPreference.TabOK;
                        break;
                }

                // check the culture of the redirect to see if it either doesn't match the alias or needs to specify
                // the language when requested
                if (!string.IsNullOrEmpty(redirect.CultureCode) && redirect.CultureCode != "Default")
                {
                    // 806 : specify duplicates where the alias culture doesn't match the redirect culture
                    // so that redirect to the best match between alias culture and redirect culture
                    // compare the supplied alias culture with the redirect culture
                    // 856 : if alias culture == "" and a custom 301 redirect then redirects are forced
                    if (!string.IsNullOrEmpty(aliasCulture) && aliasCulture != redirect.CultureCode)
                    {
                        // the culture code and the specific culture alias don't match
                        // set 301 check status and set to delete if a duplicate is found
                        redirectedRewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                                                                            redirectedRewritePath,
                                                                            ActionType.CheckFor301,
                                                                            RedirectReason.Custom_Redirect);
                        newRewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                            newRewritePath,
                            ActionType.CheckFor301,
                            RedirectReason.Custom_Redirect);
                        duplicateHandlingPreference = UrlEnums.TabKeyPreference.TabRedirected;
                    }

                    // add on the culture code for the redirect, so that the rewrite silently sets the culture for the page
                    RewriteController.AddLanguageCodeToRewritePath(ref redirectedRewritePath, redirect.CultureCode);
                }

                // now add the custom redirect to the tab dictionary
                if (string.Compare(httpAlias, redirectAlias, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    AddToTabDict(
                        tabIndex,
                        dupCheck,
                        httpAlias,
                        redirectTabPath,
                        redirectedRewritePath,
                        tab.TabID,
                        duplicateHandlingPreference,
                        ref tabPathDepth,
                        checkForDupUrls,
                        isDeleted);
                }
                else
                {
                    // 770 : there is a specific alias for this tab
                    // if not a redirect already, make it a redirect for the wrong (original) rewrite path
                    string wrongAliasRedirectedRewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                        redirectedRewritePath,
                        ActionType.Redirect301,
                        RedirectReason.Custom_Tab_Alias);

                    // add in the entry with the specific redirectAlias
                    if (redirectTabPath == string.Empty)
                    {
                        // when adding a blank custom Url, also add in a standard tab path url, because any url that also includes querystring data will use the standard tab path
                        string tabPath = GetTabPath(tab, options, parentTraceId);
                        string stdDictRewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                            rewritePath,
                            ActionType.CheckFor301,
                            RedirectReason.Custom_Tab_Alias);
                        AddToTabDict(
                            tabIndex,
                            dupCheck,
                            redirectAlias,
                            tabPath,
                            stdDictRewritePath,
                            tab.TabID,
                            UrlEnums.TabKeyPreference.TabOK,
                            ref tabPathDepth,
                            checkForDupUrls,
                            isDeleted);

                        // then add in the portal alias with no tabpath (ie like a site root url)
                        AddToTabDict(
                            tabIndex,
                            dupCheck,
                            redirectAlias,
                            redirectTabPath,
                            redirectedRewritePath,
                            tab.TabID,
                            duplicateHandlingPreference,
                            ref tabPathDepth,
                            checkForDupUrls,
                            isDeleted);

                        // 838 : disabled tabs with custom aliases - still load the settings page without redirect
                        // disabled / not active by date / external url pages cannot navigate to settings page
                        if (tab.DisableLink || !string.IsNullOrEmpty(tab.Url) ||
                           (tab.EndDate < DateTime.Now && tab.EndDate > DateTime.MinValue) ||
                           (tab.StartDate > DateTime.Now && tab.StartDate > DateTime.MinValue))
                        {
                            string settingsUrl = tabPath + "/ctl/Tab";
                            string settingsRewritePath = CreateRewritePath(tab.TabID, redirect.CultureCode, "ctl=Tab");

                            // no redirect on the ctl/Tab url
                            // add in the ctl/tab Url for the custom alias, with no redirect so that the page settings can be loaded
                            AddToTabDict(
                                tabIndex,
                                dupCheck,
                                redirectAlias,
                                settingsUrl,
                                settingsRewritePath,
                                tab.TabID,
                                UrlEnums.TabKeyPreference.TabRedirected,
                                ref tabPathDepth,
                                settings.CheckForDuplicateUrls,
                                isDeleted);
                        }
                    }
                    else
                    {
                        // add in custom entry with different alias
                        AddToTabDict(
                            tabIndex,
                            dupCheck,
                            redirectAlias,
                            redirectTabPath,
                            redirectedRewritePath,
                            tab.TabID,
                            duplicateHandlingPreference,
                            ref tabPathDepth,
                            checkForDupUrls,
                            isDeleted);

                        // add in the entry with the original alias, plus an instruction to redirect if it's used
                        AddToTabDict(
                            tabIndex,
                            dupCheck,
                            httpAlias,
                            redirectTabPath,
                            wrongAliasRedirectedRewritePath,
                            tab.TabID,
                            duplicateHandlingPreference,
                            ref tabPathDepth,
                            checkForDupUrls,
                            isDeleted);
                    }
                }

                if (tabPathDepth > maxTabPathDepth)
                {
                    maxTabPathDepth = tabPathDepth;
                }
            }

            // return the highest tabpath depth found
            tabPathDepth = maxTabPathDepth;

            // return any changes to the rewritePath
            rewritePath = newRewritePath;
        }

        private static void AddInternalAliases(FriendlyUrlSettings settings, List<string> usingHttpAliases)
        {
            if (settings.InternalAliasList != null && settings.InternalAliasList.Count > 0)
            {
                foreach (InternalAlias ia in settings.InternalAliasList)
                {
                    if (usingHttpAliases.Contains(ia.HttpAlias) == false)
                    {
                        usingHttpAliases.Add(ia.HttpAlias);
                    }
                }
            }
        }

        private static void AddPermanentRedirectToDictionary(
            SharedDictionary<string, string> tabIndex,
            Dictionary<string, DupKeyCheck> dupCheck,
            string httpAlias,
            TabInfo tab,
            string tabPath,
            ref string rewritePath,
            ref int tabPathDepth,
            bool checkForDupUrls,
            bool isDeleted)
        {
            // because we have to catch all versions of this in the dictionary, then we have to add the 'base' url
            AddToTabDict(
                tabIndex,
                dupCheck,
                httpAlias,
                tab.TabPath + "/tabid/" + tab.TabID + "/default",
                rewritePath,
                tab.TabID,
                UrlEnums.TabKeyPreference.TabRedirected,
                ref tabPathDepth,
                checkForDupUrls,
                isDeleted);

            // and put in the name-less one as well, just in case a prior version of the site was runnign without the tabnames (urlformat=sefriendly)
            AddToTabDict(
                tabIndex,
                dupCheck,
                httpAlias,
                "/tabid/" + tab.TabID + "/default",
                rewritePath,
                tab.TabID,
                UrlEnums.TabKeyPreference.TabRedirected,
                ref tabPathDepth,
                checkForDupUrls,
                isDeleted);

            // finally, put one in for the ctl/tab combination, so that you can actually get to the page settings
            AddToTabDict(
                tabIndex,
                dupCheck,
                httpAlias,
                tabPath.Replace("//", "/") + "/ctl/Tab",
                CreateRewritePath(tab.TabID, string.Empty, "ctl=Tab"),
                tab.TabID,
                UrlEnums.TabKeyPreference.TabRedirected,
                ref tabPathDepth,
                checkForDupUrls,
                isDeleted);
        }

        private static void AddSiteRootRedirects(
            PathSizes pathSizes,
            SharedDictionary<string, string> tabIndex,
            IEnumerable<PortalAliasInfo> chosenAliases,
            bool hasSiteRootRedirect,
            Dictionary<string, DupKeyCheck> dupCheck,
            ICollection<string> usingHttpAliases)
        {
            foreach (PortalAliasInfo alias in chosenAliases) // and that is once per portal alias per portal
            {
                string httpAlias = alias.HTTPAlias;

                // check to see if there is a parameter rewrite rule that allows for parameters on the site root
                if (hasSiteRootRedirect)
                {
                    int tempPathDepth = 0;
                    AddToTabDict(
                        tabIndex,
                        dupCheck,
                        httpAlias,
                        "*",
                        string.Empty,
                        -1,
                        UrlEnums.TabKeyPreference.TabOK,
                        ref tempPathDepth,
                        false,
                        false);
                }

                pathSizes.SetAliasDepth(httpAlias);

                // keep track of the http Aliases, this will be used to feed into the tab dictionary (ie, one alias per tab)
                usingHttpAliases.Add(httpAlias.ToLowerInvariant());
            }
        }

        private static void AddStandardPagesToDict(
            SharedDictionary<string, string> tabIndex,
            Dictionary<string, DupKeyCheck> dupCheck,
            string httpAlias,
            int portalId,
            string cultureCode)
        {
            int tabDepth = 0; // we ignore tab depth as it is only one for these in-built urls

            // 850 : add in the culture code to the redirect if supplied
            string portalRewritePath = "?PortalId=" + portalId.ToString();
            string cultureRewritePath = string.Empty;
            if (!string.IsNullOrEmpty(cultureCode))
            {
                cultureRewritePath += "&language=" + cultureCode;
            }

            // hard coded page paths - using 'tabDeleted' in case there is a clash with an existing page (ie, someone has created a page that takes place of the standard page, created page has preference)

            // need check custom login/register page set in portal and redirect to the specific page.
            var portal = PortalController.Instance.GetPortal(portalId);
            var loginRewritePath = portalRewritePath + "&ctl=Login" + cultureRewritePath;
            var loginPreference = UrlEnums.TabKeyPreference.TabDeleted;
            var loginTabId = Null.NullInteger;
            if (portal != null && portal.LoginTabId > Null.NullInteger && Globals.ValidateLoginTabID(portal.LoginTabId))
            {
                loginTabId = portal.LoginTabId;
                loginPreference = UrlEnums.TabKeyPreference.TabOK;
                loginRewritePath = CreateRewritePath(loginTabId, cultureCode);
            }

            AddToTabDict(
                tabIndex,
                dupCheck,
                httpAlias,
                "login",
                loginRewritePath,
                loginTabId,
                loginPreference,
                ref tabDepth,
                false,
                false);

            var registerRewritePath = portalRewritePath + "&ctl=Register" + cultureRewritePath;
            var registerPreference = UrlEnums.TabKeyPreference.TabDeleted;
            var registerTabId = Null.NullInteger;
            if (portal != null && portal.RegisterTabId > Null.NullInteger)
            {
                registerTabId = portal.RegisterTabId;
                registerPreference = UrlEnums.TabKeyPreference.TabOK;
                registerRewritePath = CreateRewritePath(registerTabId, cultureCode);
            }

            AddToTabDict(
                tabIndex,
                dupCheck,
                httpAlias,
                "register",
                registerRewritePath,
                registerTabId,
                registerPreference,
                ref tabDepth,
                false,
                false);
            AddToTabDict(
                tabIndex,
                dupCheck,
                httpAlias,
                "logoff",
                portalRewritePath + "&ctl=Logoff" + cultureRewritePath,
                -1,
                UrlEnums.TabKeyPreference.TabDeleted,
                ref tabDepth,
                false,
                false);
            AddToTabDict(
                tabIndex,
                dupCheck,
                httpAlias,
                "terms",
                portalRewritePath + "&ctl=Terms" + cultureRewritePath,
                -1,
                UrlEnums.TabKeyPreference.TabDeleted,
                ref tabDepth,
                false,
                false);
            AddToTabDict(
                tabIndex,
                dupCheck,
                httpAlias,
                "privacy",
                portalRewritePath + "&ctl=Privacy" + cultureRewritePath,
                -1,
                UrlEnums.TabKeyPreference.TabDeleted,
                ref tabDepth,
                false,
                false);
        }

        private static int AddTabToTabDict(
            SharedDictionary<string, string> tabIndex,
            Dictionary<string, DupKeyCheck> dupCheck,
            string httpAlias,
            string aliasCulture,
            string customHttpAlias,
            PortalInfo thisPortal,
            string tabPath,
            ref List<string> customAliasesUsed,
            TabInfo tab,
            FriendlyUrlSettings settings,
            FriendlyUrlOptions options,
            int homeTabId,
            ref Hashtable homePageSkins,
            Guid parentTraceId)
        {
            string rewritePath = string.Empty;
            int tabPathDepth = 0;
            bool customAliasUsedAndNotCurrent = !string.IsNullOrEmpty(customHttpAlias) && customHttpAlias != httpAlias;

            // 592 : check the permanent redirect value
            // 736 : 5.5 changes : track tab culture code
            string cultureCode = tab.CultureCode;
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = aliasCulture;
            }

            bool permanentRedirect = tab.PermanentRedirect;

            // determine the rewrite parameter
            // for deleted or pages not enabled yet, direct to the home page if the setting is enabled
            // 534 : tab is disabled, mark as deleted (don't want to cause duplicate tab warnings)
            // DNN-6186: add expired pages in dictionary as admin/host user should able to visit/edit them.
            bool isDeleted = tab.IsDeleted || tab.DisableLink;
            if (isDeleted)

            // don't care what setting is, redirect code will decide whether to redirect or 404 - just mark as page deleted &&
            // settings.DeletedTabHandlingValue == DeletedTabHandlingTypes.Do301RedirectToPortalHome)
            {
                // 777: force 404 result for all deleted pages instead of relying on 'not found'
                // 838 : separate handling for disabled pages
                ActionType action = settings.DeletedTabHandlingType == DeletedTabHandlingType.Do404Error
                                    ? ActionType.Output404
                                    : ActionType.Redirect301;
                rewritePath = tab.DisableLink
                                  ? CreateRewritePath(homeTabId, cultureCode, action, RedirectReason.Disabled_Page)
                                  : CreateRewritePath(homeTabId, cultureCode, action, RedirectReason.Deleted_Page);
            }
            else
            {
                // for all other pages, rewrite to the correct tabId for that page
                // 592 : new permanentRedirect value
                if (permanentRedirect)
                {
                    rewritePath = CreateRewritePath(
                        tab.TabID,
                        cultureCode,
                        ActionType.Redirect301,
                        RedirectReason.Tab_Permanent_Redirect);
                }
                else
                {
                    // 852 : skin per alias at tab level - if specified
                    if (tab.AliasSkins != null && tab.AliasSkins.ContainsAlias(httpAlias))
                    {
                        TabAliasSkinInfo tas = tab.AliasSkins.FindByHttpAlias(httpAlias);
                        if (tas != null)
                        {
                            string skinSrc = tas.SkinSrc;
                            if (!string.IsNullOrEmpty(skinSrc))
                            {
                                // add skin src into rewrite path
                                rewritePath = CreateRewritePath(tab.TabID, cultureCode, "skinSrc=" + skinSrc);
                            }

                            // now add to the home page skin hashtable if it's the home page.
                            if (homeTabId == tab.TabID)
                            {
                                string key = httpAlias + "::" + cultureCode;
                                string key2 = httpAlias;
                                if (homePageSkins.ContainsKey(key) == false)
                                {
                                    homePageSkins.Add(key, skinSrc);
                                }

                                if (homePageSkins.ContainsKey(key2) == false)
                                {
                                    homePageSkins.Add(key2, skinSrc);
                                }
                            }
                        }
                    }
                    else
                    {
                        rewritePath = CreateRewritePath(tab.TabID, cultureCode);
                    }
                }

                if (thisPortal != null && (thisPortal.UserTabId == tab.TabID || thisPortal.UserTabId == tab.ParentId || thisPortal.UserTabId == -1))
                {
                    // user profile action specified.  If tabid match for this tab, add a do301 check because we want to make
                    // sure that the trimmed Url is used when appropriate
                    rewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                        rewritePath,
                        ActionType.CheckFor301,
                        RedirectReason.User_Profile_Url);
                }
            }

            if (tabPath.Replace("//", "/") != tab.TabPath.Replace("//", "/"))
            {
                // when the generated tab path is different to the standard tab path, character substituion has happened
                // this entry is going to have space substitution in it, so it is added into the dictionary with a delete notification and a 301 replaced
                // this entry is the 'original' (spaces removed) version ie mypage
                string substituteRewritePath = rewritePath;
                if (!isDeleted)

                // if it is deleted, we don't care if the spaces were replaced, or anything else, just take care in deleted handling
                {
                    string replaceSpaceWith = string.Empty;
                    if (settings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                    {
                        replaceSpaceWith = settings.ReplaceSpaceWith;
                    }

                    substituteRewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                        substituteRewritePath,
                        ActionType.Redirect301,
                        tabPath.Contains(replaceSpaceWith)
                                                                ? RedirectReason.Spaces_Replaced
                                                                : RedirectReason.Custom_Redirect);
                }

                // the preference variable determines what to do if a duplicate tab is found already in the dictionary
                var preference = UrlEnums.TabKeyPreference.TabRedirected;
                if (isDeleted)
                {
                    // if the tab is actually deleted, downgrade the preference to 'deleted'.  Any other tabs with the same path that
                    // are redirected but not deleted should take preference
                    preference = UrlEnums.TabKeyPreference.TabDeleted;
                }

                // Note ; if anything else is wrong with this url, (ie, wrong alias) then that will be corrected in a redirect
                AddToTabDict(
                    tabIndex,
                    dupCheck,
                    httpAlias,
                    tab.TabPath,
                    substituteRewritePath,
                    tab.TabID,
                    preference,
                    ref tabPathDepth,
                    settings.CheckForDuplicateUrls,
                    isDeleted);
            }

            // check for permanent redirects as specified in the core dnn permanent redirect property
            if (permanentRedirect)
            {
                AddPermanentRedirectToDictionary(
                    tabIndex,
                    dupCheck,
                    httpAlias,
                    tab,
                    tabPath,
                    ref rewritePath,
                    ref tabPathDepth,
                    settings.CheckForDuplicateUrls,
                    isDeleted);
            }

            // disabled / not active by date / external url pages cannot navigate to settings page
            if (tab.DisableLink || !string.IsNullOrEmpty(tab.Url) ||
               (tab.EndDate < DateTime.Now && tab.EndDate > DateTime.MinValue) ||
               (tab.StartDate > DateTime.Now && tab.StartDate > DateTime.MinValue))
            {
                string settingsUrl = tabPath.Replace("//", "/") + "/ctl/Tab";
                string settingsRewritePath = CreateRewritePath(tab.TabID, string.Empty, "ctl=tab");
                AddToTabDict(
                    tabIndex,
                    dupCheck,
                    httpAlias,
                    settingsUrl,
                    settingsRewritePath,
                    tab.TabID,
                    UrlEnums.TabKeyPreference.TabRedirected,
                    ref tabPathDepth,
                    settings.CheckForDuplicateUrls,
                    isDeleted);
            }

            // 777: every tab is added to the dictionary, including those that are deleted

            // inspect the optional tab redirects and add them as well, keeping track if any are '200' status, meaning the standard Url will be 301, if replaced unfriendly is switched on
            // 589 : tab with custom 200 redirects not changing base url to 301 statusa
            AddCustomRedirectsToDictionary(
                tabIndex,
                dupCheck,
                httpAlias,
                tab,
                settings,
                options,
                ref rewritePath,
                out tabPathDepth,
                ref customAliasesUsed,
                isDeleted,
                parentTraceId);

            // if auto ascii conversion is on, do that as well
            if (settings.AutoAsciiConvert)
            {
                bool replacedDiacritic;
                string asciiTabPath = TabPathHelper.ReplaceDiacritics(tabPath, out replacedDiacritic);
                if (replacedDiacritic)
                {
                    ActionType existingAction;
                    RedirectTokens.GetActionFromRewritePath(rewritePath, out existingAction);
                    if (settings.RedirectUnfriendly && existingAction != ActionType.Redirect301)
                    {
                        // add in a tab path, with 301, for the version with the diacritics in
                        string diacriticRewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                            rewritePath,
                            ActionType.Redirect301,
                            RedirectReason.Diacritic_Characters);
                        AddToTabDict(
                            tabIndex,
                            dupCheck,
                            httpAlias,
                            tabPath,
                            diacriticRewritePath,
                            tab.TabID,
                            UrlEnums.TabKeyPreference.TabOK,
                            ref tabPathDepth,
                            settings.CheckForDuplicateUrls,
                            isDeleted);
                    }
                    else
                    {
                        // add in the standard version so that the page responds to both the diacritic version
                        AddToTabDict(
                            tabIndex,
                            dupCheck,
                            httpAlias,
                            tabPath,
                            rewritePath,
                            tab.TabID,
                            UrlEnums.TabKeyPreference.TabOK,
                            ref tabPathDepth,
                            settings.CheckForDuplicateUrls,
                            isDeleted);
                    }
                }

                tabPath = asciiTabPath; // switch tabpath to new, ascii-converted version for rest of processing
            }

            // add the 'standard' Url in
            if (tab.TabID == homeTabId && settings.RedirectUnfriendly)
            {
                // home page shoudl be redirected back to the site root
                // 899: check for redirect on home page
                rewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                    rewritePath,
                    ActionType.CheckFor301,
                    RedirectReason.Site_Root_Home);
                AddToTabDict(
                    tabIndex,
                    dupCheck,
                    httpAlias,
                    tabPath,
                    rewritePath,
                    tab.TabID,
                    UrlEnums.TabKeyPreference.TabOK,
                    ref tabPathDepth,
                    settings.CheckForDuplicateUrls,
                    isDeleted);
            }
            else
            {
                if (customAliasUsedAndNotCurrent && settings.RedirectUnfriendly)
                {
                    // add in the standard page, but it's a redirect to the customAlias
                    rewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                        rewritePath,
                        ActionType.Redirect301,
                        RedirectReason.Custom_Tab_Alias);
                    AddToTabDict(
                        tabIndex,
                        dupCheck,
                        httpAlias,
                        tabPath,
                        rewritePath,
                        tab.TabID,
                        UrlEnums.TabKeyPreference.TabRedirected,
                        ref tabPathDepth,
                        settings.CheckForDuplicateUrls,
                        isDeleted);
                }
                else
                {
                    if (customAliasUsedAndNotCurrent && settings.RedirectUnfriendly)
                    {
                        // add in the standard page, but it's a redirect to the customAlias
                        rewritePath = RedirectTokens.AddRedirectReasonToRewritePath(
                            rewritePath,
                            ActionType.Redirect301,
                            RedirectReason.Custom_Tab_Alias);
                        AddToTabDict(
                            tabIndex,
                            dupCheck,
                            httpAlias,
                            tabPath,
                            rewritePath,
                            tab.TabID,
                            UrlEnums.TabKeyPreference.TabRedirected,
                            ref tabPathDepth,
                            settings.CheckForDuplicateUrls,
                            isDeleted);
                    }
                    else
                    {
                        // add in the standard page to the dictionary
                        // 931 : don't replace existing custom url if this is a redirect or a check for redirect
                        ActionType action;
                        var dupCheckPreference = UrlEnums.TabKeyPreference.TabOK;
                        RedirectTokens.GetActionFromRewritePath(rewritePath, out action);
                        if (action == ActionType.CheckFor301 || action == ActionType.Redirect301 || action == ActionType.Redirect302)
                        {
                            dupCheckPreference = UrlEnums.TabKeyPreference.TabRedirected;
                        }

                        AddToTabDict(
                            tabIndex,
                            dupCheck,
                            httpAlias,
                            tabPath,
                            rewritePath,
                            tab.TabID,
                            dupCheckPreference,
                            ref tabPathDepth,
                            settings.CheckForDuplicateUrls,
                            isDeleted);
                    }
                }
            }

            return tabPathDepth;
        }

        private static void AddToTabDict(
            SharedDictionary<string, string> tabIndex,
            Dictionary<string, DupKeyCheck> dupCheckDict,
            string httpAlias,
            string tabPath,
            string rewrittenPath,
            int tabId,
            UrlEnums.TabKeyPreference keyDupAction,
            ref int tabPathDepth,
            bool checkForDupUrls,
            bool isDeleted)
        {
            // remove leading '/' and convert to lower for all keys
            string tabPathSimple = tabPath.Replace("//", "/").ToLowerInvariant();

            // the tabpath depth is only set if it's higher than the running highest tab path depth
            int thisTabPathDepth = tabPathSimple.Length - tabPathSimple.Replace("/", string.Empty).Length;
            if (thisTabPathDepth > tabPathDepth)
            {
                tabPathDepth = thisTabPathDepth;
            }

            if (tabPathSimple.Length > 0 && tabPathSimple[0] == '/')
            {
                tabPathSimple = tabPathSimple.Substring(1);
            }

            // Contruct the tab key for the dictionary. Using :: allows for separation of portal alias and tab path.
            string tabKey = (httpAlias + "::" + tabPathSimple).ToLowerInvariant();

            // construct the duplicate key check
            string dupKey = (httpAlias + "/" + tabPathSimple).ToLowerInvariant();
            if (dupKey[dupKey.Length - 1] != '/')
            {
                dupKey += "/";
            }

            // now make sure there is NEVER a duplicate key exception by testing for existence first
            using (tabIndex.GetWriteLock())
            {
                if (tabIndex.ContainsKey(tabKey))
                {
                    // it's possible for a tab to be deleted and the tab path repeated.
                    // the dictionary must be checked to ascertain whether the existing tab
                    // should be replaced or not.  If the action is 'TabOK' it means
                    // replace the entry regardless.  If the action is 'TabRedirected' it means
                    // replace the existing dictionary ONLY if the existing dictionary entry is a
                    // deleted tab.
                    bool replaceTab = keyDupAction == UrlEnums.TabKeyPreference.TabOK; // default, replace the tab
                    if (replaceTab == false)
                    {
                        // ok, the tab to be added is either a redirected or deleted tab
                        // get the existing entry
                        // 775 : don't assume that the duplicate check dictionary has the key
                        if (dupCheckDict.ContainsKey(dupKey))
                        {
                            DupKeyCheck foundTab = dupCheckDict[dupKey];

                            // a redirected tab will replace a deleted tab
                            if (foundTab.IsDeleted && keyDupAction == UrlEnums.TabKeyPreference.TabRedirected)
                            {
                                replaceTab = true;
                            }

                            if (foundTab.TabIdOriginal == "-1")
                            {
                                replaceTab = true;
                            }
                        }
                    }

                    if (replaceTab && !isDeleted) // don't replace if the incoming tab is deleted
                    {
                        // remove the previous one
                        tabIndex.Remove(tabKey);

                        // add the new one
                        tabIndex.Add(tabKey, Globals.glbDefaultPage + rewrittenPath);
                    }
                }
                else
                {
                    // just add the tabkey into the dictionary
                    tabIndex.Add(tabKey, Globals.glbDefaultPage + rewrittenPath);
                }
            }

            // checking for duplicates means throwing an exception when one is found, but this is just logged to the event log
            if (dupCheckDict.ContainsKey(dupKey))
            {
                DupKeyCheck foundTAb = dupCheckDict[dupKey];
                if ((foundTAb.IsDeleted == false && isDeleted == false) // found is not deleted, this tab is not deleted
                    && keyDupAction == UrlEnums.TabKeyPreference.TabOK
                    && foundTAb.TabIdOriginal != "-1")

                // -1 tabs are login, register, privacy etc
                {
                    // check whether to log for this or not
                    if (checkForDupUrls && foundTAb.TabIdOriginal != tabId.ToString())

                    // dont' show message for where same tab is being added twice)
                    {
                        // there is a naming conflict where this alias/tab path could be mistaken
                        int tabIdOriginal;
                        string tab1Name = string.Empty, tab2Name = string.Empty;
                        var dupInSameCulture = false;
                        if (int.TryParse(foundTAb.TabIdOriginal, out tabIdOriginal))
                        {
                            Dictionary<int, int> portalDic = PortalController.GetPortalDictionary();
                            int portalId = -1;
                            if (portalDic != null && portalDic.ContainsKey(tabId))
                            {
                                portalId = portalDic[tabId];
                            }

                            TabInfo tab1 = TabController.Instance.GetTab(tabIdOriginal, portalId, false);
                            TabInfo tab2 = TabController.Instance.GetTab(tabId, portalId, false);
                            if (tab1 != null)
                            {
                                tab1Name = tab1.TabName + " [" + tab1.TabPath + "]";
                            }

                            if (tab2 != null)
                            {
                                tab2Name = tab2.TabName + " [" + tab2.TabPath + "]";
                            }

                            if (tab1 != null && tab2 != null)
                            {
                                dupInSameCulture = !PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", portalId, false)
                                                    || tab1.CultureCode == tab2.CultureCode;
                            }
                        }

                        if (dupInSameCulture)
                        {
                            string msg = "Page naming conflict. Url of (" + foundTAb.TabPath +
                                         ") resolves to two separate pages (" + tab1Name + " [tabid = " +
                                         foundTAb.TabIdOriginal + "], " + tab2Name + " [tabid = " + tabId.ToString() +
                                         "]). Only the second page will be shown for the url.";
                            const string msg2 =
                                "PLEASE NOTE : this is an information message only, this message does not affect site operations in any way.";

                            // 771 : change to admin alert instead of exception
                            // log a host alert
                            var log = new LogInfo { LogTypeKey = "HOST_ALERT" };
                            log.AddProperty("Advanced Friendly URL Provider Duplicate URL Warning", "Page Naming Conflict");
                            log.AddProperty("Duplicate Page Details", msg);
                            log.AddProperty("Warning Information", msg2);
                            log.AddProperty("Suggested Action", "Rename one or both of the pages to ensure a unique URL");
                            log.AddProperty(
                                "Hide this message",
                                "To stop this message from appearing in the log, uncheck the option for 'Produce an Exception in the Site Log if two pages have the same name/path?' in the Advanced Url Rewriting settings.");
                            log.AddProperty("Thread Id", Thread.CurrentThread.ManagedThreadId.ToString());
                            LogController.Instance.AddLog(log);
                        }
                    }
                }
                else
                {
                    dupCheckDict.Remove(dupKey);

                    // add this tab to the duplicate key dictionary
                    dupCheckDict.Add(dupKey, new DupKeyCheck(dupKey, tabId.ToString(), dupKey, isDeleted));
                }
            }
            else
            {
                // add this tab to the duplicate key dictionary - the dup key check dict is always maintained
                // regardless of whether checking is employed or not
                dupCheckDict.Add(dupKey, new DupKeyCheck(dupKey, tabId.ToString(), dupKey, isDeleted));
            }
        }

        private static OrderedDictionary BuildPortalAliasesDictionary()
        {
            var aliases = PortalAliasController.Instance.GetPortalAliases();

            // create a new OrderedDictionary.  We use this because we
            // want to key by the correct regex pattern and return the
            // portalAlias that matches, and we want to preserve the
            // order of the items, such that the item with the most path separators (/)
            // is at the front of the list.
            var aliasList = new OrderedDictionary(aliases.Count);
            var pathLengths = new List<int>();
            foreach (string aliasKey in aliases.Keys)
            {
                PortalAliasInfo alias = aliases[aliasKey];

                // regex escape the portal alias for inclusion into a regex pattern
                string plainAlias = alias.HTTPAlias;
                var aliasesToAdd = new List<string> { plainAlias };

                // check for existence of www. version of domain, if it doesn't have a www.
                if (plainAlias.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (plainAlias.Length > 4)
                    {
                        string noWWWVersion = plainAlias.Substring(4);
                        if (!aliases.Contains(noWWWVersion))
                        {
                            // there is no no-www version of the alias
                            aliasesToAdd.Add(noWWWVersion);
                        }
                    }
                }
                else
                {
                    string wwwVersion = "www." + plainAlias;
                    if (!aliases.Contains(wwwVersion))
                    {
                        aliasesToAdd.Add(wwwVersion);
                    }
                }

                int count = 0;
                foreach (string aliasToAdd in aliasesToAdd)
                {
                    // set flag on object to know whether to redirect or not
                    count++;
                    var aliasObject = new PortalAliasInfo(alias) { Redirect = count != 1 };

                    // work out how many path separators there are in the portalAlias (ie myalias/mychild = 1 path)
                    int pathLength = plainAlias.Split('/').GetUpperBound(0);

                    // now work out where in the list we should put this portalAlias regex pattern
                    // the list is to be sorted so that those aliases with the most paths
                    // are at the front of the list : ie, they are tested first
                    int insertPoint = pathLengths.Count - 1;

                    // walk through the existing list of path lengths,
                    // and ascertain where in the list this one falls
                    // if they are all the same path length, then place them in portal alias order
                    for (int i = 0; i < pathLengths.Count; i++)
                    {
                        insertPoint = i;
                        if (pathLength > pathLengths[i])
                        {
                            // larger than this position, insert at this value
                            break;
                        }

                        insertPoint++; // next one along (if at end, means add)
                    }

                    if (pathLengths.Count > 0 && insertPoint <= pathLengths.Count - 1)
                    {
                        // put the new regex pattern into the correct position
                        aliasList.Insert(insertPoint, aliasToAdd, aliasObject);
                        pathLengths.Insert(insertPoint, pathLength);
                    }
                    else
                    {
                        // put the new regex pattern on the end of the list
                        aliasList.Add(aliasToAdd, aliasObject);
                        pathLengths.Add(pathLength);
                    }
                }
            }

            return aliasList;
        }

        private static SharedDictionary<string, string> BuildTabDictionary(
            out PathSizes pathSizes,
            FriendlyUrlSettings settings,
            int buildPortalId,
            SharedDictionary<string, string> tabIndex,
            out Hashtable homePageSkins,
            out SharedDictionary<string, string> portalTabPathDictionary,
            Guid parentTraceId)
        {
            if (tabIndex == null)
            {
                tabIndex = new SharedDictionary<string, string>();
            }

            homePageSkins = new Hashtable();
            pathSizes = new PathSizes { MinAliasDepth = 10, MinTabPathDepth = 10, MaxAliasDepth = 0, MaxTabPathDepth = 0 };

            portalTabPathDictionary = null;
            if (buildPortalId >= 0)
            {
                // dictioanry for storing the tab paths in
                portalTabPathDictionary = new SharedDictionary<string, string>();

                // init the duplicate key check dictionary - disposed after the tab dictionary is built
                var dupCheck = new Dictionary<string, DupKeyCheck>();

                // get the list of tabs for all portals
                // new for 2.0 : only get tabs by portal
                // 770 : keep track of custom alias tabs
                Dictionary<int, TabInfo> tabs = FriendlyUrlController.GetTabs(buildPortalId, false, settings);

                const bool hasSiteRootRedirect = true;

                /* for the requested build portal, add in the standard urls and special rules */

                // 735 : switch to custom method for getting portal
                PortalInfo thisPortal = CacheController.GetPortal(buildPortalId, true);
                List<PortalAliasInfo> chosenAliases;
                Dictionary<string, string> chosenAliasesCultures;
                var aliasSpecificCultures = new List<string>();
                var usingHttpAliases = new List<string>();
                var customHttpAliasesUsed = new List<string>();
                GetAliasFromSettings(buildPortalId, out chosenAliases, out chosenAliasesCultures);
                FriendlyUrlOptions options = UrlRewriterUtils.GetOptionsFromSettings(settings);

                // keep a list of cultures specific to an alias
                foreach (string culture in chosenAliasesCultures.Values.Where(culture => aliasSpecificCultures.Contains(culture) == false))
                {
                    aliasSpecificCultures.Add(culture);
                }

                // the home tabid of the portal - should be the home page for the default language (all others will get page path)
                int homeTabId = thisPortal.HomeTabId;

                // Add site root redirects
                AddSiteRootRedirects(pathSizes, tabIndex, chosenAliases, hasSiteRootRedirect, dupCheck, usingHttpAliases);

                // add in any internal aliases as valid aliase
                AddInternalAliases(settings, usingHttpAliases);

                // loop through each tab and add all of the various Url paths that the tab can be found with,
                // for all aliases the tab will be used with
                foreach (TabInfo tab in tabs.Values)
                {
                    int tabPathDepth = 0;

                    // 935 : get the tab path and add to the tab path dictionary if it's not just a straight conversion of the TabPath value
                    // bool modified;
                    string tabPath = TabPathHelper.GetFriendlyUrlTabPath(tab, options, parentTraceId);
                    string tabKey = tab.TabID.ToString();

                    using (portalTabPathDictionary.GetWriteLock())
                    {
                        if (portalTabPathDictionary.ContainsKey(tabKey) == false)
                        {
                            portalTabPathDictionary.Add(tabKey, tabPath);
                        }
                    }

                    // now, go through the list of tabs for this portal and build up the dictionary
                    if ((settings.FriendlyAdminHostUrls && tab.PortalID == -1) || tab.PortalID == buildPortalId)
                    {
                        // check if this value has been excluded from being a friendly url
                        bool isExcluded = RewriteController.IsExcludedFromFriendlyUrls(tab, settings, true);
                        string tabCulture = tab.CultureCode;

                        // 770 : custom alias per tab (and culture)
                        bool customAliasUsed;
                        var customHttpAlias = ManageCustomAliases(
                            tabCulture,
                            thisPortal,
                            tab,
                            usingHttpAliases,
                            customHttpAliasesUsed,
                            out customAliasUsed);

                        // process each entry for the alias
                        foreach (string httpAlias in usingHttpAliases)
                        {
                            // string httpAlias = portalAlias.HTTPAlias;
                            // 761 : allow duplicate tab paths between culture-specific aliases
                            // this is done by ascertaining which culture a particular alias belongs to
                            // then checking tab cultures as they are added to the dictionary
                            string aliasCulture = string.Empty;
                            if (chosenAliasesCultures.ContainsKey(httpAlias.ToLowerInvariant()))
                            {
                                aliasCulture = chosenAliasesCultures[httpAlias.ToLowerInvariant()];
                            }

                            bool ignoreTabWrongCulture = false;

                            // the tab is the wrong culture, so don't add it to the dictionary
                            if (aliasCulture != string.Empty)
                            {
                                if (tabCulture != aliasCulture

                                    // this is a language-specific alias that's different to the culture for this alias
                                    && !string.IsNullOrEmpty(tabCulture) // and the tab culture is set
                                    && aliasSpecificCultures.Contains(tabCulture))

                                // and there is a specific alias for this tab culture
                                {
                                    ignoreTabWrongCulture = true;
                                }
                            }

                            if (!ignoreTabWrongCulture)
                            {
                                if (!isExcluded)
                                {
                                    // Add this tab to the dictionary
                                    // 750 : user profile action not returned as buildPortalId not used
                                    tabPathDepth = AddTabToTabDict(
                                        tabIndex,
                                        dupCheck,
                                        httpAlias,
                                        aliasCulture,
                                        customHttpAlias,
                                        thisPortal,
                                        tabPath,
                                        ref customHttpAliasesUsed,
                                        tab,
                                        settings,
                                        options,
                                        homeTabId,
                                        ref homePageSkins,
                                        parentTraceId);
                                }
                                else
                                {
                                    // 589 : custom redirects added as 200 status not causing base urls to redirect
                                    bool excludeFriendlyUrls = true;

                                    // 549 : detect excluded friendly urls by putting a known pattern into the dictionary
                                    // add this tab to the dictionary, but with the hack pattern [UseBase] to capture the fact it's a base Url
                                    // then, if there's redirects for it, add those as well.  It's possible to exclude a tab from friendly urls, but
                                    // give it custom redirects
                                    string rewritePath = null;
                                    if (tab.TabUrls.Count > 0)
                                    {
                                        rewritePath = CreateRewritePath(tab.TabID, string.Empty);
                                        string rewritePathKeep = rewritePath; // remember this value to compare
                                        AddCustomRedirectsToDictionary(
                                            tabIndex,
                                            dupCheck,
                                            httpAlias,
                                            tab,
                                            settings,
                                            options,
                                            ref rewritePath,
                                            out tabPathDepth,
                                            ref customHttpAliasesUsed,
                                            tab.IsDeleted,
                                            parentTraceId);
                                        if (rewritePath != rewritePathKeep)

                                        // check to see the rewrite path is still the same, or did it get changed?
                                        {
                                            // OK, the rewrite path was modifed by the custom redirects dictionary add
                                            excludeFriendlyUrls = false;
                                        }
                                    }

                                    if (excludeFriendlyUrls)
                                    {
                                        rewritePath = "[UseBase]";

                                        // use hack pattern to indicate not to rewrite on this Url
                                    }

                                    AddToTabDict(
                                        tabIndex,
                                        dupCheck,
                                        httpAlias,
                                        tab.TabPath,
                                        rewritePath,
                                        tab.TabID,
                                        UrlEnums.TabKeyPreference.TabRedirected,
                                        ref tabPathDepth,
                                        true,
                                        false);
                                }
                            }
                            else
                            {
                                // ignoring this tab because the alias culture doesn't match to the tab culture
                                // however, we need to add it to the dictionary in case there's an old link (pre-translation/pre-friendly url/pre-alias&culture linked)
                                string rewritePath = CreateRewritePath(tab.TabID, tabCulture);
                                AddToTabDict(
                                    tabIndex,
                                    dupCheck,
                                    httpAlias,
                                    tab.TabPath,
                                    rewritePath,
                                    tab.TabID,
                                    UrlEnums.TabKeyPreference.TabRedirected,
                                    ref tabPathDepth,
                                    true,
                                    tab.IsDeleted);
                            }

                            pathSizes.SetTabPathDepth(tabPathDepth);
                        }

                        if (customHttpAlias != string.Empty && customAliasUsed == false &&
                            usingHttpAliases.Contains(customHttpAlias))
                        {
                            // this was using a custom Http Alias, so remove this from the using list if it wasn't already there
                            usingHttpAliases.Remove(customHttpAlias);
                        }
                    }
                }

                // now build the standard Urls for all of the aliases that are used
                foreach (string httpAlias in usingHttpAliases)
                {
                    // 750 : using -1 instead of buildPortalId
                    // 850 : set culture code based on httpALias, where specific culture
                    // is being associated with httpAlias
                    string cultureCode = null;
                    if (chosenAliasesCultures.ContainsKey(httpAlias))
                    {
                        cultureCode = chosenAliasesCultures[httpAlias];
                    }

                    AddStandardPagesToDict(tabIndex, dupCheck, httpAlias, buildPortalId, cultureCode);
                }

                // and for any custom urls being used
                foreach (string httpAlias in customHttpAliasesUsed)
                {
                    // 750 : using -1 instead of buildPortalId
                    // is being associated with httpAlias
                    string cultureCode = null;
                    if (chosenAliasesCultures.ContainsKey(httpAlias))
                    {
                        cultureCode = chosenAliasesCultures[httpAlias];
                    }

                    AddStandardPagesToDict(tabIndex, dupCheck, httpAlias, buildPortalId, cultureCode);

                    // if any site root, add those as well. So if any module providers or rules work
                    // on the custom http aliases, they will work as well.
                    if (hasSiteRootRedirect)
                    {
                        int tempPathDepth = 0;
                        AddToTabDict(
                            tabIndex,
                            dupCheck,
                            httpAlias,
                            "*",
                            string.Empty,
                            -1,
                            UrlEnums.TabKeyPreference.TabOK,
                            ref tempPathDepth,
                            false,
                            false);
                    }
                }

                // do a check of the rebuildData object, to see if there is anything we needed to add to the dictionary
                var rebuildData = (PageIndexData)DataCache.GetCache("rebuildData");
                if (rebuildData != null)
                {
                    // there was rebuild data stored so we could do things post-dictionary rebuild
                    if (rebuildData.LastPageKey != null && rebuildData.LastPageValue != null)
                    {
                        if (tabIndex.ContainsKey(rebuildData.LastPageKey) == false)
                        {
                            // add this item to the list of pages, even though it no longer exists
                            tabIndex.Add(rebuildData.LastPageKey, rebuildData.LastPageValue);
                        }
                    }

                    // now clear out the rebuildData object, because we've checked and used it
                    DataCache.RemoveCache("rebuildData");
                }
            }

            return tabIndex;
        }

        private static SharedDictionary<string, string> FetchTabPathDictionary(int portalId)
        {
            SharedDictionary<string, string> tabPathDict;
            lock (tabPathDictBuildLock)
            {
                tabPathDict = CacheController.GetTabPathsFromCache(portalId);
            }

            return tabPathDict;
        }

        private static void GetAliasFromSettings(
            int portalId,
            out List<PortalAliasInfo> useAliases,
            out Dictionary<string, string> aliasCultures)
        {
            useAliases = new List<PortalAliasInfo>();
            aliasCultures = new Dictionary<string, string>();

            // 761 : return list of chosen aliases as well, so that Urls can be d
            var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).ToList();

            // list of portal aliases for this portal
            List<string> chosenAliases = null;
            var primaryAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).ToList();
            if (primaryAliases.Count > 0)
            {
                chosenAliases = primaryAliases.GetAliasesForPortalId(portalId);
                aliasCultures = primaryAliases.GetAliasesAndCulturesForPortalId(portalId);
            }

            if (chosenAliases != null && chosenAliases.Count > 0)
            {
                // add the chosen alias in based on the usePortalAlias setting
                useAliases.AddRange(aliases.Where(a => chosenAliases.Contains(a.HTTPAlias.ToLowerInvariant())));
            }

            if (useAliases.Count == 0)
            {
                // nothing found to match,
                // add all aliases in
                useAliases.AddRange(aliases);
            }
        }

        private static string ManageCustomAliases(
            string tabCulture,
            PortalInfo thisPortal,
            TabInfo tab,
            List<string> httpAliases,
            List<string> customHttpAliasesUsed,
            out bool customAliasUsed)
        {
            string customHttpAlias = string.Empty;
            string currentCulture = tabCulture;
            if (string.IsNullOrEmpty(tabCulture))
            {
                currentCulture = thisPortal.DefaultLanguage;
            }

            if (tab.CustomAliases.ContainsKey(currentCulture))
            {
                customHttpAlias = tab.CustomAliases[currentCulture].ToLowerInvariant();
            }

            customAliasUsed = httpAliases.Contains(customHttpAlias);

            // if there is a custom alias for this tab, and it's not one of the ones in the alias list, put it in
            // so that this tab will be put into the dictionary with not only the standard alias(es) but also
            // the custom alias.  Other logic will decide if to redirect the 'wrong' alias if requested with this tab.
            if (customAliasUsed == false && customHttpAlias != string.Empty)
            {
                httpAliases.Add(customHttpAlias);
                if (customHttpAliasesUsed.Contains(customHttpAlias) == false)
                {
                    customHttpAliasesUsed.Add(customHttpAlias);
                }
            }

            return customHttpAlias;
        }

        /// <summary>
        /// Returns whether the portal specified exists in the Tab index or not.
        /// </summary>
        /// <param name="portalDepths">The current portalDepths dictionary.</param>
        /// <param name="portalId">The id of the portal to search for.</param>
        /// <returns></returns>
        private static bool PortalExistsInIndex(SharedDictionary<int, PathSizes> portalDepths, int portalId)
        {
            bool result = false;

            if (portalDepths != null)
            {
                using (portalDepths.GetReadLock())
                {
                    result = portalDepths.ContainsKey(portalId);
                }
            }

            return result;
        }
    }
}
