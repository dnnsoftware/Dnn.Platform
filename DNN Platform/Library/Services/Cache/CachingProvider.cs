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
using System.Web;
using System.Web.Caching;
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Services.Cache
{
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
		#region Private Members

        private static System.Web.Caching.Cache _cache;
        private const string CachePrefix = "DNN_";
		
		#endregion

		#region Protected Properties

		/// <summary>
		/// Gets the default cache provider.
		/// </summary>
		/// <value>HttpRuntime.Cache</value>
        protected static System.Web.Caching.Cache Cache
        {
            get
            {
                return _cache ?? (_cache = HttpRuntime.Cache);
            }
        }
		
		#endregion

		#region Shared/Static Methods

		/// <summary>
		/// Cleans the cache key by remove cache key prefix.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <returns>cache key without prefix.</returns>
		/// <exception cref="ArgumentException">cache key is empty.</exception>
        public static string CleanCacheKey(string CacheKey)
        {
            if (String.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", "CacheKey");
            }
            return CacheKey.Substring(CachePrefix.Length);
        }

		/// <summary>
		/// Gets the cache key with key prefix.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
		/// <returns>CachePrefix + CacheKey</returns>
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
		
	#endregion

	    #region Private Methods
	
        private void ClearCacheInternal(string prefix, bool clearRuntime)
        {
            foreach (DictionaryEntry objDictionaryEntry in HttpRuntime.Cache)
            {
                if (Convert.ToString(objDictionaryEntry.Key).StartsWith(prefix))
                {
                    if (clearRuntime)
                    {
						//remove item from runtime cache
                        RemoveInternal(Convert.ToString(objDictionaryEntry.Key));
                    }
                    else
                    {
						//Call provider's remove method
                        Remove(Convert.ToString(objDictionaryEntry.Key));
                    }
                }
            }
        }

        private void ClearCacheKeysByPortalInternal(int portalId, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.PortalCacheKey, clearRuntime, Null.NullInteger, string.Empty);
            RemoveFormattedCacheKey(DataCache.LocalesCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.ProfileDefinitionsCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.ListsCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.SkinsCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.PortalUserCountCacheKey, clearRuntime, portalId);
			RemoveFormattedCacheKey(DataCache.PackagesCacheKey, clearRuntime, portalId);

			RemoveCacheKey(DataCache.AllPortalsCacheKey, clearRuntime);
        }

        private void ClearDesktopModuleCacheInternal(int portalId, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.DesktopModuleCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.PortalDesktopModuleCacheKey, clearRuntime, portalId);
            RemoveCacheKey(DataCache.ModuleDefinitionCacheKey, clearRuntime);
            RemoveCacheKey(DataCache.ModuleControlsCacheKey, clearRuntime);
        }

        private void ClearFolderCacheInternal(int portalId, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.FolderCacheKey, clearRuntime, portalId);

            // FolderUserCacheKey also includes permissions and userId but we don't have that information
            // here so we remove them using a prefix
            var folderUserCachePrefix = GetCacheKey(string.Format("Folders|{0}|", portalId));
            ClearCacheInternal(folderUserCachePrefix, clearRuntime);
            
            RemoveFormattedCacheKey(DataCache.FolderPermissionCacheKey, clearRuntime, portalId);
        }

        private void ClearHostCacheInternal(bool clearRuntime)
        {
            RemoveCacheKey(DataCache.HostSettingsCacheKey, clearRuntime);
            RemoveCacheKey(DataCache.SecureHostSettingsCacheKey, clearRuntime);
            RemoveCacheKey(DataCache.UnSecureHostSettingsCacheKey, clearRuntime);
            RemoveCacheKey(DataCache.PortalAliasCacheKey, clearRuntime);
            RemoveCacheKey("CSS", clearRuntime);
            RemoveCacheKey("StyleSheets", clearRuntime);
            RemoveCacheKey(DataCache.DesktopModulePermissionCacheKey, clearRuntime);
            RemoveCacheKey("GetRoles", clearRuntime);
            RemoveCacheKey("CompressionConfig", clearRuntime);
            RemoveCacheKey(DataCache.SubscriptionTypesCacheKey, clearRuntime);
            RemoveCacheKey(DataCache.PackageTypesCacheKey, clearRuntime);
            RemoveCacheKey(DataCache.ContentTypesCacheKey, clearRuntime);
            RemoveCacheKey(DataCache.JavaScriptLibrariesCacheKey, clearRuntime);

            //Clear "portal keys" for Host
            ClearFolderCacheInternal(-1, clearRuntime);
            ClearDesktopModuleCacheInternal(-1, clearRuntime);
            ClearCacheKeysByPortalInternal(-1, clearRuntime);
            ClearTabCacheInternal(-1, clearRuntime);

        }

        private void ClearModuleCacheInternal(int tabId, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.TabModuleCacheKey, clearRuntime, tabId);
            RemoveFormattedCacheKey(DataCache.ModulePermissionCacheKey, clearRuntime, tabId);
        }

        private void ClearModulePermissionsCachesByPortalInternal(int portalId, bool clearRuntime)
        {
            var objTabs = new TabController();
            foreach (KeyValuePair<int, TabInfo> tabPair in objTabs.GetTabsByPortal(portalId))
            {
                RemoveFormattedCacheKey(DataCache.ModulePermissionCacheKey, clearRuntime, tabPair.Value.TabID);
            }
        }

        private void ClearPortalCacheInternal(int portalId, bool cascade, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.PortalSettingsCacheKey, clearRuntime, portalId);

            Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(portalId);
            if (locales == null || locales.Count == 0)
            {
                //At least attempt to remove default locale
                string defaultLocale = PortalController.GetPortalDefaultLanguage(portalId);
                RemoveCacheKey(String.Format(DataCache.PortalCacheKey, portalId, defaultLocale), clearRuntime);
            }
            else
            {
                foreach (Locale portalLocale in LocaleController.Instance.GetLocales(portalId).Values)
                {
                    RemoveCacheKey(String.Format(DataCache.PortalCacheKey, portalId, portalLocale.Code), clearRuntime);
                }
            }
            if (cascade)
            {
                var objTabs = new TabController();
                foreach (KeyValuePair<int, TabInfo> tabPair in objTabs.GetTabsByPortal(portalId))
                {
                    ClearModuleCacheInternal(tabPair.Value.TabID, clearRuntime);
                }
                var moduleController = new ModuleController();
                foreach (ModuleInfo moduleInfo in moduleController.GetModules(portalId))
                {
                    RemoveCacheKey("GetModuleSettings" + moduleInfo.ModuleID, clearRuntime);
                }
            }
			
            //Clear "portal keys" for Portal
            ClearFolderCacheInternal(portalId, clearRuntime);
            ClearCacheKeysByPortalInternal(portalId, clearRuntime);
            ClearDesktopModuleCacheInternal(portalId, clearRuntime);
            ClearTabCacheInternal(portalId, clearRuntime);

            RemoveCacheKey(String.Format(DataCache.RolesCacheKey, portalId), clearRuntime);
            RemoveCacheKey(String.Format(DataCache.JournalTypesCacheKey, portalId), clearRuntime);
        }

        private void ClearTabCacheInternal(int portalId, bool clearRuntime)
        {
            RemoveFormattedCacheKey(DataCache.TabCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.TabAliasSkinCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.TabCustomAliasCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.TabUrlCacheKey, clearRuntime, portalId);
            RemoveFormattedCacheKey(DataCache.TabPermissionCacheKey, clearRuntime, portalId);
            Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(portalId);
            if (locales == null || locales.Count == 0)
            {
                //At least attempt to remove default locale
                string defaultLocale = PortalController.GetPortalDefaultLanguage(portalId);
                RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, defaultLocale, portalId), clearRuntime);
            }
            else
            {
                foreach (Locale portalLocale in LocaleController.Instance.GetLocales(portalId).Values)
                {
                    RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, portalLocale.Code, portalId), clearRuntime);
                }
            }

            RemoveCacheKey(string.Format(DataCache.TabPathCacheKey, Null.NullString, portalId), clearRuntime);
        }

        private void RemoveCacheKey(string CacheKey, bool clearRuntime)
        {
            if (clearRuntime)
            {
				//remove item from runtime cache
                RemoveInternal(GetCacheKey(CacheKey));
            }
            else
            {
				//Call provider's remove method
                Remove(GetCacheKey(CacheKey));
            }
        }

        private void RemoveFormattedCacheKey(string CacheKeyBase, bool clearRuntime, params object[] parameters)
        {
            if (clearRuntime)
            {
				//remove item from runtime cache
                RemoveInternal(string.Format(GetCacheKey(CacheKeyBase), parameters));
            }
            else
            {
				//Call provider's remove method
                Remove(string.Format(GetCacheKey(CacheKeyBase), parameters));
            }
        }
		
		#endregion

		#region Protected Methods

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
                    ClearCacheInternal(data, clearRuntime);
                    break;
                case "Host":
                    ClearHostCacheInternal(clearRuntime);
                    break;
                case "Folder":
                    ClearFolderCacheInternal(int.Parse(data), clearRuntime);
                    break;
                case "Module":
                    ClearModuleCacheInternal(int.Parse(data), clearRuntime);
                    break;
                case "ModulePermissionsByPortal":
                    ClearModulePermissionsCachesByPortalInternal(int.Parse(data), clearRuntime);
                    break;
                case "Portal":
                    ClearPortalCacheInternal(int.Parse(data), false, clearRuntime);
                    break;
                case "PortalCascade":
                    ClearPortalCacheInternal(int.Parse(data), true, clearRuntime);
                    break;
                case "Tab":
                    ClearTabCacheInternal(int.Parse(data), clearRuntime);
                    break;
                case "ServiceFrameworkRoutes":
                    ReloadServicesFrameworkRoutes();
                    break;
            }
        }

	    private void ReloadServicesFrameworkRoutes()
	    {
            //registration of routes when the servers is operating is done as part of the cache
            //because the web request cahcing provider is the only inter-server communication channel
            //that is reliable
            ServicesRoutingManager.RegisterServiceRoutes();
	    }

	    /// <summary>
		/// Removes the internal.
		/// </summary>
		/// <param name="cacheKey">The cache key.</param>
        protected void RemoveInternal(string cacheKey)
        {
			//attempt remove from private dictionary
            DataCache.RemoveFromPrivateDictionary(cacheKey);
            //remove item from memory
            if (Cache[cacheKey] != null)
            {
                Cache.Remove(cacheKey);
            }
        }

		/// <summary>
		/// Clears the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="data">The data.</param>
        public virtual void Clear(string type, string data)
        {
            ClearCacheInternal(type, data, false);
        }

        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return Cache.GetEnumerator();
        }

		/// <summary>
		/// Gets the item.
		/// </summary>
		/// <param name="cacheKey">The cache key.</param>
		/// <returns>cache content</returns>
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
            Insert(cacheKey, itemToCache, null as DNNCacheDependency, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

		/// <summary>
		/// Inserts the specified cache key.
		/// </summary>
		/// <param name="cacheKey">The cache key.</param>
		/// <param name="itemToCache">The object.</param>
		/// <param name="dependency">The dependency.</param>
        public virtual void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency)
        {
            Insert(cacheKey, itemToCache, dependency, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
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
            Insert(cacheKey, itemToCache, dependency, absoluteExpiration, slidingExpiration, CacheItemPriority.Default, null);
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
            return (ServerController.GetEnabledServers().Count > 1);
        }

		/// <summary>
		/// Purges the cache.
		/// </summary>
		/// <returns></returns>
        public virtual string PurgeCache()
        {
            return Localization.Localization.GetString("PurgeCacheUnsupported.Text", Localization.Localization.GlobalResourceFile);
        }

		/// <summary>
		/// Removes the specified cache key.
		/// </summary>
		/// <param name="CacheKey">The cache key.</param>
        public virtual void Remove(string CacheKey)
        {
            RemoveInternal(CacheKey);
        }
		
		#endregion

        #region Obsolete Methods

        [Obsolete("Deprecated in DNN 5.1 - Use one of the INsert methods")]
        public virtual object Add(string CacheKey, object objObject, CacheDependency objDependency, DateTime AbsoluteExpiration, TimeSpan SlidingExpiration, CacheItemPriority Priority, CacheItemRemovedCallback OnRemoveCallback)
        {
            object retValue = GetItem(CacheKey);
            if (retValue == null)
            {
                Insert(CacheKey, objObject, new DNNCacheDependency(objDependency), AbsoluteExpiration, SlidingExpiration, Priority, OnRemoveCallback);
            }
            return retValue;
        }

        [Obsolete("Deprecated in DNN 5.1 - Cache Persistence is not supported")]
        public virtual object GetPersistentCacheItem(string CacheKey, Type objType)
        {
            return GetItem(CacheKey);
        }

        [Obsolete("Deprecated in DNN 5.1 - Cache Persistence is not supported")]
        public virtual void Insert(string cacheKey, object itemToCache, bool PersistAppRestart)
        {
            Insert(cacheKey, itemToCache);
        }

        [Obsolete("Deprecated in DNN 5.1 - Cache Persistence is not supported")]
        public virtual void Insert(string cacheKey, object itemToCache, CacheDependency objDependency, bool PersistAppRestart)
        {
            Insert(cacheKey, itemToCache, new DNNCacheDependency(objDependency), System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        [Obsolete("Deprecated in DNN 5.1 - Cache Persistence is not supported")]
        public virtual void Insert(string cacheKey, object itemToCache, CacheDependency objDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration, bool PersistAppRestart)
        {
            Insert(cacheKey, itemToCache, new DNNCacheDependency(objDependency), absoluteExpiration, slidingExpiration, CacheItemPriority.Default, null);
        }

        [Obsolete("Deprecated in DNN 5.1 - Cache Persistence is not supported")]
        public virtual void Insert(string Key, object itemToCache, CacheDependency objDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback, bool PersistAppRestart)
        {
            Insert(Key, itemToCache, new DNNCacheDependency(objDependency), absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }

        [Obsolete("Deprecated in DNN 5.1 - Use new overload that uses a DNNCacheDependency")]
        public virtual void Insert(string cacheKey, object itemToCache, CacheDependency objDependency)
        {
            Insert(cacheKey, itemToCache, new DNNCacheDependency(objDependency), System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        [Obsolete("Deprecated in DNN 5.1 - Use new overload that uses a DNNCacheDependency")]
        public virtual void Insert(string cacheKey, object itemToCache, CacheDependency objDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            Insert(cacheKey, itemToCache, new DNNCacheDependency(objDependency), absoluteExpiration, slidingExpiration, CacheItemPriority.Default, null);
        }

        [Obsolete("Deprecated in DNN 5.1 - Use new overload that uses a DNNCacheDependency")]
        public virtual void Insert(string cacheKey, object itemToCache, CacheDependency objDependency, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            Insert(cacheKey, itemToCache, new DNNCacheDependency(objDependency), absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }

        [Obsolete("Deprecated in DNN 5.1.1 - Cache Persistence is not supported")]
        public virtual void RemovePersistentCacheItem(string CacheKey)
        {
            Remove(CacheKey);
        }

        #endregion
    }
}