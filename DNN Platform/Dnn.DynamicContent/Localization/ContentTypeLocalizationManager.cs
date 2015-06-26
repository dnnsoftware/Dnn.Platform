// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;

namespace Dnn.DynamicContent.Localization
{
    //TODO: XML Comments and Unit Tests
    public class ContentTypeLocalizationManager : ControllerBase<ContentTypeLocalization, IContentTypeLocalizationManager, ContentTypeLocalizationManager>, IContentTypeLocalizationManager
    {
        public const string CacheKey = "ContentTypes_Localization";
        public const string LocalizedCacheKey = "ContentTypes_Localization_{0}_{1}";
        public const string Scope = "PortalId";

        protected override Func<IContentTypeLocalizationManager> GetFactory()
        {
            return () => new ContentTypeLocalizationManager();
        }

        public ContentTypeLocalizationManager() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public ContentTypeLocalizationManager(IDataContext dataContext) : base(dataContext) { }

        public int AddLocalization(ContentTypeLocalization item)
        {
            Requires.PropertyNotNullOrEmpty(item, "CultureCode");
            Requires.PropertyNotNullOrEmpty(item, "Key");
            Requires.PropertyNotNullOrEmpty(item, "Value");

            Add(item);

            ClearLocalizedCache(item.PortalId, item.CultureCode);

            return item.LocalizationId;
        }

        private void ClearLocalizedCache(int portalId, string cultureCode)
        {
            var cacheKey = String.Format(LocalizedCacheKey, portalId, cultureCode);
            DataCache.RemoveCache(cacheKey);
        }

        public void DeleteLocalization(ContentTypeLocalization item)
        {
            Delete(item);

            ClearLocalizedCache(item.PortalId, item.CultureCode);
        }

        public void DeleteLocalizations(int portalId, string key)
        {
            using (DataContext)
            {
                var rep = DataContext.GetRepository<ContentTypeLocalization>();

                rep.Delete(String.Format("WHERE [Key] = '{0}'", key));
            }

            foreach (var locale in LocaleController.Instance.GetLocales(portalId).Values)
            {
                if (locale.Code != PortalController.Instance.GetCurrentPortalSettings().CultureCode)
                {
                    ClearLocalizedCache(portalId, locale.Code);
                }
            }
        }

        private Dictionary<string, string> GetLocalizedDictionary(int portalId, string cultureCode)
        {
            var cacheKey = String.Format(LocalizedCacheKey, portalId, cultureCode);
            var cacheItemArgs = new CacheItemArgs(cacheKey, 20, CacheItemPriority.AboveNormal);
            var localizedDictionary = DataCache.GetCachedData<Dictionary<string, string>>(cacheItemArgs, (c) =>
                                        {
                                            var dictionary = new Dictionary<string, string>();

                                            foreach (var item in GetLocalizations(portalId).Where(l => l.CultureCode == cultureCode))
                                            {
                                                dictionary[item.Key] = item.Value;
                                            }

                                            return dictionary;
                                        });

            return localizedDictionary;
        }

        public string GetLocalizedValue(string key, string cultureCode, int portalId)
        {
            var localizedDictionary = GetLocalizedDictionary(portalId, cultureCode);
            var localizedValue = String.Empty;
            if (localizedDictionary.ContainsKey(key))
            {
                localizedValue = localizedDictionary[key];
            }
            return localizedValue;
        }

        public IQueryable<ContentTypeLocalization> GetLocalizations(int portalId)
        {
            return Get(portalId).AsQueryable();
        }

        public void UpdateLocalization(ContentTypeLocalization item)
        {
            Requires.PropertyNotNullOrEmpty(item, "CultureCode");
            Requires.PropertyNotNullOrEmpty(item, "Key");
            Requires.PropertyNotNullOrEmpty(item, "Value");

            Update(item);

            ClearLocalizedCache(item.PortalId, item.CultureCode);
        }
    }
}
