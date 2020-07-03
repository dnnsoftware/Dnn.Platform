// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.OutputCaching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.OutputCache;

    /// <summary>
    /// Manages the output cache for a request.
    /// </summary>
    public class OutputCacheModule : IHttpModule
    {
        private const string ContextKeyResponseFilter = "OutputCache:ResponseFilter";
        private const string ContextKeyTabId = "OutputCache:TabId";
        private const string ContextKeyTabOutputCacheProvider = "OutputCache:TabOutputCacheProvider";
        private HttpApplication _app;

        private enum IncludeExcludeType
        {
            IncludeByDefault,
            ExcludeByDefault,
        }

        public void Init(HttpApplication httpApp)
        {
            this._app = httpApp;

            httpApp.ResolveRequestCache += this.OnResolveRequestCache;
            httpApp.UpdateRequestCache += this.OnUpdateRequestCache;
        }

        public void Dispose()
        {
        }

        private bool IsInstallInProgress(HttpApplication app)
        {
            return InstallBlocker.Instance.IsInstallInProgress();
        }

        private void OnResolveRequestCache(object sender, EventArgs e)
        {
            bool cached = false;
            if (this._app == null || this._app.Context == null || !this._app.Response.ContentType.Equals("text/html", StringComparison.InvariantCultureIgnoreCase) || this._app.Context.Request.IsAuthenticated || this._app.Context.Request.Browser.Crawler)
            {
                return;
            }

            if (this.IsInstallInProgress(this._app))
            {
                return;
            }

            if (this._app.Context.Request.RequestType == "POST" || !this._app.Context.Request.Url.LocalPath.EndsWith(Globals.glbDefaultPage, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            int tabId = portalSettings.ActiveTab.TabID;

            Hashtable tabSettings = TabController.Instance.GetTabSettings(tabId);

            if (tabSettings["CacheProvider"] == null || string.IsNullOrEmpty(tabSettings["CacheProvider"].ToString()))
            {
                return;
            }

            int portalId = portalSettings.PortalId;
            string locale = Localization.GetPageLocale(portalSettings).Name;

            IncludeExcludeType includeExclude = IncludeExcludeType.ExcludeByDefault;
            if (tabSettings["CacheIncludeExclude"] != null && !string.IsNullOrEmpty(tabSettings["CacheIncludeExclude"].ToString()))
            {
                if (tabSettings["CacheIncludeExclude"].ToString() == "0")
                {
                    includeExclude = IncludeExcludeType.ExcludeByDefault;
                }
                else
                {
                    includeExclude = IncludeExcludeType.IncludeByDefault;
                }
            }

            string tabOutputCacheProvider = tabSettings["CacheProvider"].ToString();
            this._app.Context.Items[ContextKeyTabOutputCacheProvider] = tabOutputCacheProvider;
            int maxCachedVariationsForTab = 250; // by default, prevent DOS attacks
            if (tabSettings["MaxVaryByCount"] != null && !string.IsNullOrEmpty(tabSettings["MaxVaryByCount"].ToString()))
            {
                maxCachedVariationsForTab = Convert.ToInt32(tabSettings["MaxVaryByCount"].ToString());
            }

            var includeVaryByKeys = new StringCollection();
            includeVaryByKeys.Add("ctl");
            includeVaryByKeys.Add("returnurl");
            includeVaryByKeys.Add("tabid");
            includeVaryByKeys.Add("portalid");
            includeVaryByKeys.Add("locale");
            includeVaryByKeys.Add("alias");

            // make sure to always add keys in lowercase only
            if (includeExclude == IncludeExcludeType.ExcludeByDefault)
            {
                string includeVaryByKeysSettings = string.Empty;
                if (tabSettings["IncludeVaryBy"] != null)
                {
                    includeVaryByKeysSettings = tabSettings["IncludeVaryBy"].ToString();
                }

                if (!string.IsNullOrEmpty(includeVaryByKeysSettings))
                {
                    if (includeVaryByKeysSettings.Contains(","))
                    {
                        string[] keys = includeVaryByKeysSettings.Split(',');
                        foreach (string key in keys)
                        {
                            includeVaryByKeys.Add(key.Trim().ToLowerInvariant());
                        }
                    }
                    else
                    {
                        includeVaryByKeys.Add(includeVaryByKeysSettings.Trim().ToLowerInvariant());
                    }
                }
            }

            var excludeVaryByKeys = new StringCollection();
            if (includeExclude == IncludeExcludeType.IncludeByDefault)
            {
                string excludeVaryByKeysSettings = string.Empty;
                if (tabSettings["ExcludeVaryBy"] != null)
                {
                    excludeVaryByKeysSettings = tabSettings["ExcludeVaryBy"].ToString();
                }

                if (!string.IsNullOrEmpty(excludeVaryByKeysSettings))
                {
                    if (excludeVaryByKeysSettings.Contains(","))
                    {
                        string[] keys = excludeVaryByKeysSettings.Split(',');
                        foreach (string key in keys)
                        {
                            excludeVaryByKeys.Add(key.Trim().ToLowerInvariant());
                        }
                    }
                    else
                    {
                        excludeVaryByKeys.Add(excludeVaryByKeysSettings.Trim().ToLowerInvariant());
                    }
                }
            }

            var varyBy = new SortedDictionary<string, string>();

            foreach (string key in this._app.Context.Request.QueryString)
            {
                if (key != null && this._app.Context.Request.QueryString[key] != null)
                {
                    var varyKey = key.ToLowerInvariant();
                    varyBy.Add(varyKey, this._app.Context.Request.QueryString[key]);

                    if (includeExclude == IncludeExcludeType.IncludeByDefault && !includeVaryByKeys.Contains(varyKey))
                    {
                        includeVaryByKeys.Add(varyKey);
                    }
                }
            }

            if (!varyBy.ContainsKey("portalid"))
            {
                varyBy.Add("portalid", portalId.ToString());
            }

            if (!varyBy.ContainsKey("tabid"))
            {
                varyBy.Add("tabid", tabId.ToString());
            }

            if (!varyBy.ContainsKey("locale"))
            {
                varyBy.Add("locale", locale);
            }

            if (!varyBy.ContainsKey("alias"))
            {
                varyBy.Add("alias", portalSettings.PortalAlias.HTTPAlias);
            }

            string cacheKey = OutputCachingProvider.Instance(tabOutputCacheProvider).GenerateCacheKey(tabId, includeVaryByKeys, excludeVaryByKeys, varyBy);

            bool returnedFromCache = OutputCachingProvider.Instance(tabOutputCacheProvider).StreamOutput(tabId, cacheKey, this._app.Context);

            if (returnedFromCache)
            {
                // output the content type heade when read content from cache.
                this._app.Context.Response.AddHeader("Content-Type", string.Format("{0}; charset={1}", this._app.Response.ContentType, this._app.Response.Charset));

                // This is to give a site owner the ability
                // to visually verify that a page was rendered via
                // the output cache.  Use FireFox FireBug or another
                // tool to view the response headers easily.
                this._app.Context.Response.AddHeader("DNNOutputCache", "true");

                // Also add it ti the Context - the Headers are readonly unless using IIS in Integrated Pipleine mode
                // and we need to know if OutPut Caching is active in the compression module
                this._app.Context.Items.Add("DNNOutputCache", "true");

                this._app.Context.Response.End();
                cached = true;
            }

            this._app.Context.Items[ContextKeyTabId] = tabId;

            if (cached != true)
            {
                if (tabSettings["CacheDuration"] != null && !string.IsNullOrEmpty(tabSettings["CacheDuration"].ToString()) && Convert.ToInt32(tabSettings["CacheDuration"].ToString()) > 0)
                {
                    int seconds = Convert.ToInt32(tabSettings["CacheDuration"].ToString());
                    var duration = new TimeSpan(0, 0, seconds);

                    OutputCacheResponseFilter responseFilter = OutputCachingProvider.Instance(this._app.Context.Items[ContextKeyTabOutputCacheProvider].ToString()).GetResponseFilter(
                        Convert.ToInt32(this._app.Context.Items[ContextKeyTabId]),
                        maxCachedVariationsForTab,
                        this._app.Response.Filter,
                        cacheKey,
                        duration);
                    this._app.Context.Items[ContextKeyResponseFilter] = responseFilter;
                    this._app.Context.Response.Filter = responseFilter;
                }
            }
        }

        private void OnUpdateRequestCache(object sender, EventArgs e)
        {
            if (!HttpContext.Current.Request.Browser.Crawler)
            {
                var responseFilter = this._app.Context.Items[ContextKeyResponseFilter] as OutputCacheResponseFilter;
                if (responseFilter != null)
                {
                    responseFilter.StopFiltering(Convert.ToInt32(this._app.Context.Items[ContextKeyTabId]), false);
                }
            }
        }
    }
}
