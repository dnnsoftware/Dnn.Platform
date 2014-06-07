#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web.Hosting;
using System.Xml.XPath;

using DotNetNuke.Collections.Internal;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Cache;

namespace DotNetNuke.Services.Localization
{
    public class LocalizationProvider : ComponentBase<ILocalizationProvider, LocalizationProvider>, ILocalizationProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocalizationProvider));
        #region Nested type: CustomizedLocale

        private enum CustomizedLocale
        {
            None = 0,
            Portal = 1,
            Host = 2
        }

        #endregion

        #region Implementation of ILocalizationProvider

        public string GetString(string key, string resourceFileRoot)
        {
            return GetString(key, resourceFileRoot, null, PortalController.Instance.GetCurrentPortalSettings(), false);
        }

        public string GetString(string key, string resourceFileRoot, string language)
        {
            return GetString(key, resourceFileRoot, language, PortalController.Instance.GetCurrentPortalSettings(), false);
        }

        public string GetString(string key, string resourceFileRoot, string language, PortalSettings portalSettings)
        {
            return GetString(key, resourceFileRoot, language, portalSettings, false);
        }

        public string GetString(string key, string resourceFileRoot, string language, PortalSettings portalSettings, bool disableShowMissingKeys)
        {
            //make the default translation property ".Text"
            if (key.IndexOf(".", StringComparison.Ordinal) < 1)
            {
                key += ".Text";
            }
            string resourceValue = Null.NullString;
            bool keyFound = TryGetStringInternal(key, language, resourceFileRoot, portalSettings, ref resourceValue);

            //If the key can't be found then it doesn't exist in the Localization Resources
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

            return resourceValue;
        }

        #endregion

        private static object GetResourceFileCallBack(CacheItemArgs cacheItemArgs)
        {
            string cacheKey = cacheItemArgs.CacheKey;
            Dictionary<string, string> resources = null;

            string filePath = null;
            try
            {
                //Get resource file lookup to determine if the resource file even exists
                SharedDictionary<string, bool> resourceFileExistsLookup = GetResourceFileLookupDictionary();

                if (ResourceFileMayExist(resourceFileExistsLookup, cacheKey))
                {
                    //check if an absolute reference for the resource file was used
                    if (cacheKey.Contains(":\\") && Path.IsPathRooted(cacheKey))
                    {
                        //if an absolute reference, check that the file exists
                        if (File.Exists(cacheKey))
                        {
                            filePath = cacheKey;
                        }
                    }

                    //no filepath found from an absolute reference, try and map the path to get the file path
                    if (filePath == null)
                    {
                        filePath = HostingEnvironment.MapPath(Globals.ApplicationPath + cacheKey);
                    }

                    //The file is not in the lookup, or we know it exists as we have found it before
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
                                        resources[nav.GetAttribute("name", String.Empty)] = selectSingleNode.Value;
                                    }
                                }
                            }
                        }
                        cacheItemArgs.CacheDependency = new DNNCacheDependency(filePath);

                        //File exists so add it to lookup with value true, so we are safe to try again
                        using (resourceFileExistsLookup.GetWriteLock())
                        {
                            resourceFileExistsLookup[cacheKey] = true;
                        }
                    }
                    else
                    {
                        //File does not exist so add it to lookup with value false, so we don't try again
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
            return CBO.GetCachedObject<Dictionary<string, string>>(new CacheItemArgs(resourceFile, DataCache.ResourceFilesCacheTimeOut, DataCache.ResourceFilesCachePriority),
                                                                   GetResourceFileCallBack,
                                                                   true);
        }

        private static string GetResourceFileName(string resourceFileRoot, string language)
        {
            string resourceFile;
            language = language.ToLower();
            if (resourceFileRoot != null)
            {
                if (language == Localization.SystemLocale.ToLower() || String.IsNullOrEmpty(language))
                {
                    switch (resourceFileRoot.Substring(resourceFileRoot.Length - 5, 5).ToLower())
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
                            resourceFile = resourceFileRoot + ".ascx.resx"; //a portal module
                            break;
                    }
                }
                else
                {
                    switch (resourceFileRoot.Substring(resourceFileRoot.Length - 5, 5).ToLower())
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
                if (language == Localization.SystemLocale.ToLower() || String.IsNullOrEmpty(language))
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
            //Try the user's language first
            bool bFound = TryGetFromResourceFile(key, GetResourceFileName(resourceFile, userLanguage), portalID, ref resourceValue);

            if (!bFound && fallbackLanguage != userLanguage)
            {
                //Try fallback language next
                bFound = TryGetFromResourceFile(key, GetResourceFileName(resourceFile, fallbackLanguage), portalID, ref resourceValue);
            }
            if (!bFound && !(defaultLanguage == userLanguage || defaultLanguage == fallbackLanguage))
            {
                //Try default Language last
                bFound = TryGetFromResourceFile(key, GetResourceFileName(resourceFile, defaultLanguage), portalID, ref resourceValue);
            }
            return bFound;
        }

        private static bool TryGetFromResourceFile(string key, string resourceFile, int portalID, ref string resourceValue)
        {
            //Try Portal Resource File
            bool bFound = TryGetFromResourceFile(key, resourceFile, portalID, CustomizedLocale.Portal, ref resourceValue);
            if (!bFound)
            {
                //Try Host Resource File
                bFound = TryGetFromResourceFile(key, resourceFile, portalID, CustomizedLocale.Host, ref resourceValue);
            }
            if (!bFound)
            {
                //Try Portal Resource File
                bFound = TryGetFromResourceFile(key, resourceFile, portalID, CustomizedLocale.None, ref resourceValue);
            }
            return bFound;
        }

        private static bool TryGetStringInternal(string key, string userLanguage, string resourceFile, PortalSettings portalSettings, ref string resourceValue)
        {
            string defaultLanguage = Null.NullString;
            string fallbackLanguage = Localization.SystemLocale;
            int portalId = Null.NullInteger;

            //Get the default language
            if (portalSettings != null)
            {
                defaultLanguage = portalSettings.DefaultLanguage;
                portalId = portalSettings.PortalId;
            }

            //Set the userLanguage if not passed in
            if (String.IsNullOrEmpty(userLanguage))
            {
                userLanguage = Thread.CurrentThread.CurrentUICulture.ToString();
            }

            //Default the userLanguage to the defaultLanguage if not set
            if (String.IsNullOrEmpty(userLanguage))
            {
                userLanguage = defaultLanguage;
            }
            Locale userLocale = null;
            try
            {
                if (Globals.Status != Globals.UpgradeStatus.Install)
                {
                    //Get Fallback language, but not when we are installing (because we may not have a database yet)
                    userLocale = LocaleController.Instance.GetLocale(userLanguage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            if (userLocale != null && !String.IsNullOrEmpty(userLocale.Fallback))
            {
                fallbackLanguage = userLocale.Fallback;
            }
            if (String.IsNullOrEmpty(resourceFile))
            {
                resourceFile = Localization.SharedResourceFile;
            }

            //Try the resource file for the key
            bool bFound = TryGetFromResourceFile(key, resourceFile, userLanguage, fallbackLanguage, defaultLanguage, portalId, ref resourceValue);
            if (!bFound)
            {
                if (Localization.SharedResourceFile.ToLowerInvariant() != resourceFile.ToLowerInvariant())
                {
                    //try to use a module specific shared resource file
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

            //Local resource files are either named ~/... or <ApplicationPath>/...
            //The following logic creates a cachekey of /....
            string cacheKey = resourceFileName.Replace("~/", "/").ToLowerInvariant();
            if (!String.IsNullOrEmpty(Globals.ApplicationPath))
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

            //Get resource file lookup to determine if the resource file even exists
            SharedDictionary<string, bool> resourceFileExistsLookup = GetResourceFileLookupDictionary();

            if (ResourceFileMayExist(resourceFileExistsLookup, cacheKey))
            {
                //File is not in lookup or its value is true so we know it exists
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