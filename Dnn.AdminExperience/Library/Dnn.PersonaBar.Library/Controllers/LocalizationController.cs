// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web.Caching;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Cache;

    public class LocalizationController : ServiceLocator<ILocalizationController, LocalizationController>, ILocalizationController
    {
        public static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan OneHour = TimeSpan.FromHours(1);

        /// <inheritdoc/>
        public string CultureName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name; }
        }

        /// <inheritdoc/>
        public long GetResxTimeStamp(string resourceFile, Dto.Localization localization)
        {
            return GetLastModifiedTime(resourceFile, this.CultureName, localization).Ticks;
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture, Dto.Localization localization)
        {
            Requires.NotNullOrEmpty("resourceFile", resourceFile);
            Requires.NotNullOrEmpty("culture", culture);

            var cacheKey = string.Format(CultureInfo.CurrentCulture, localization.ResxDataCacheKey, culture, resourceFile);
            if (DataCache.GetCache(cacheKey) is Dictionary<string, string> localizedDict)
            {
                return localizedDict;
            }

            var dictionary = new Dictionary<string, string>();

            foreach (var kvp in GetLocalizationValues(resourceFile, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
            {
                dictionary[kvp.Key] = kvp.Value;
            }

            DataCache.SetCache(
                cacheKey,
                dictionary,
                (DNNCacheDependency)null,
                Cache.NoAbsoluteExpiration,
                FiveMinutes,
                CacheItemPriority.Normal,
                null);

            return dictionary;
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture)
        {
            Requires.NotNullOrEmpty("resourceFile", resourceFile);
            Requires.NotNullOrEmpty("culture", culture);

            var dictionary = new Dictionary<string, string>();
            foreach (var kvp in GetLocalizationValues(resourceFile, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
            {
                dictionary[kvp.Key] = kvp.Value;
            }

            return dictionary;
        }

        /// <inheritdoc/>
        protected override Func<ILocalizationController> GetFactory()
        {
            return () => new LocalizationController();
        }

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

        private static string GetNameAttribute(XPathNavigator navigator)
        {
            return navigator.GetAttribute("name", string.Empty);
        }

        private static void AssertHeaderValue(IEnumerable<XmlNode> headers, string key, string value)
        {
            var header = headers.FirstOrDefault(x => GetNameAttribute(x).Equals(key, StringComparison.OrdinalIgnoreCase));
            if (header != null)
            {
                if (!header.InnerText.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new LocalizationException($"Resource header '{key}' != '{value}'");
                }
            }
            else
            {
                throw new LocalizationException($"Resource header '{key}' is missing");
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> GetLocalizationValues(string fullPath, string culture)
        {
            using (var stream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(fullPath), FileMode.Open, FileAccess.Read))
            {
                var document = new XmlDocument { XmlResolver = null };
                document.Load(stream);

                // ReSharper disable once AssignNullToNotNullAttribute
                var headers = document.SelectNodes(@"/root/resheader").Cast<XmlNode>().ToArray();

                AssertHeaderValue(headers, "resmimetype", "text/microsoft-resx");

                // ReSharper disable once AssignNullToNotNullAttribute
                foreach (XPathNavigator navigator in document.CreateNavigator().Select("/root/data"))
                {
                    if (navigator.NodeType == XPathNodeType.Comment)
                    {
                        continue;
                    }

                    var name = GetNameAttribute(navigator);

                    const string textPostFix = ".Text";
                    if (name.EndsWith(textPostFix))
                    {
                        name = name.Substring(0, name.Length - textPostFix.Length);
                    }

                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    var valueNode = navigator.SelectSingleNode("value");
                    if (valueNode != null)
                    {
                        yield return new KeyValuePair<string, string>(name, valueNode.Value);
                    }
                }
            }
        }

        private static DateTime GetLastModifiedTime(string resourceFile, string culture, Dto.Localization localization)
        {
            Requires.NotNullOrEmpty("culture", culture);

            var cacheKey = string.Format(CultureInfo.CurrentCulture, localization.ResxModifiedDateCacheKey, culture);
            var cachedData = DataCache.GetCache(cacheKey);
            if (cachedData is DateTime)
            {
                return (DateTime)DataCache.GetCache(cacheKey);
            }

            var lastModifiedDate = GetLastModifiedTimeInternal(resourceFile, culture);

            DataCache.SetCache(
                cacheKey,
                lastModifiedDate,
                (DNNCacheDependency)null,
                Cache.NoAbsoluteExpiration,
                OneHour,
                CacheItemPriority.Normal,
                null);

            return lastModifiedDate;
        }

        private static DateTime GetLastModifiedTimeInternal(string resourceFile, string culture)
        {
            var cultureSpecificFile = System.Web.HttpContext.Current.Server.MapPath($"{resourceFile.Replace(".resx", string.Empty)}.{culture}.resx");
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
    }
}
