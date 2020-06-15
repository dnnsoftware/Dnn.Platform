// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Localization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web.Hosting;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;

    public class LocalizationProvider : ComponentBase<ILocalizationProvider, LocalizationProvider>, ILocalizationProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocalizationProvider));

        public enum CustomizedLocale
        {
            None = 0,
            Portal = 1,
            Host = 2,
        }

        public string GetString(string key, string resourceFileRoot)
        {
            return this.GetString(key, resourceFileRoot, null, PortalController.Instance.GetCurrentPortalSettings(), false);
        }

        public string GetString(string key, string resourceFileRoot, string language)
        {
            return this.GetString(key, resourceFileRoot, language, PortalController.Instance.GetCurrentPortalSettings(), false);
        }

        public string GetString(string key, string resourceFileRoot, string language, PortalSettings portalSettings)
        {
            return this.GetString(key, resourceFileRoot, language, portalSettings, false);
        }

        public string GetString(string key, string resourceFileRoot, string language, PortalSettings portalSettings, bool disableShowMissingKeys)
        {
            // make the default translation property ".Text"
            if (key.IndexOf(".", StringComparison.Ordinal) < 1)
            {
                key += ".Text";
            }

            string resourceValue = Null.NullString;
            bool keyFound = TryGetStringInternal(key, language, resourceFileRoot, portalSettings, ref resourceValue);

            // If the key can't be found then it doesn't exist in the Localization Resources
            if (Localization.ShowMissingKeys && !disableShowMissingKeys)
            {
                if (keyFound)
                {
                    resourceValue = "[L]" + resourceValue;
                }
                else
                {
                    resourceValue = "RESX:" + key;
                }
            }

            if (!keyFound)
            {
                Logger.WarnFormat("Missing localization key. key:{0} resFileRoot:{1} threadCulture:{2} userlan:{3}", key, resourceFileRoot, Thread.CurrentThread.CurrentUICulture, language);
            }

            return string.IsNullOrEmpty(resourceValue) ? string.Empty : resourceValue;
        }

        /// <summary>
        /// Saves a string to a resource file.
        /// </summary>
        /// <param name="key">The key to save (e.g. "MyWidget.Text").</param>
        /// <param name="value">The text value for the key.</param>
        /// <param name="resourceFileRoot">Relative path for the resource file root (e.g. "DesktopModules/Admin/Lists/App_LocalResources/ListEditor.ascx.resx").</param>
        /// <param name="language">The locale code in lang-region format (e.g. "fr-FR").</param>
        /// <param name="portalSettings">The current portal settings.</param>
        /// <param name="resourceType">Specifies whether to save as portal, host or system resource file.</param>
        /// <param name="createFile">if set to <c>true</c> a new file will be created if it is not found.</param>
        /// <param name="createKey">if set to <c>true</c> a new key will be created if not found.</param>
        /// <returns>If the value could be saved then true will be returned, otherwise false.</returns>
        /// <exception cref="System.Exception">Any file io error or similar will lead to exceptions.</exception>
        public bool SaveString(string key, string value, string resourceFileRoot, string language, PortalSettings portalSettings, CustomizedLocale resourceType, bool createFile, bool createKey)
        {
            try
            {
                if (key.IndexOf(".", StringComparison.Ordinal) < 1)
                {
                    key += ".Text";
                }

                string resourceFileName = GetResourceFileName(resourceFileRoot, language);
                resourceFileName = resourceFileName.Replace("." + language.ToLowerInvariant() + ".", "." + language + ".");
                switch (resourceType)
                {
                    case CustomizedLocale.Host:
                        resourceFileName = resourceFileName.Replace(".resx", ".Host.resx");
                        break;
                    case CustomizedLocale.Portal:
                        resourceFileName = resourceFileName.Replace(".resx", ".Portal-" + portalSettings.PortalId + ".resx");
                        break;
                }

                resourceFileName = resourceFileName.TrimStart('~', '/', '\\');
                string filePath = HostingEnvironment.MapPath("~/" + resourceFileName);
                XmlDocument doc = null;
                if (File.Exists(filePath))
                {
                    doc = new XmlDocument { XmlResolver = null };
                    doc.Load(filePath);
                }
                else
                {
                    if (createFile)
                    {
                        doc = new XmlDocument { XmlResolver = null };
                        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                        XmlNode root = doc.CreateElement("root");
                        doc.AppendChild(root);
                        AddResourceFileNode(ref root, "resheader", "resmimetype", "text/microsoft-resx");
                        AddResourceFileNode(ref root, "resheader", "version", "2.0");
                        AddResourceFileNode(ref root, "resheader", "reader", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                        AddResourceFileNode(ref root, "resheader", "writer", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    }
                }

                if (doc == null)
                {
                    return false;
                }

                XmlNode reskeyNode = doc.SelectSingleNode("root/data[@name=\"" + key + "\"]");
                if (reskeyNode != null)
                {
                    reskeyNode.SelectSingleNode("value").InnerText = value;
                }
                else
                {
                    if (createKey)
                    {
                        XmlNode root = doc.SelectSingleNode("root");
                        AddResourceFileNode(ref root, "data", key, value);
                    }
                    else
                    {
                        return false;
                    }
                }

                doc.Save(filePath);
                DataCache.RemoveCache("/" + resourceFileName.ToLowerInvariant());
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error while trying to create resource in {0}", resourceFileRoot), ex);
            }
        }

        public Dictionary<string, string> GetCompiledResourceFile(PortalSettings portalSettings, string resourceFile, string locale)
        {
            return
                CBO.GetCachedObject<Dictionary<string, string>>(
                    new CacheItemArgs(
                    "Compiled-" + resourceFile + "-" + locale + "-" + portalSettings.PortalId,
                    DataCache.ResourceFilesCacheTimeOut, DataCache.ResourceFilesCachePriority, resourceFile, locale,
                    portalSettings), GetCompiledResourceFileCallBack, true);
        }

        private static object GetCompiledResourceFileCallBack(CacheItemArgs cacheItemArgs)
        {
            string resourceFile = (string)cacheItemArgs.Params[0];
            string locale = (string)cacheItemArgs.Params[1];
            PortalSettings portalSettings = (PortalSettings)cacheItemArgs.Params[2];
            string systemLanguage = Localization.SystemLocale;
            string defaultLanguage = portalSettings.DefaultLanguage;
            string fallbackLanguage = Localization.SystemLocale;
            Locale targetLocale = LocaleController.Instance.GetLocale(locale);
            if (!string.IsNullOrEmpty(targetLocale.Fallback))
            {
                fallbackLanguage = targetLocale.Fallback;
            }

            // get system default and merge the specific ones one by one
            var res = GetResourceFile(resourceFile);
            if (res == null)
            {
                return new Dictionary<string, string>();
            }

            // clone the dictionart so that when merge values into dictionart, it won't
            // affect the cache data.
            res = res.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            res = MergeResourceFile(res, GetResourceFileName(resourceFile, systemLanguage, portalSettings.PortalId));
            if (defaultLanguage != systemLanguage)
            {
                res = MergeResourceFile(res, GetResourceFileName(resourceFile, defaultLanguage));
                res = MergeResourceFile(res, GetResourceFileName(resourceFile, defaultLanguage, portalSettings.PortalId));
            }

            if (fallbackLanguage != defaultLanguage)
            {
                res = MergeResourceFile(res, GetResourceFileName(resourceFile, fallbackLanguage));
                res = MergeResourceFile(res, GetResourceFileName(resourceFile, fallbackLanguage, portalSettings.PortalId));
            }

            if (locale != fallbackLanguage)
            {
                res = MergeResourceFile(res, GetResourceFileName(resourceFile, locale));
                res = MergeResourceFile(res, GetResourceFileName(resourceFile, locale, portalSettings.PortalId));
            }

            return res;
        }

        private static Dictionary<string, string> MergeResourceFile(Dictionary<string, string> current, string resourceFile)
        {
            var resFile = GetResourceFile(resourceFile);
            if (resFile == null)
            {
                return current;
            }

            foreach (string key in current.Keys.ToList())
            {
                if (resFile.ContainsKey(key))
                {
                    current[key] = resFile[key];
                }
            }

            return current;
        }

        /// <summary>
        /// Adds one of either a "resheader" or "data" element to resxRoot (which should be the root element of the resx file).
        /// This function is used to construct new resource files and to add resource keys to an existing file.
        /// </summary>
        /// <param name="resxRoot">The RESX root.</param>
        /// <param name="elementName">Name of the element ("resheader" or "data").</param>
        /// <param name="nodeName">Name of the node (in case of "data" specify the localization key here, e.g. "MyWidget.Text").</param>
        /// <param name="nodeValue">The node value (text value to use).</param>
        private static void AddResourceFileNode(ref XmlNode resxRoot, string elementName, string nodeName, string nodeValue)
        {
            XmlNode newNode = resxRoot.AddElement(elementName, string.Empty).AddAttribute("name", nodeName);
            if (elementName == "data")
            {
                newNode = newNode.AddAttribute("xml:space", "preserve");
            }

            newNode.AddElement("value", nodeValue);
        }

        private static object GetResourceFileCallBack(CacheItemArgs cacheItemArgs)
        {
            string cacheKey = cacheItemArgs.CacheKey;
            Dictionary<string, string> resources = null;

            string filePath = null;
            try
            {
                // Get resource file lookup to determine if the resource file even exists
                SharedDictionary<string, bool> resourceFileExistsLookup = GetResourceFileLookupDictionary();

                if (ResourceFileMayExist(resourceFileExistsLookup, cacheKey))
                {
                    // check if an absolute reference for the resource file was used
                    if (cacheKey.Contains(":\\") && Path.IsPathRooted(cacheKey))
                    {
                        // if an absolute reference, check that the file exists
                        if (File.Exists(cacheKey))
                        {
                            filePath = cacheKey;
                        }
                    }

                    // no filepath found from an absolute reference, try and map the path to get the file path
                    if (filePath == null)
                    {
                        filePath = HostingEnvironment.MapPath(Globals.ApplicationPath + cacheKey);
                    }

                    // The file is not in the lookup, or we know it exists as we have found it before
                    if (File.Exists(filePath))
                    {
                        if (filePath != null)
                        {
                            var doc = new XPathDocument(filePath);
                            resources = new Dictionary<string, string>();
                            foreach (XPathNavigator nav in doc.CreateNavigator().Select("root/data"))
                            {
                                if (nav.NodeType != XPathNodeType.Comment)
                                {
                                    var selectSingleNode = nav.SelectSingleNode("value");
                                    if (selectSingleNode != null)
                                    {
                                        resources[nav.GetAttribute("name", string.Empty)] = selectSingleNode.Value;
                                    }
                                }
                            }
                        }

                        cacheItemArgs.CacheDependency = new DNNCacheDependency(filePath);

                        // File exists so add it to lookup with value true, so we are safe to try again
                        using (resourceFileExistsLookup.GetWriteLock())
                        {
                            resourceFileExistsLookup[cacheKey] = true;
                        }
                    }
                    else
                    {
                        // File does not exist so add it to lookup with value false, so we don't try again
                        using (resourceFileExistsLookup.GetWriteLock())
                        {
                            resourceFileExistsLookup[cacheKey] = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("The following resource file caused an error while reading: {0}", filePath), ex);
            }

            return resources;
        }

        private static SharedDictionary<string, bool> GetResourceFileLookupDictionary()
        {
            return
                CBO.GetCachedObject<SharedDictionary<string, bool>>(
                    new CacheItemArgs(DataCache.ResourceFileLookupDictionaryCacheKey, DataCache.ResourceFileLookupDictionaryTimeOut, DataCache.ResourceFileLookupDictionaryCachePriority),
                    c => new SharedDictionary<string, bool>(),
                    true);
        }

        private static Dictionary<string, string> GetResourceFile(string resourceFile)
        {
            return CBO.GetCachedObject<Dictionary<string, string>>(
                new CacheItemArgs(resourceFile, DataCache.ResourceFilesCacheTimeOut, DataCache.ResourceFilesCachePriority),
                GetResourceFileCallBack,
                true);
        }

        private static string GetResourceFileName(string resourceFileRoot, string language, int portalId)
        {
            string resourceFile = GetResourceFileName(resourceFileRoot, language);
            if (portalId != -1)
            {
                resourceFile = resourceFile.Replace(".resx", ".Portal-" + portalId + ".resx");
            }

            return resourceFile;
        }

        private static string GetResourceFileName(string resourceFileRoot, string language)
        {
            string resourceFile;
            language = language.ToLowerInvariant();
            if (resourceFileRoot != null)
            {
                if (language == Localization.SystemLocale.ToLowerInvariant() || string.IsNullOrEmpty(language))
                {
                    switch (resourceFileRoot.Substring(resourceFileRoot.Length - 5, 5).ToLowerInvariant())
                    {
                        case ".resx":
                            resourceFile = resourceFileRoot;
                            break;
                        case ".ascx":
                            resourceFile = resourceFileRoot + ".resx";
                            break;
                        case ".aspx":
                            resourceFile = resourceFileRoot + ".resx";
                            break;
                        default:
                            resourceFile = resourceFileRoot + ".ascx.resx"; // a portal module
                            break;
                    }
                }
                else
                {
                    switch (resourceFileRoot.Substring(resourceFileRoot.Length - 5, 5).ToLowerInvariant())
                    {
                        case ".resx":
                            resourceFile = resourceFileRoot.Replace(".resx", "." + language + ".resx");
                            break;
                        case ".ascx":
                            resourceFile = resourceFileRoot.Replace(".ascx", ".ascx." + language + ".resx");
                            break;
                        case ".aspx":
                            resourceFile = resourceFileRoot.Replace(".aspx", ".aspx." + language + ".resx");
                            break;
                        default:
                            resourceFile = resourceFileRoot + ".ascx." + language + ".resx";
                            break;
                    }
                }
            }
            else
            {
                if (language == Localization.SystemLocale.ToLowerInvariant() || string.IsNullOrEmpty(language))
                {
                    resourceFile = Localization.SharedResourceFile;
                }
                else
                {
                    resourceFile = Localization.SharedResourceFile.Replace(".resx", "." + language + ".resx");
                }
            }

            return resourceFile;
        }

        private static bool ResourceFileMayExist(SharedDictionary<string, bool> resourceFileExistsLookup, string cacheKey)
        {
            bool mayExist;
            using (resourceFileExistsLookup.GetReadLock())
            {
                mayExist = !resourceFileExistsLookup.ContainsKey(cacheKey) || resourceFileExistsLookup[cacheKey];
            }

            return mayExist;
        }

        private static bool TryGetFromResourceFile(string key, string resourceFile, string userLanguage, string fallbackLanguage, string defaultLanguage, int portalID, ref string resourceValue)
        {
            // Try the user's language first
            bool bFound = TryGetFromResourceFile(key, GetResourceFileName(resourceFile, userLanguage), portalID, ref resourceValue);

            if (!bFound && fallbackLanguage != userLanguage)
            {
                // Try fallback language next
                bFound = TryGetFromResourceFile(key, GetResourceFileName(resourceFile, fallbackLanguage), portalID, ref resourceValue);
            }

            if (!bFound && !(defaultLanguage == userLanguage || defaultLanguage == fallbackLanguage))
            {
                // Try default Language last
                bFound = TryGetFromResourceFile(key, GetResourceFileName(resourceFile, defaultLanguage), portalID, ref resourceValue);
            }

            return bFound;
        }

        private static bool TryGetFromResourceFile(string key, string resourceFile, int portalID, ref string resourceValue)
        {
            // Try Portal Resource File
            bool bFound = TryGetFromResourceFile(key, resourceFile, portalID, CustomizedLocale.Portal, ref resourceValue);
            if (!bFound)
            {
                // Try Host Resource File
                bFound = TryGetFromResourceFile(key, resourceFile, portalID, CustomizedLocale.Host, ref resourceValue);
            }

            if (!bFound)
            {
                // Try Portal Resource File
                bFound = TryGetFromResourceFile(key, resourceFile, portalID, CustomizedLocale.None, ref resourceValue);
            }

            return bFound;
        }

        private static bool TryGetStringInternal(string key, string userLanguage, string resourceFile, PortalSettings portalSettings, ref string resourceValue)
        {
            string defaultLanguage = Null.NullString;
            string fallbackLanguage = Localization.SystemLocale;
            int portalId = Null.NullInteger;

            // Get the default language
            if (portalSettings != null)
            {
                defaultLanguage = portalSettings.DefaultLanguage;
                portalId = portalSettings.PortalId;
            }

            // Set the userLanguage if not passed in
            if (string.IsNullOrEmpty(userLanguage))
            {
                userLanguage = Thread.CurrentThread.CurrentUICulture.ToString();
            }

            // Default the userLanguage to the defaultLanguage if not set
            if (string.IsNullOrEmpty(userLanguage))
            {
                userLanguage = defaultLanguage;
            }

            Locale userLocale = null;
            try
            {
                if (Globals.Status != Globals.UpgradeStatus.Install)
                {
                    // Get Fallback language, but not when we are installing (because we may not have a database yet)
                    userLocale = LocaleController.Instance.GetLocale(userLanguage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            if (userLocale != null && !string.IsNullOrEmpty(userLocale.Fallback))
            {
                fallbackLanguage = userLocale.Fallback;
            }

            if (string.IsNullOrEmpty(resourceFile))
            {
                resourceFile = Localization.SharedResourceFile;
            }

            // Try the resource file for the key
            bool bFound = TryGetFromResourceFile(key, resourceFile, userLanguage, fallbackLanguage, defaultLanguage, portalId, ref resourceValue);
            if (!bFound)
            {
                if (Localization.SharedResourceFile.ToLowerInvariant() != resourceFile.ToLowerInvariant())
                {
                    // try to use a module specific shared resource file
                    string localSharedFile = resourceFile.Substring(0, resourceFile.LastIndexOf("/", StringComparison.Ordinal) + 1) + Localization.LocalSharedResourceFile;

                    if (localSharedFile.ToLowerInvariant() != resourceFile.ToLowerInvariant())
                    {
                        bFound = TryGetFromResourceFile(key, localSharedFile, userLanguage, fallbackLanguage, defaultLanguage, portalId, ref resourceValue);
                    }
                }
            }

            if (!bFound)
            {
                if (Localization.SharedResourceFile.ToLowerInvariant() != resourceFile.ToLowerInvariant())
                {
                    bFound = TryGetFromResourceFile(key, Localization.SharedResourceFile, userLanguage, fallbackLanguage, defaultLanguage, portalId, ref resourceValue);
                }
            }

            return bFound;
        }

        private static bool TryGetFromResourceFile(string key, string resourceFile, int portalID, CustomizedLocale resourceType, ref string resourceValue)
        {
            bool bFound = Null.NullBoolean;
            string resourceFileName = resourceFile;
            switch (resourceType)
            {
                case CustomizedLocale.Host:
                    resourceFileName = resourceFile.Replace(".resx", ".Host.resx");
                    break;
                case CustomizedLocale.Portal:
                    resourceFileName = resourceFile.Replace(".resx", ".Portal-" + portalID + ".resx");
                    break;
            }

            if (resourceFileName.StartsWith("desktopmodules", StringComparison.InvariantCultureIgnoreCase)
                || resourceFileName.StartsWith("admin", StringComparison.InvariantCultureIgnoreCase)
                || resourceFileName.StartsWith("controls", StringComparison.InvariantCultureIgnoreCase))
            {
                resourceFileName = "~/" + resourceFileName;
            }

            // Local resource files are either named ~/... or <ApplicationPath>/...
            // The following logic creates a cachekey of /....
            string cacheKey = resourceFileName.Replace("~/", "/").ToLowerInvariant();
            if (!string.IsNullOrEmpty(Globals.ApplicationPath))
            {
                if (Globals.ApplicationPath != "/portals")
                {
                    if (cacheKey.StartsWith(Globals.ApplicationPath))
                    {
                        cacheKey = cacheKey.Substring(Globals.ApplicationPath.Length);
                    }
                }
                else
                {
                    cacheKey = "~" + cacheKey;
                    if (cacheKey.StartsWith("~" + Globals.ApplicationPath))
                    {
                        cacheKey = cacheKey.Substring(Globals.ApplicationPath.Length + 1);
                    }
                }
            }

            // Get resource file lookup to determine if the resource file even exists
            SharedDictionary<string, bool> resourceFileExistsLookup = GetResourceFileLookupDictionary();

            if (ResourceFileMayExist(resourceFileExistsLookup, cacheKey))
            {
                // File is not in lookup or its value is true so we know it exists
                Dictionary<string, string> dicResources = GetResourceFile(cacheKey);
                if (dicResources != null)
                {
                    bFound = dicResources.TryGetValue(key, out resourceValue);
                }
            }

            return bFound;
        }
    }
}
