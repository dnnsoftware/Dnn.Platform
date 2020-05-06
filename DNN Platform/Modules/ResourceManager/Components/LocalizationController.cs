// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Caching;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Services.Cache;
using Dnn.Modules.ResourceManager.Components.Models;

namespace Dnn.Modules.ResourceManager.Components
{
    public class LocalizationController : ServiceLocator<ILocalizationController, LocalizationController>, ILocalizationController
    {

        /// <summary>
        /// Provides a method to override Service Locator's GetFactory method
        /// </summary>
        /// <returns></returns>
        protected override Func<ILocalizationController> GetFactory()
        {
            return () => new LocalizationControllerImpl();
        }


        public string CultureName => Instance.CultureName;

        public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture)
            => Instance.GetLocalizedDictionary(resourceFile, culture);

        public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture,
            Localization localization) => Instance.GetLocalizedDictionary(resourceFile, culture, localization);

        public long GetResxTimeStamp(string resourceFile, Localization localization)
            => Instance.GetResxTimeStamp(resourceFile, localization);


        #region implementation

        class LocalizationControllerImpl : ILocalizationController
        {
            private static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);
            private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);

            public string CultureName
            {
                get { return Thread.CurrentThread.CurrentUICulture.Name; }
            }

            public long GetResxTimeStamp(string resourceFile, Localization localization)
            {
                return GetLastModifiedTime(resourceFile, CultureName, localization).Ticks;
            }

            public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture,
                Localization localization)
            {
                Requires.NotNullOrEmpty("resourceFile", resourceFile);
                Requires.NotNullOrEmpty("culture", culture);

                var cacheKey = string.Format(localization.ResxDataCacheKey, culture, resourceFile);
                var localizedDict = DataCache.GetCache(cacheKey) as Dictionary<string, string>;
                if (localizedDict != null) return localizedDict;

                var dictionary = new Dictionary<string, string>();


                foreach (
                    var kvp in
                        GetLocalizationValues(resourceFile, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
                {
                    dictionary[kvp.Key] = kvp.Value;
                }

                DataCache.SetCache(cacheKey, dictionary, (DNNCacheDependency) null,
                    Cache.NoAbsoluteExpiration, FiveMinutes, CacheItemPriority.Normal, null);

                return dictionary;
            }

            private DateTime GetLastModifiedTime(string resourceFile, string culture, Localization localization)
            {
                Requires.NotNullOrEmpty("culture", culture);

                var cacheKey = string.Format(localization.ResxModifiedDateCacheKey, culture);
                var cachedData = DataCache.GetCache(cacheKey);
                if (cachedData is DateTime) return (DateTime) DataCache.GetCache(cacheKey);
                var lastModifiedDate = GetLastModifiedTimeInternal(resourceFile, culture);


                DataCache.SetCache(cacheKey, lastModifiedDate, (DNNCacheDependency) null,
                    Cache.NoAbsoluteExpiration, OneHour, CacheItemPriority.Normal, null);

                return lastModifiedDate;
            }

            private DateTime GetLastModifiedTimeInternal(string resourceFile, string culture)
            {

                var cultureSpecificFile =
                    System.Web.HttpContext.Current.Server.MapPath(resourceFile.Replace(".resx", "") + "." + culture +
                                                                  ".resx");
                var lastModifiedDate = DateTime.MinValue;

                if (File.Exists(cultureSpecificFile))
                {
                    lastModifiedDate = File.GetLastWriteTime(cultureSpecificFile);
                }
                else
                {
                    var cultureNeutralFile = System.Web.HttpContext.Current.Server.MapPath(resourceFile);
                    if (File.Exists(cultureNeutralFile))
                    {
                        lastModifiedDate = File.GetLastWriteTime(cultureNeutralFile);
                    }

                }
                return lastModifiedDate;
            }

            public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture)
            {
                Requires.NotNullOrEmpty("resourceFile", resourceFile);
                Requires.NotNullOrEmpty("culture", culture);

                var cacheKey = string.Format(Constants.LocalizationDataCacheKey, culture, resourceFile);
                var localizedDict = DataCache.GetCache(cacheKey) as Dictionary<string, string>;
                if (localizedDict != null) return localizedDict;

                var dictionary = new Dictionary<string, string>();
                foreach (
                    var kvp in
                        GetLocalizationValues(resourceFile, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
                {
                    dictionary[kvp.Key] = kvp.Value;
                }

                DataCache.SetCache(cacheKey, dictionary, (DNNCacheDependency) null,
                    Cache.NoAbsoluteExpiration, Constants.FiveMinutes, CacheItemPriority.Normal, null);

                return dictionary;
            }

            #region Private Methods

            private static string GetNameAttribute(XmlNode node)
            {
                if (node.Attributes != null)
                {
                    var attribute = node.Attributes.GetNamedItem("name");
                    if (attribute != null)
                    {
                        return attribute.Value;
                    }
                }

                return null;
            }

            private static void AssertHeaderValue(IEnumerable<XmlNode> headers, string key, string value)
            {
                var header =
                    headers.FirstOrDefault(
                        x => GetNameAttribute(x).Equals(key, StringComparison.InvariantCultureIgnoreCase));
                if (header != null)
                {
                    if (!header.InnerText.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new ApplicationException(string.Format("Resource header '{0}' != '{1}'", key, value));
                    }
                }
                else
                {
                    throw new ApplicationException(string.Format("Resource header '{0}' is missing", key));
                }
            }

            private static IEnumerable<KeyValuePair<string, string>> GetLocalizationValues(string fullPath,
                string culture)
            {
                using (
                    var stream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(fullPath), FileMode.Open,
                        FileAccess.Read))
                {
                    var document = new XmlDocument { XmlResolver = null };
                    document.Load(stream);

                    // ReSharper disable AssignNullToNotNullAttribute
                    var headers = document.SelectNodes(@"/root/resheader").Cast<XmlNode>().ToArray();
                    // ReSharper restore AssignNullToNotNullAttribute

                    AssertHeaderValue(headers, "resmimetype", "text/microsoft-resx");

                    // ReSharper disable AssignNullToNotNullAttribute
                    foreach (var xmlNode in document.SelectNodes("/root/data").Cast<XmlNode>())
                        // ReSharper restore AssignNullToNotNullAttribute
                    {
                        var name = GetNameAttribute(xmlNode);

                        const string textPostFix = ".Text";
                        if (name.EndsWith(textPostFix))
                        {
                            name = name.Substring(0, name.Length - textPostFix.Length);
                        }

                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }

                        var key = name;
                        if (key.Contains("."))
                        {
                            key = name + textPostFix;
                        }

                        var value = DotNetNuke.Services.Localization.Localization.GetString(key, fullPath, culture);

                        yield return new KeyValuePair<string, string>(name, value);
                    }
                }
            }

            #endregion
        }

        #endregion

    }
}