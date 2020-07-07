// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Log.EventLog;

    using Assembly = System.Reflection.Assembly;

    public class ExtensionUrlProviderController
    {
        private static readonly object providersBuildLock = new object();

        private static readonly Regex RewrittenUrlRegex = new Regex(
            @"(?<tabid>(?:\?|&)tabid=\d+)(?<qs>&[^=]+=[^&]*)*",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static void DeleteProvider(ExtensionUrlProviderInfo urlProvider)
        {
            DataProvider.Instance().DeleteExtensionUrlProvider(urlProvider.ExtensionUrlProviderId);
            ClearCache();
        }

        public static void DisableProvider(int providerId, int portalId)
        {
            DataProvider.Instance().UpdateExtensionUrlProvider(providerId, false);
            ClearCache(portalId);
        }

        public static void EnableProvider(int providerId, int portalId)
        {
            DataProvider.Instance().UpdateExtensionUrlProvider(providerId, true);
            ClearCache(portalId);
        }

        public static List<ExtensionUrlProviderInfo> GetProviders(int portalId)
        {
            return CBO.FillCollection<ExtensionUrlProviderInfo>(DataProvider.Instance().GetExtensionUrlProviders(portalId));
        }

        /// <summary>
        /// Loads the module providers.
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        /// <remarks>Note : similar copy for UI purposes in ConfigurationController.cs.</remarks>
        public static List<ExtensionUrlProvider> GetModuleProviders(int portalId)
        {
            var cacheKey = string.Format("ExtensionUrlProviders_{0}", portalId);
            var moduleProviders = CBO.GetCachedObject<List<ExtensionUrlProvider>>(
                new CacheItemArgs(
                cacheKey,
                60,
                CacheItemPriority.High,
                portalId),
                c =>
                                    {
                                        var id = (int)c.Params[0];
                                        IDataReader dr = DataProvider.Instance().GetExtensionUrlProviders(id);
                                        try
                                        {
                                            var providers = new List<ExtensionUrlProvider>();
                                            var providerConfigs = CBO.FillCollection(dr, new List<ExtensionUrlProviderInfo>(), false);

                                            foreach (var providerConfig in providerConfigs)
                                            {
                                                var providerType = Reflection.CreateType(providerConfig.ProviderType);
                                                if (providerType == null)
                                                {
                                                    continue;
                                                }

                                                var provider = Reflection.CreateObject(providerType) as ExtensionUrlProvider;
                                                if (provider == null)
                                                {
                                                    continue;
                                                }

                                                provider.ProviderConfig = providerConfig;
                                                provider.ProviderConfig.PortalId = id;
                                                providers.Add(provider);
                                            }

                                            if (dr.NextResult())
                                            {
                                                // Setup Settings
                                                while (dr.Read())
                                                {
                                                    var extensionUrlProviderId = Null.SetNullInteger(dr["ExtensionUrlProviderID"]);
                                                    var key = Null.SetNullString(dr["SettingName"]);
                                                    var value = Null.SetNullString(dr["SettingValue"]);

                                                    var provider = providers.SingleOrDefault(p => p.ProviderConfig.ExtensionUrlProviderId == extensionUrlProviderId);
                                                    if (provider != null)
                                                    {
                                                        provider.ProviderConfig.Settings[key] = value;
                                                    }
                                                }
                                            }

                                            if (dr.NextResult())
                                            {
                                                // Setup Tabs
                                                while (dr.Read())
                                                {
                                                    var extensionUrlProviderId = Null.SetNullInteger(dr["ExtensionUrlProviderID"]);
                                                    var tabId = Null.SetNullInteger(dr["TabID"]);

                                                    var provider = providers.SingleOrDefault(p => p.ProviderConfig.ExtensionUrlProviderId == extensionUrlProviderId);
                                                    if (provider != null && !provider.ProviderConfig.TabIds.Contains(tabId))
                                                    {
                                                        provider.ProviderConfig.TabIds.Add(tabId);
                                                    }
                                                }
                                            }

                                            return providers;
                                        }
                                        finally
                                        {
                                            // Close reader
                                            CBO.CloseDataReader(dr, true);
                                        }
                                    });

            return moduleProviders;
        }

        public static FriendlyUrlOptions GetOptionsFromSettings(FriendlyUrlSettings settings)
        {
            return new FriendlyUrlOptions
            {
                PunctuationReplacement = (settings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                                                ? settings.ReplaceSpaceWith
                                                : string.Empty,
                SpaceEncoding = settings.SpaceEncodingValue,
                MaxUrlPathLength = 200,
                ConvertDiacriticChars = settings.AutoAsciiConvert,
                RegexMatch = settings.RegexMatch,
                IllegalChars = settings.IllegalChars,
                ReplaceChars = settings.ReplaceChars,
                ReplaceDoubleChars = settings.ReplaceDoubleChars,
                ReplaceCharWithChar = settings.ReplaceCharacterDictionary,
                PageExtension = settings.PageExtensionUsageType == PageExtensionUsageType.Never ? string.Empty : settings.PageExtension,
            };
        }

        /// <summary>
        /// logs an exception related to a module provider once per cache-lifetime.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="status"></param>
        /// <param name="result"></param>
        /// <param name="messages"></param>
        /// <param name="provider"></param>
        public static void LogModuleProviderExceptionInRequest(Exception ex, string status,
                                                                ExtensionUrlProvider provider,
                                                                UrlAction result,
                                                                List<string> messages)
        {
            if (ex != null)
            {
                string moduleProviderName = "Unknown Provider";
                string moduleProviderVersion = "Unknown Version";
                if (provider != null)
                {
                    moduleProviderName = provider.ProviderConfig.ProviderName;
                    moduleProviderVersion = provider.GetType().Assembly.GetName(false).Version.ToString();
                }

                // this logic prevents a site logging an exception for every request made.  Instead
                // the exception will be logged once for the life of the cache / application restart or 1 hour, whichever is shorter.
                // create a cache key for this exception type
                string cacheKey = ex.GetType().ToString();

                // see if there is an existing object logged for this exception type
                object existingEx = DataCache.GetCache(cacheKey);
                if (existingEx == null)
                {
                    // if there was no existing object logged for this exception type, this is a new exception
                    DateTime expire = DateTime.Now.AddHours(1);
                    DataCache.SetCache(cacheKey, cacheKey, expire);

                    // just store the cache key - it doesn't really matter
                    // create a log event
                    string productVer = DotNetNukeContext.Current.Application.Version.ToString();
                    var log = new LogInfo { LogTypeKey = "GENERAL_EXCEPTION" };
                    log.AddProperty(
                        "Url Rewriting Extension Url Provider Exception",
                        "Exception in Url Rewriting Process");
                    log.AddProperty("Provider Name", moduleProviderName);
                    log.AddProperty("Provider Version", moduleProviderVersion);
                    log.AddProperty("Http Status", status);
                    log.AddProperty("Product Version", productVer);
                    if (result != null)
                    {
                        log.AddProperty("Original Path", result.OriginalPath ?? "null");
                        log.AddProperty("Raw Url", result.RawUrl ?? "null");
                        log.AddProperty("Final Url", result.FinalUrl ?? "null");

                        log.AddProperty("Rewrite Result", !string.IsNullOrEmpty(result.RewritePath)
                                                                     ? result.RewritePath
                                                                     : "[no rewrite]");
                        log.AddProperty("Redirect Location", string.IsNullOrEmpty(result.FinalUrl)
                                                                    ? "[no redirect]"
                                                                    : result.FinalUrl);
                        log.AddProperty("Action", result.Action.ToString());
                        log.AddProperty("Reason", result.Reason.ToString());
                        log.AddProperty("Portal Id", result.PortalId.ToString());
                        log.AddProperty("Tab Id", result.TabId.ToString());
                        log.AddProperty("Http Alias", result.PortalAlias != null ? result.PortalAlias.HTTPAlias : "Null");

                        if (result.DebugMessages != null)
                        {
                            int i = 1;
                            foreach (string debugMessage in result.DebugMessages)
                            {
                                string msg = debugMessage;
                                if (debugMessage == null)
                                {
                                    msg = "[message was null]";
                                }

                                log.AddProperty("Debug Message[result] " + i.ToString(), msg);
                                i++;
                            }
                        }
                    }
                    else
                    {
                        log.AddProperty("Result", "Result value null");
                    }

                    if (messages != null)
                    {
                        int i = 1;
                        foreach (string msg in messages)
                        {
                            log.AddProperty("Debug Message[raw] " + i.ToString(), msg);
                            i++;
                        }
                    }

                    log.AddProperty("Exception Type", ex.GetType().ToString());
                    log.AddProperty("Message", ex.Message);
                    log.AddProperty("Stack Trace", ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        log.AddProperty("Inner Exception Message", ex.InnerException.Message);
                        log.AddProperty("Inner Exception Stacktrace", ex.InnerException.StackTrace);
                    }

                    log.BypassBuffering = true;
                    LogController.Instance.AddLog(log);
                }
            }
        }

        public static void SaveProvider(ExtensionUrlProviderInfo provider)
        {
            provider.ExtensionUrlProviderId = DataProvider.Instance().AddExtensionUrlProvider(
                                                    provider.ExtensionUrlProviderId,
                                                    provider.DesktopModuleId,
                                                    provider.ProviderName,
                                                    provider.ProviderType,
                                                    provider.SettingsControlSrc,
                                                    provider.IsActive,
                                                    provider.RewriteAllUrls,
                                                    provider.RedirectAllUrls,
                                                    provider.ReplaceAllUrls);
        }

        public static void SaveSetting(int providerId, int portalId, string settingName, string settingValue)
        {
            DataProvider.Instance().SaveExtensionUrlProviderSetting(providerId, portalId, settingName, settingValue);
            ClearCache(portalId);
        }

        /// <summary>
        /// Checks to see if any providers are marked as 'always call for rewrites'.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        /// <param name="settings"></param>
        /// <param name="parentTraceId"></param>
        /// <returns></returns>
        internal static bool CheckForAlwaysCallProviders(int portalId, int tabId, FriendlyUrlSettings settings, Guid parentTraceId)
        {
            List<int> alwaysCallTabids = CacheController.GetAlwaysCallProviderTabs(portalId);
            bool checkForAlwaysCallResult = false;
            if (alwaysCallTabids == null)
            {
                alwaysCallTabids = new List<int>(); // create new list

                // nothing in cache, build list
                List<ExtensionUrlProvider> providers = GetModuleProviders(portalId).Where(p => p.ProviderConfig.IsActive).ToList();
                foreach (ExtensionUrlProvider provider in providers)
                {
                    // check the always call property
                    if (provider.AlwaysCallForRewrite(portalId))
                    {
                        if (provider.ProviderConfig.AllTabs)
                        {
                            if (alwaysCallTabids.Contains(RewriteController.AllTabsRewrite) == false)
                            {
                                alwaysCallTabids.Add(RewriteController.AllTabsRewrite);
                            }
                        }
                        else
                        {
                            foreach (int providerTabId in provider.ProviderConfig.TabIds)
                            {
                                if (alwaysCallTabids.Contains(tabId) == false)
                                {
                                    alwaysCallTabids.Add(providerTabId);
                                }
                            }
                        }
                    }
                }

                // now store back in cache
                CacheController.StoreAlwaysCallProviderTabs(portalId, alwaysCallTabids, settings);
            }

            if (alwaysCallTabids.Contains(tabId) || alwaysCallTabids.Contains(RewriteController.AllTabsRewrite))
            {
                checkForAlwaysCallResult = true;
            }

            return checkForAlwaysCallResult;
        }

        internal static bool CheckForRedirect(
            Uri requestUri,
            UrlAction result,
            NameValueCollection queryStringCol,
            FriendlyUrlSettings settings,
            out string location,
            ref List<string> messages,
            Guid parentTraceId)
        {
            bool redirected = false;
            location = string.Empty;
            ExtensionUrlProvider activeProvider = null;
            try
            {
                List<ExtensionUrlProvider> providersToCall = GetProvidersToCall(result.TabId, result.PortalId, settings,
                                                                             parentTraceId);
                if (providersToCall != null && providersToCall.Count > 0)
                {
                    FriendlyUrlOptions options = UrlRewriterUtils.GetOptionsFromSettings(settings);
                    foreach (ExtensionUrlProvider provider in providersToCall)
                    {
                        activeProvider = provider; // for error handling
                        redirected = provider.CheckForRedirect(result.TabId, result.PortalId, result.HttpAlias,
                                                               requestUri, queryStringCol, options, out location,
                                                               ref messages);
                        if (redirected)
                        {
                            result.FinalUrl = location;
                            result.Reason = RedirectReason.Module_Provider_Redirect;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // log module provider exception
                LogModuleProviderExceptionInRequest(ex, "500 Internal Server Error", activeProvider, result, messages);

                // return defaults
                redirected = false;
                location = string.Empty;
                string providerName = "Unknown";
                if (activeProvider != null)
                {
                    providerName = activeProvider.ProviderConfig.ProviderName;
                }

                if (result != null)
                {
                    result.DebugMessages.Add("Exception in provider [" + providerName + "] :" + ex.Message);
                }
            }

            return redirected;
        }

        /// <summary>
        /// Returns boolean value is any loaded providers require checking of rewrite / redirect values from the site root (ie, not dnn tab path).
        /// </summary>
        /// <returns></returns>
        internal static bool CheckForSiteRootRewrite(int portalId, FriendlyUrlSettings settings, Guid parentTraceId)
        {
            var providers = GetProvidersToCall(RewriteController.SiteRootRewrite, portalId, settings, parentTraceId);

            // list should have returned all providers with site root rewrite, but double check here in case of faulty third-party logic
            return providers.Any(provider => provider.AlwaysUsesDnnPagePath(portalId) == false);
        }

        internal static bool GetUrlFromExtensionUrlProviders(
            int portalId,
            TabInfo tab,
            FriendlyUrlSettings settings,
            string friendlyUrlPath,
            string cultureCode,
            ref string endingPageName,
            out string changedPath,
            out bool changeToSiteRoot,
            ref List<string> messages,
            Guid parentTraceId)
        {
            bool wasChanged = false;
            changedPath = friendlyUrlPath;
            changeToSiteRoot = false;
            ExtensionUrlProvider activeProvider = null;
            if (messages == null)
            {
                messages = new List<string>();
            }

            try
            {
                List<ExtensionUrlProvider> providersToCall = GetProvidersToCall(tab.TabID, portalId, settings,
                                                                             parentTraceId);
                FriendlyUrlOptions options = UrlRewriterUtils.GetOptionsFromSettings(settings);
                foreach (ExtensionUrlProvider provider in providersToCall)
                {
                    activeProvider = provider; // keep for exception purposes
                    bool useDnnPagePath;

                    // go through and call each provider to generate the friendly urls for the module
                    string customPath = provider.ChangeFriendlyUrl(
                        tab,
                        friendlyUrlPath,
                        options,
                        cultureCode,
                        ref endingPageName,
                        out useDnnPagePath,
                        ref messages);

                    if (string.IsNullOrEmpty(endingPageName))
                    {
                        endingPageName = Globals.glbDefaultPage; // set back to default.aspx if provider cleared it
                    }

                    // now check to see if a change was made or not.  Don't trust the provider.
                    if (!string.IsNullOrEmpty(customPath))
                    {
                        // was customPath any different to friendlyUrlPath?
                        if (string.CompareOrdinal(customPath, friendlyUrlPath) != 0)
                        {
                            wasChanged = true;
                            changedPath = customPath.Trim();
                            changeToSiteRoot = !useDnnPagePath; // useDNNpagePath means no change to site root.
                            const string format = "Path returned from {0} -> path:{1}, ending Page:{2}, use Page Path:{3}";
                            messages.Add(string.Format(format, provider.ProviderConfig.ProviderName, customPath, endingPageName, useDnnPagePath));
                            break; // first module provider to change the Url is the only one used
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogModuleProviderExceptionInRequest(ex, "500 Internal Server Error", activeProvider, null, messages);

                // reset all values to defaults
                wasChanged = false;
                changedPath = friendlyUrlPath;
                changeToSiteRoot = false;
            }

            return wasChanged;
        }

        internal static bool TransformFriendlyUrlPath(
            string newUrl,
            string tabKeyVal,
            string[] urlParms,
            bool isSiteRootMatch,
            ref UrlAction result,
            FriendlyUrlSettings settings,
            out string rewrittenUrl,
            out bool newAction,
            ref List<string> messages,
            Guid parentTraceId)
        {
            bool rewriteDone = false;
            rewrittenUrl = newUrl;
            newAction = false;
            ExtensionUrlProvider activeProvider = null;
            try
            {
                int tabId = result.TabId;
                if (isSiteRootMatch)
                {
                    tabId = RewriteController.SiteRootRewrite;
                }

                List<ExtensionUrlProvider> providersToCall = GetProvidersToCall(
                    tabId,
                    result.PortalId,
                    settings,
                    parentTraceId);
                if (providersToCall != null && providersToCall.Count > 0)
                {
                    // now check for providers by calling the providers
                    int upperBound = urlParms.GetUpperBound(0);

                    // clean extension off parameters array
                    var parms = new string[upperBound + 1];
                    Array.ConstrainedCopy(urlParms, 0, parms, 0, upperBound + 1);
                    if (upperBound >= 0)
                    {
                        bool replaced;
                        parms[upperBound] = RewriteController.CleanExtension(parms[upperBound], settings, out replaced);
                    }

                    // get options from current settings
                    FriendlyUrlOptions options = UrlRewriterUtils.GetOptionsFromSettings(settings);
                    foreach (ExtensionUrlProvider provider in providersToCall)
                    {
                        // set active provider for exception handling
                        activeProvider = provider;

                        // call down to specific providers and see if we get a rewrite
                        string location;
                        int status;
                        string queryString = provider.TransformFriendlyUrlToQueryString(
                            parms,
                            result.TabId,
                            result.PortalId,
                            options,
                            result.CultureCode,
                            result.PortalAlias,
                            ref messages,
                            out status,
                            out location);
                        if (status == 0 || status == 200) // either not set, or set to '200 OK'.
                        {
                            if (!string.IsNullOrEmpty(queryString) && queryString != newUrl)
                            {
                                rewriteDone = true;

                                // check for duplicate tabIds.
                                string qsRemainder = null;
                                if (Regex.IsMatch(queryString, @"tabid=\d+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                                {
                                    // 930 : look for other querystring information in the rewritten Url, or invalid rewritten urls can be created
                                    // pattern to determine which tab matches
                                    // look for any other querystirng information in the already rewritten Url (ie language parameters)
                                    Match rewrittenUrlMatch = RewrittenUrlRegex.Match(rewrittenUrl);
                                    if (rewrittenUrlMatch.Groups["qs"].Success)
                                    {
                                        // keep any other querystring remainders
                                        qsRemainder = rewrittenUrlMatch.Groups["qs"].Captures.Cast<Capture>().Aggregate(string.Empty, (current, qsCapture) => current + qsCapture.Value); // initialise
                                    }

                                    // supplied value overwrites existing value, so remove from the rewritten url
                                    rewrittenUrl = RewrittenUrlRegex.Replace(rewrittenUrl, string.Empty);
                                }

                                if (rewrittenUrl.Contains("?") == false)
                                {
                                    // use a leading ?, not a leading &
                                    queryString = FriendlyUrlController.EnsureNotLeadingChar("&", queryString);
                                    queryString = FriendlyUrlController.EnsureLeadingChar("?", queryString);
                                }
                                else
                                {
                                    // use a leading &, not a leading ?
                                    queryString = FriendlyUrlController.EnsureNotLeadingChar("?", queryString);
                                    queryString = FriendlyUrlController.EnsureLeadingChar("&", queryString);
                                }

                                // add querystring onto rewritten Url
                                rewrittenUrl += queryString;
                                if (qsRemainder != null)
                                {
                                    rewrittenUrl += qsRemainder;
                                }

                                break;
                            }
                        }
                        else
                        {
                            switch (status)
                            {
                                case 301:
                                    result.Action = ActionType.Redirect301;
                                    result.Reason = RedirectReason.Module_Provider_Rewrite_Redirect;
                                    result.FinalUrl = location;
                                    break;
                                case 302:
                                    result.Action = ActionType.Redirect302;
                                    result.Reason = RedirectReason.Module_Provider_Rewrite_Redirect;
                                    result.FinalUrl = location;
                                    break;
                                case 404:
                                    result.Action = ActionType.Output404;
                                    break;
                                case 500:
                                    result.Action = ActionType.Output500;
                                    break;
                            }

                            newAction = true; // not doing a 200 status
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // log module provider exception
                LogModuleProviderExceptionInRequest(ex, "500 Internal Server Error", activeProvider, result, messages);

                // reset values to initial
                rewriteDone = false;
                rewrittenUrl = newUrl;
                newAction = false;
                string providerName = "Unknown";
                if (activeProvider != null)
                {
                    providerName = activeProvider.ProviderConfig.ProviderName;
                }

                if (result != null)
                {
                    result.DebugMessages.Add("Exception in provider [" + providerName + "] :" + ex.Message);
                }
            }

            return rewriteDone;
        }

        private static void ClearCache()
        {
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                ClearCache(portal.PortalID);
            }
        }

        private static void ClearCache(int portalId)
        {
            var cacheKey = string.Format("ExtensionUrlProviders_{0}", portalId);
            DataCache.RemoveCache(cacheKey);
        }

        /// <summary>
        /// Returns the providers to call. Returns tabid matches first, and any portal id matches after that.
        /// </summary>
        /// <param name="tabId"></param>
        /// <param name="portalId"></param>
        /// <param name="settings"></param>
        /// <param name="parentTraceId"></param>
        /// <returns></returns>
        private static List<ExtensionUrlProvider> GetProvidersToCall(
            int tabId,
            int portalId,
            FriendlyUrlSettings settings,
            Guid parentTraceId)
        {
            List<ExtensionUrlProvider> providers;

            // 887 : introduce lockable code to prevent caching race errors
            lock (providersBuildLock)
            {
                bool definitelyNoProvider;

                // 887 : use cached list of tabs instead of per-tab cache of provider
                // get the list of providers to call based on the tab and the portal
                var providersToCall = CacheController.GetProvidersForTabAndPortal(
                    tabId,
                    portalId,
                    settings,
                    out definitelyNoProvider,
                    parentTraceId);
                if (definitelyNoProvider == false && providersToCall == null)

                // nothing in the cache, and we don't have a definitive 'no' that there isn't a provider
                {
                    // get all providers for the portal
                    var allProviders = GetModuleProviders(portalId).Where(p => p.ProviderConfig.IsActive).ToList();

                    // store the list of tabs for this portal that have a provider attached
                    CacheController.StoreListOfTabsWithProviders(allProviders, portalId, settings);

                    // stash the provider portals in the cache
                    CacheController.StoreModuleProvidersForPortal(portalId, settings, allProviders);

                    // now check if there is a provider for this tab/portal combination
                    if (allProviders.Count > 0)
                    {
                        // find if a module is specific to a tab
                        providersToCall = new List<ExtensionUrlProvider>();
                        providersToCall.AddRange(allProviders);
                    }
                }

                // always return an instantiated provider collection
                providers = providersToCall ?? new List<ExtensionUrlProvider>();
            }

            // return the collection of module providers
            return providers;
        }
    }
}
