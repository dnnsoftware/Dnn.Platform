// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Cache
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Caching;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// CachingProvider provides basic component of cache system, by default it will use HttpRuntime.Cache.
    /// </summary>
    /// <remarks>
    /// <para>Using cache will speed up the application to a great degree, we recommend to use cache for whole modules,
    /// but sometimes cache also make confuse for user, if we didn't take care of how to make cache expired when needed,
    /// such as if a data has already been deleted but the cache arn't clear, it will cause un expected errors.
    /// so you should choose a correct performance setting type when you trying to cache some stuff, and always remember
    /// update cache immediately after the data changed.</para>
    /// </remarks>
    /// <example>
    /// <code lang="C#">
    /// public static void ClearCache(string cachePrefix)
    /// {
    ///     CachingProvider.Instance().Clear("Prefix", GetDnnCacheKey(cachePrefix));
    /// }
    /// </code>
    /// </example>
    public abstract class CachingProvider
    {
        private const string CachePrefix = "DNN_";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CachingProvider));

        private static System.Web.Caching.Cache _cache;

        /// <summary>
        /// Gets the default cache provider.
        /// </summary>
        /// <value>HttpRuntime.Cache.</value>
        protected static System.Web.Caching.Cache Cache
        {
            get
            {
                return _cache ?? (_cache = HttpRuntime.Cache);
            }
        }

        /// <summary>
        /// Gets a value indicating whether whether current caching provider disabled to expire cache.
        /// </summary>
        /// <remarks>This setting shouldn't affect current server, cache should always expire in current server even this setting set to True.</remarks>
        protected static bool CacheExpirationDisable { get; private set; }

        /// <summary>
        /// Cleans the cache key by remove cache key prefix.
        /// </summary>
        /// <param name="CacheKey">The cache key.</param>
        /// <returns>cache key without prefix.</returns>
        /// <exception cref="ArgumentException">cache key is empty.</exception>
        public static string CleanCacheKey(string CacheKey)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", "CacheKey");
            }

            return CacheKey.Substring(CachePrefix.Length);
        }

        /// <summary>
        /// Gets the cache key with key prefix.
        /// </summary>
        /// <param name="CacheKey">The cache key.</param>
        /// <returns>CachePrefix + CacheKey.</returns>
        /// <exception cref="ArgumentException">Cache key is empty.</exception>
        public static string GetCacheKey(string CacheKey)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", "CacheKey");
            }

            return CachePrefix + CacheKey;
        }

        /// <summary>
        /// Instances of  caching provider.
        /// </summary>
        /// <returns>The Implemments provider of cache system defind in web.config.</returns>
        public static CachingProvider Instance()
        {
            return ComponentFactory.GetComponent<CachingProvider>();
        }

        /// <summary>
        /// Clears the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="data">The data.</param>
        public virtual void Clear(string type, string data)
        {
            this.ClearCacheInternal(type, data, false);
        }

        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return Cache.GetEnumerator();
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns>cache content.</returns>
        public virtual object GetItem(string cacheKey)
        {
            return Cache[cacheKey];
        }

        /// <summary>
        /// Inserts the specified cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="itemToCache">The object.</param>
        public virtual void Insert(string cacheKey, object itemToCache)
        {
            this.Insert(cacheKey, itemToCache, null as DNNCacheDependency, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        /// <summary>
        /// Inserts the specified cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="itemToCache">The object.</param>
        /// <param name="dependency">The dependency.</param>
        public virtual void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency)
        {
            this.Insert(cacheKey, itemToCache, dependency, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        /// <summary>
        /// Inserts the specified cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="itemToCache">The object.</param>
        /// <param name="dependency">The dependency.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <param name="slidingExpiration">The sliding expiration.</param>
        public virtual void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            this.Insert(cacheKey, itemToCache, dependency, absoluteExpiration, slidingExpiration, CacheItemPriority.Default, null);
        }

        /// <summary>
        /// Inserts the specified cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="itemToCache">The value.</param>
        /// <param name="dependency">The dependency.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <param name="slidingExpiration">The sliding expiration.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="onRemoveCallback">The on remove callback.</param>
        public virtual void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority,
                                   CacheItemRemovedCallback onRemoveCallback)
        {
            Cache.Insert(cacheKey, itemToCache, dependency == null ? null : dependency.SystemCacheDependency, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }

        /// <summary>
        /// Determines whether is web farm.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if is web farm; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsWebFarm()
        {
            return ServerController.GetEnabledServers().Count > 1;
        }

        /// <summary>
        /// Purges the cache.
        /// </summary>
        /// <returns></returns>
        public virtual string PurgeCache()
        {
            return Localization.GetString("PurgeCacheUnsupported.Text", Localization.GlobalResourceFile);
        }

        /// <summary>
        /// Removes the specified cache key.
        /// </summary>
        /// <param name="CacheKey">The cache key.</param>
        public virtual void Remove(string CacheKey)
        {
            this.RemoveInternal(CacheKey);
        }

        /// <summary>
        /// Disable Cache Expirataion. This control won't affect core caching provider, its behavior determined by extended caching provider.
        /// This property designed for when process long time action, extended caching provider should not sync cache between web servers to improve performance.
        /// </summary>
        /// <seealso cref="CacheExpirationDisable"/>
        internal static void DisableCacheExpiration()
        {
            CacheExpirationDisable = true;
            Logger.Warn("Disable cache expiration.");
        }

        /// <summary>
        /// Enable Cache Expirataion. This control won't affect core caching provider, its behavior determined by extended caching provider.
        /// This property designed for when process long time action, extended caching provider should not sync cache between web servers to improve performance.
        /// </summary>
        /// <seealso cref="CacheExpirationDisable"/>
        internal static void EnableCacheExpiration()
        {
            CacheExpirationDisable = false;
            DataCache.ClearHostCache(true);
            Logger.Warn("Enable cache expiration.");
        }

        /// <summary>
        /// Clears the cache internal.
        /// </summary>
        /// <param name="cacheType">Type of the cache.</param>
        /// <param name="data">The data.</param>
        /// <param name="clearRuntime">if set to <c>true</c> clear runtime cache.</param>
        protected void ClearCacheInternal(string cacheType, string data, bool clearRuntime)
        {
            switch (cacheType)
            {
                case "Prefix":
                    this.ClearCacheInternal(data, clearRuntime);
                    break;
                case "Host":
                    this.ClearHostCacheInternal(clearRuntime);
                    break;
                case "Folder":
                    this.ClearFolderCacheInternal(int.Parse(data), clearRuntime);
                    break;
                case "Module":
                    this.ClearModuleCacheInternal(int.Parse(data), clearRuntime);
                    break;
                case "ModulePermissionsByPortal":
                    this.ClearModulePermissionsCachesByPortalInternal(int.Parse(data), clearRuntime);
                    break;
                case "Portal":
                    this.ClearPortalCacheInternal(int.Parse(data), false, clearRuntime);
                    break;
                case "PortalCascade":
                    this.ClearPortalCacheInternal(int.Parse(data), true, clearRuntime);
                    break;
                case "Tab":
                    this.ClearTabCacheInternal(int.Parse(data), clearRuntime);
                    break;
                case "ServiceFrameworkRoutes":
                    this.ReloadServicesFrameworkRoutes();
                    break;
            }
        }

        /// <summary>
        /// Removes the internal.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        protected void RemoveInternal(string cacheKey)
        {
            // attempt remove from private dictionary
            DataCache.RemoveFromPrivateDictionary(cacheKey);

            // remove item from memory
            if (Cache[cacheKey] != null)
            {
                Cache.Remove(cacheKey);
            }
        }

        private void ClearCacheInternal(string prefix, bool clearRuntime)
        {
            foreach (DictionaryEntry objDictionaryEntry in HttpRuntime.Cache)
            {
                if (Convert.ToString(objDictionaryEntry.Key).StartsWith(prefix))
                {
                    if (clearRuntime)
                    {
                        // remove item from runtime cache
                        this.RemoveInternal(Convert.ToString(objDictionaryEntry.Key));
                    }
                    else
                    {
                        // Call provider's remove method
                        this.Remove(Convert.ToString(objDictionaryEntry.Key));
                    }
                }
            }
        }

        private void ClearCacheKeysByPortalInternal(int portalId, bool clearRuntime)
        {
            this.RemoveFormattedCacheKey(DataCache.PortalCacheKey, clearRuntime, Null.NullInteger, string.Empty);
            this.RemoveFormattedCacheKey(DataCache.LocalesCacheKey, clearRuntime, portalId);
            this.RemoveFormattedCacheKey(DataCache.ProfileDefinitionsCacheKey, clearRuntime, portalId);
            this.RemoveFormattedCacheKey(DataCache.ListsCacheKey, clearRuntime, portalId);
            this.RemoveFormattedCacheKey(DataCache.SkinsCacheKey, clearRuntime, portalId);
            this.RemoveFormattedCacheKey(DataCache.PortalUserCountCacheKey, clearRuntime, portalId);
            this.RemoveFormattedCacheKey(DataCache.PackagesCacheKey, clearRuntime, portalId);

            this.RemoveCacheKey(DataCache.AllPortalsCacheKey, clearRuntime);
        }

        private void ClearDesktopModuleCacheInternal(int portalId, bool clearRuntime)
        {
            this.RemoveFormattedCacheKey(DataCache.DesktopModuleCacheKey, clearRuntime, portalId);
            this.RemoveFormattedCacheKey(DataCache.PortalDesktopModuleCacheKey, clearRuntime, portalId);
            this.RemoveCacheKey(DataCache.ModuleDefinitionCacheKey, clearRuntime);
            this.RemoveCacheKey(DataCache.ModuleControlsCacheKey, clearRuntime);
        }

        private void ClearFolderCacheInternal(int portalId, bool clearRuntime)
        {
            this.RemoveFormattedCacheKey(DataCache.FolderCacheKey, clearRuntime, portalId);

            // FolderUserCacheKey also includes permissions and userId but we don't have that information
            // here so we remove them using a prefix
            var folderUserCachePrefix = GetCacheKey(string.Format("Folders|{0}|", portalId));
            this.ClearCacheInternal(folderUserCachePrefix, clearRuntime);

            PermissionProvider.ResetCacheDependency(
                portalId,
                () => this.RemoveFormattedCacheKey(DataCache.FolderPermissionCacheKey, clearRuntime, portalId));
        }

        private void ClearHostCacheInternal(bool clearRuntime)
        {
            this.RemoveCacheKey(DataCache.HostSettingsCacheKey, clearRuntime);
            this.RemoveCacheKey(DataCache.SecureHostSettingsCacheKey, clearRuntime);
            this.RemoveCacheKey(DataCache.UnSecureHostSettingsCacheKey, clearRuntime);
            this.RemoveCacheKey(DataCache.PortalAliasCacheKey, clearRuntime);
            this.RemoveCacheKey("CSS", clearRuntime);
            this.RemoveCacheKey("StyleSheets", clearRuntime);
            this.RemoveCacheKey(DataCache.DesktopModulePermissionCacheKey, clearRuntime);
            this.RemoveCacheKey("GetRoles", clearRuntime);
            this.RemoveCacheKey("CompressionConfig", clearRuntime);
            this.RemoveCacheKey(DataCache.SubscriptionTypesCacheKey, clearRuntime);
            this.RemoveCacheKey(DataCache.PackageTypesCacheKey, clearRuntime);
            this.RemoveCacheKey(DataCache.PermissionsCacheKey, clearRuntime);
            this.RemoveCacheKey(DataCache.ContentTypesCacheKey, clearRuntime);
            this.RemoveCacheKey(DataCache.JavaScriptLibrariesCacheKey, clearRuntime);

            // Clear "portal keys" for Host
            this.ClearFolderCacheInternal(-1, clearRuntime);
            this.ClearDesktopModuleCacheInternal(-1, clearRuntime);
            this.ClearCacheKeysByPortalInternal(-1, clearRuntime);
            this.ClearTabCacheInternal(-1, clearRuntime);
        }

        private void ClearModuleCacheInternal(int tabId, bool clearRuntime)
        {
            var cacheKey = string.Format(DataCache.TabModuleCacheKey, tabId);
            var tabModules = Cache.Get(cacheKey) as Dictionary<int, ModuleInfo>;
            if (tabModules != null && tabModules.Any())
            {
                foreach (var moduleInfo in tabModules.Values)
                {
                    cacheKey = string.Format(DataCache.SingleTabModuleCacheKey, moduleInfo.TabModuleID);
                    if (clearRuntime)
                    {
                        this.RemoveInternal(cacheKey);
                    }
                    else
                    {
                        this.Remove(cacheKey);
                    }
                }
            }

            this.RemoveFormattedCacheKey(DataCache.TabModuleCacheKey, clearRuntime, tabId);
            this.RemoveFormattedCacheKey(DataCache.PublishedTabModuleCacheKey, clearRuntime, tabId);
            this.RemoveFormattedCacheKey(DataCache.ModulePermissionCacheKey, clearRuntime, tabId);
            this.RemoveFormattedCacheKey(DataCache.ModuleSettingsCacheKey, clearRuntime, tabId);
        }

        private void ClearModulePermissionsCachesByPortalInternal(int portalId, bool clearRuntime)
        {
            foreach (var tabPair in TabController.Instance.GetTabsByPortal(portalId))
            {
                this.RemoveFormattedCacheKey(DataCache.ModulePermissionCacheKey, clearRuntime, tabPair.Value.TabID);
            }
        }

        private void ClearPortalCacheInternal(int portalId, bool cascade, bool clearRuntime)
        {
            this.RemoveFormattedCacheKey(DataCache.PortalSettingsCacheKey, clearRuntime, portalId, string.Empty);

            var locales = LocaleController.Instance.GetLocales(portalId);
            if (locales == null || locales.Count == 0)
            {
                // At least attempt to remove default locale
                string defaultLocale = PortalController.GetPortalDefaultLanguage(portalId);
                this.RemoveCacheKey(string.Format(DataCache.PortalCacheKey, portalId, defaultLocale), clearRuntime);
                this.RemoveCacheKey(string.Format(DataCache.PortalCacheKey, portalId, Null.NullString), clearRuntime);
                this.RemoveFormattedCacheKey(DataCache.PortalSettingsCacheKey, clearRuntime, portalId, defaultLocale);
            }
            else
            {
                foreach (Locale portalLocale in LocaleController.Instance.GetLocales(portalId).Values)
                {
                    this.RemoveCacheKey(string.Format(DataCache.PortalCacheKey, portalId, portalLocale.Code), clearRuntime);
                    this.RemoveFormattedCacheKey(DataCache.PortalSettingsCacheKey, clearRuntime, portalId, portalLocale.Code);
                }

                this.RemoveCacheKey(string.Format(DataCache.PortalCacheKey, portalId, Null.NullString), clearRuntime);
                this.RemoveFormattedCacheKey(DataCache.PortalSettingsCacheKey, clearRuntime, portalId, Null.NullString);
            }

            if (cascade)
            {
                foreach (KeyValuePair<int, TabInfo> tabPair in TabController.Instance.GetTabsByPortal(portalId))
                {
                    this.ClearModuleCacheInternal(tabPair.Value.TabID, clearRuntime);
                }

                foreach (ModuleInfo moduleInfo in ModuleController.Instance.GetModules(portalId))
                {
                    this.RemoveCacheKey("GetModuleSettings" + moduleInfo.ModuleID, clearRuntime);
                }
            }

            // Clear "portal keys" for Portal
            this.ClearFolderCacheInternal(portalId, clearRuntime);
            this.ClearCacheKeysByPortalInternal(portalId, clearRuntime);
            this.ClearDesktopModuleCacheInternal(portalId, clearRuntime);
            this.ClearTabCacheInternal(portalId, clearRuntime);

            this.RemoveCacheKey(string.Format(DataCache.RolesCacheKey, portalId), clearRuntime);
            this.RemoveCacheKey(string.Format(DataCache.JournalTypesCacheKey, portalId), clearRuntime);
        }

        private void ClearTabCacheInternal(int portalId, bool clearRuntime)
        {
            this.RemoveFormattedCacheKey(DataCache.TabCacheKey, clearRuntime, portalId);
            this.RemoveFormattedCacheKey(DataCache.TabAliasSkinCacheKey, clearRuntime, portalId);
            this.RemoveFormattedCacheKey(DataCache.TabCustomAliasCacheKey, clearRuntime, portalId);
            this.RemoveFormattedCacheKey(DataCache.TabUrlCacheKey, clearRuntime, portalId);
            this.RemoveFormattedCacheKey(DataCache.TabPermissionCacheKey, clearRuntime, portalId);
            Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(portalId);
            if (locales == null || locales.Count == 0)
            {
                // At least attempt to remove default locale
                string defaultLocale = PortalController.GetPortalDefaultLanguage(portalId);
                this.RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, defaultLocale, portalId), clearRuntime);
            }
            else
            {
                foreach (Locale portalLocale in LocaleController.Instance.GetLocales(portalId).Values)
                {
                    this.RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, portalLocale.Code, portalId), clearRuntime);
                }
            }

            this.RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, Null.NullString, portalId), clearRuntime);
            this.RemoveCacheKey(string.Format(DataCache.TabSettingsCacheKey, portalId), clearRuntime);
        }

        private void RemoveCacheKey(string CacheKey, bool clearRuntime)
        {
            if (clearRuntime)
            {
                // remove item from runtime cache
                this.RemoveInternal(GetCacheKey(CacheKey));
            }
            else
            {
                // Call provider's remove method
                this.Remove(GetCacheKey(CacheKey));
            }
        }

        private void RemoveFormattedCacheKey(string CacheKeyBase, bool clearRuntime, params object[] parameters)
        {
            if (clearRuntime)
            {
                // remove item from runtime cache
                this.RemoveInternal(string.Format(GetCacheKey(CacheKeyBase), parameters));
            }
            else
            {
                // Call provider's remove method
                this.Remove(string.Format(GetCacheKey(CacheKeyBase), parameters));
            }
        }

        private void ReloadServicesFrameworkRoutes()
        {
            // registration of routes when the servers is operating is done as part of the cache
            // because the web request cahcing provider is the only inter-server communication channel
            // that is reliable
            ServicesRoutingManager.RegisterServiceRoutes();
        }
    }
}
