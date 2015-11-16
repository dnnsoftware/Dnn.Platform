#region Copyright
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2015
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.OutputCache;

namespace DotNetNuke.HttpModules.OutputCaching
{
    public class OutputCacheModule : IHttpModule
    {
        private const string ContextKeyResponseFilter = "OutputCache:ResponseFilter";
        private const string ContextKeyTabId = "OutputCache:TabId";
        private const string ContextKeyTabOutputCacheProvider = "OutputCache:TabOutputCacheProvider";
        private HttpApplication _app;

        #region IHttpModule Members

        public void Init(HttpApplication httpApp)
        {
            _app = httpApp;

            httpApp.ResolveRequestCache += OnResolveRequestCache;
            httpApp.UpdateRequestCache += OnUpdateRequestCache;
        }


        public void Dispose()
        {
        }

        #endregion

        private void OnResolveRequestCache(object sender, EventArgs e)
        {
            bool cached = false;
            if ((_app == null) || (_app.Context == null) || (_app.Context.Items == null) || _app.Response.ContentType.ToLower() != "text/html" || _app.Context.Request.IsAuthenticated ||
                HttpContext.Current.Request.Browser.Crawler)
            {
                return;
            }

            if (_app.Context.Request.RequestType == "POST" || ! (_app.Context.Request.Url.LocalPath.ToLower().EndsWith(Globals.glbDefaultPage.ToLower())))
            {
                return;
            }
            var portalSettings = (PortalSettings) (HttpContext.Current.Items["PortalSettings"]);
            int tabId = portalSettings.ActiveTab.TabID;

            Hashtable tabSettings = TabController.Instance.GetTabSettings(tabId);

            if (tabSettings["CacheProvider"] == null || string.IsNullOrEmpty(tabSettings["CacheProvider"].ToString()))
            {
                return;
            }

            int portalId = portalSettings.PortalId;
            string locale = Localization.GetPageLocale(portalSettings).Name;

            IncludeExcludeType includeExclude = IncludeExcludeType.ExcludeByDefault;
            if (tabSettings["CacheIncludeExclude"] != null && ! string.IsNullOrEmpty(tabSettings["CacheIncludeExclude"].ToString()))
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
            _app.Context.Items[ContextKeyTabOutputCacheProvider] = tabOutputCacheProvider;
            int maxCachedVariationsForTab = 250; //by default, prevent DOS attacks
            if (tabSettings["MaxVaryByCount"] != null && ! string.IsNullOrEmpty(tabSettings["MaxVaryByCount"].ToString()))
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
            //make sure to always add keys in lowercase only

            if (includeExclude == IncludeExcludeType.ExcludeByDefault)
            {
                string includeVaryByKeysSettings = string.Empty;
                if (tabSettings["IncludeVaryBy"] != null)
                {
                    includeVaryByKeysSettings = tabSettings["IncludeVaryBy"].ToString();
                }

                if (! string.IsNullOrEmpty(includeVaryByKeysSettings))
                {
                    if (includeVaryByKeysSettings.Contains(","))
                    {
                        string[] keys = includeVaryByKeysSettings.Split(char.Parse(","));
                        foreach (string key in keys)
                        {
                            includeVaryByKeys.Add(key.Trim().ToLower());
                        }
                    }
                    else
                    {
                        includeVaryByKeys.Add(includeVaryByKeysSettings.Trim().ToLower());
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

                if (! string.IsNullOrEmpty(excludeVaryByKeysSettings))
                {
                    if (excludeVaryByKeysSettings.Contains(","))
                    {
                        string[] keys = excludeVaryByKeysSettings.Split(char.Parse(","));
                        foreach (string key in keys)
                        {
                            excludeVaryByKeys.Add(key.Trim().ToLower());
                        }
                    }
                    else
                    {
                        excludeVaryByKeys.Add(excludeVaryByKeysSettings.Trim().ToLower());
                    }
                }
            }

            var varyBy = new SortedDictionary<string, string>();

            foreach (string key in _app.Context.Request.QueryString)
            {
                if (key != null && _app.Context.Request.QueryString[key] != null)
                    varyBy.Add(key.ToLower(), _app.Context.Request.QueryString[key]);
            }
            if (! (varyBy.ContainsKey("portalid")))
            {
                varyBy.Add("portalid", portalId.ToString());
            }
            if (! (varyBy.ContainsKey("tabid")))
            {
                varyBy.Add("tabid", tabId.ToString());
            }
            if (! (varyBy.ContainsKey("locale")))
            {
                varyBy.Add("locale", locale);
            }
			if (!(varyBy.ContainsKey("alias")))
			{
				varyBy.Add("alias", portalSettings.PortalAlias.HTTPAlias);
			}


            string cacheKey = OutputCachingProvider.Instance(tabOutputCacheProvider).GenerateCacheKey(tabId, includeVaryByKeys, excludeVaryByKeys, varyBy);

            bool returnedFromCache = OutputCachingProvider.Instance(tabOutputCacheProvider).StreamOutput(tabId, cacheKey, _app.Context);

            if (returnedFromCache)
            {
				//output the content type heade when read content from cache.
				_app.Context.Response.AddHeader("Content-Type", string.Format("{0}; charset={1}", _app.Response.ContentType, _app.Response.Charset));
                //This is to give a site owner the ability
                //to visually verify that a page was rendered via
                //the output cache.  Use FireFox FireBug or another
                //tool to view the response headers easily.
                _app.Context.Response.AddHeader("DNNOutputCache", "true");

                //Also add it ti the Context - the Headers are readonly unless using IIS in Integrated Pipleine mode
                //and we need to know if OutPut Caching is active in the compression module
                _app.Context.Items.Add("DNNOutputCache", "true");

                _app.Context.Response.End();
                cached = true;
            }

/*
                IsHandledPageRequest = true;
*/
            _app.Context.Items[ContextKeyTabId] = tabId;

            if (cached != true)
            {
                if (tabSettings["CacheDuration"] != null && ! string.IsNullOrEmpty(tabSettings["CacheDuration"].ToString()) && Convert.ToInt32(tabSettings["CacheDuration"].ToString()) > 0)
                {
                    int seconds = Convert.ToInt32(tabSettings["CacheDuration"].ToString());
                    var duration = new TimeSpan(0, 0, seconds);

                    OutputCacheResponseFilter responseFilter = OutputCachingProvider.Instance(_app.Context.Items[ContextKeyTabOutputCacheProvider].ToString()).GetResponseFilter(Convert.ToInt32(_app.Context.Items[ContextKeyTabId]),
                                                                                                                                                                                 maxCachedVariationsForTab,
                                                                                                                                                                                 _app.Response.Filter,
                                                                                                                                                                                 cacheKey,
                                                                                                                                                                                 duration);
                    _app.Context.Items[ContextKeyResponseFilter] = responseFilter;
                    _app.Context.Response.Filter = responseFilter;
                }
            }
        }

        private void OnUpdateRequestCache(object sender, EventArgs e)
        {
            if (! HttpContext.Current.Request.Browser.Crawler)
            {
                var responseFilter = (OutputCacheResponseFilter) (_app.Context.Items[ContextKeyResponseFilter]);
                if (responseFilter != null)
                {
                    responseFilter.StopFiltering(Convert.ToInt32(_app.Context.Items[ContextKeyTabId]), false);
                }
            }
        }

        #region Nested type: IncludeExcludeType

        private enum IncludeExcludeType
        {
            IncludeByDefault,
            ExcludeByDefault
        }

        #endregion
    }
}