﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals.Internal;
    using DotNetNuke.Entities.Portals.Templates;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    // using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Web.Client;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;
    using IAbPortalSettings = DotNetNuke.Abstractions.Portals.IPortalSettings;

    /// <summary>PortalController provides business layer of portal.</summary>
    /// <remarks>
    /// DotNetNuke supports the concept of virtualised sites in a single install. This means that multiple sites,
    /// each potentially with multiple unique URL's, can exist in one instance of DotNetNuke i.e. one set of files and one database.
    /// </remarks>
    public partial class PortalController : ServiceLocator<IPortalController, PortalController>, IPortalController
    {
        protected const string HttpContextKeyPortalSettingsDictionary = "PortalSettingsDictionary{0}{1}";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalController));

        /// <summary>Adds the portal dictionary.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        public static void AddPortalDictionary(int portalId, int tabId)
        {
            var portalDic = GetPortalDictionary();
            portalDic[tabId] = portalId;
            DataCache.SetCache(DataCache.PortalDictionaryCacheKey, portalDic);
        }

        /// <summary>Creates the root folder for a child portal.</summary>
        /// <remarks>
        /// If call this method, it will create the specific folder if the folder doesn't exist;
        /// and will copy subhost.aspx to the folder if there is no 'Default.aspx'.
        /// </remarks>
        /// <param name="childPath">The child path.</param>
        /// <returns>
        /// If the method executed successful, it will return NullString, otherwise return error message.
        /// </returns>
        /// <example>
        /// <code lang="C#">
        /// string childPhysicalPath = Server.MapPath(childPath);
        /// message = PortalController.CreateChildPortalFolder(childPhysicalPath);
        /// </code>
        /// </example>
        public static string CreateChildPortalFolder(string childPath)
        {
            string message = Null.NullString;

            // Set up Child Portal
            try
            {
                // create the subdirectory for the new portal
                if (!Directory.Exists(childPath))
                {
                    Directory.CreateDirectory(childPath);
                }

                // create the subhost default.aspx file
                if (!File.Exists(childPath + "\\" + Globals.glbDefaultPage))
                {
                    File.Copy(Globals.HostMapPath + "subhost.aspx", childPath + "\\" + Globals.glbDefaultPage);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                message += Localization.GetString("ChildPortal.Error") + exc.Message + exc.StackTrace;
            }

            return message;
        }

        /// <summary>Deletes all expired portals.</summary>
        /// <param name="serverPath">The server path.</param>
        public static void DeleteExpiredPortals(string serverPath)
        {
            foreach (PortalInfo portal in GetExpiredPortals())
            {
                DeletePortal(portal, serverPath);
            }
        }

        /// <summary>Deletes the portal.</summary>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>If the method executed successful, it will return NullString, otherwise return error message.</returns>
        public static string DeletePortal(PortalInfo portal, string serverPath)
        {
            string message = string.Empty;

            // check if this is the last portal
            var portals = Instance.GetPortals();
            if (portals.Count > 1)
            {
                if (portal != null)
                {
                    // delete custom resource files
                    Globals.DeleteFilesRecursive(serverPath, ".Portal-" + portal.PortalID + ".resx");

                    // If child portal delete child folder
                    var arr = PortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).ToList();
                    if (arr.Count > 0)
                    {
                        var portalAliasInfo = arr[0];
                        string portalName = Globals.GetPortalDomainName(portalAliasInfo.HTTPAlias, null, true);
                        if (portalAliasInfo.HTTPAlias.IndexOf("/", StringComparison.Ordinal) > -1)
                        {
                            portalName = GetPortalFolder(portalAliasInfo.HTTPAlias);
                        }

                        if (!string.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName))
                        {
                            DeletePortalFolder(serverPath, portalName);
                        }
                    }

                    // delete upload directory
                    if (!string.IsNullOrEmpty(portal.HomeDirectory))
                    {
                        var homeDirectory = portal.HomeDirectoryMapPath;

                        // check whether home directory is not used by other portal
                        // it happens when new portal creation failed, but home directory defined by user is already in use with other portal
                        var homeDirectoryInUse = portals.OfType<PortalInfo>().Any(x =>
                            x.PortalID != portal.PortalID &&
                            x.HomeDirectoryMapPath.Equals(homeDirectory, StringComparison.OrdinalIgnoreCase));

                        if (!homeDirectoryInUse)
                        {
                            if (Directory.Exists(homeDirectory))
                            {
                                Globals.DeleteFolderRecursive(homeDirectory);
                            }
                        }
                    }

                    // remove database references
                    DeletePortalInternal(portal.PortalID);
                }
            }
            else
            {
                message = Localization.GetString("LastPortal");
            }

            return message;
        }

        /// <summary>Get the portal folder froma child portal alias.</summary>
        /// <param name="alias">portal alias.</param>
        /// <returns>folder path of the child portal.</returns>
        public static string GetPortalFolder(string alias)
        {
            alias = alias.ToLowerInvariant().Replace("http://", string.Empty).Replace("https://", string.Empty);
            var appPath = Globals.ApplicationPath + "/";
            if (string.IsNullOrEmpty(Globals.ApplicationPath) || alias.IndexOf(appPath, StringComparison.InvariantCultureIgnoreCase) == Null.NullInteger)
            {
                return alias.Contains("/") ? alias.Substring(alias.IndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1) : string.Empty;
            }

            return alias.Substring(alias.IndexOf(appPath, StringComparison.InvariantCultureIgnoreCase) + appPath.Length);
        }

        /// <summary>Delete the child portal folder and try to remove its parent when parent folder is empty.</summary>
        /// <param name="serverPath">the server path.</param>
        /// <param name="portalFolder">the child folder path.</param>
        public static void DeletePortalFolder(string serverPath, string portalFolder)
        {
            var physicalPath = serverPath + portalFolder;
            Globals.DeleteFolderRecursive(physicalPath);

            // remove parent folder if its empty.
            var parentFolder = Directory.GetParent(physicalPath);
            while (parentFolder != null && !parentFolder.FullName.Equals(serverPath.TrimEnd('\\'), StringComparison.InvariantCultureIgnoreCase))
            {
                if (parentFolder.GetDirectories().Length + parentFolder.GetFiles().Length == 0)
                {
                    parentFolder.Delete();
                    parentFolder = parentFolder.Parent;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>Gets the portal dictionary.</summary>
        /// <returns>portal dictionary. the dictionary's Key -> Value is: TabId -> PortalId.</returns>
        public static Dictionary<int, int> GetPortalDictionary()
        {
            string cacheKey = string.Format(DataCache.PortalDictionaryCacheKey);
            return CBO.GetCachedObject<Dictionary<int, int>>(new CacheItemArgs(cacheKey, DataCache.PortalDictionaryTimeOut, DataCache.PortalDictionaryCachePriority), GetPortalDictionaryCallback);
        }

        /// <summary>GetPortalsByName gets all the portals whose name matches a provided filter expression.</summary>
        /// <param name="nameToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of PortalInfo objects.</returns>
        public static ArrayList GetPortalsByName(string nameToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            Type type = typeof(PortalInfo);
            return CBO.FillCollection(DataProvider.Instance().GetPortalsByName(nameToMatch, pageIndex, pageSize), ref type, ref totalRecords);
        }

        public static ArrayList GetPortalsByUser(int userId)
        {
            Type type = typeof(PortalInfo);
            return CBO.FillCollection(DataProvider.Instance().GetPortalsByUser(userId), type);
        }

        public static int GetEffectivePortalId(int portalId)
        {
            if (portalId > Null.NullInteger && Globals.Status != Globals.UpgradeStatus.Upgrade)
            {
                var portal = Instance.GetPortal(portalId);
                var portalGroup = (from p in PortalGroupController.Instance.GetPortalGroups()
                                   where p.PortalGroupId == portal.PortalGroupID
                                   select p)
                                .SingleOrDefault();

                if (portalGroup != null)
                {
                    portalId = portalGroup.MasterPortalId;
                }
            }

            return portalId;
        }

        /// <summary>Gets all expired portals.</summary>
        /// <returns>all expired portals as array list.</returns>
        public static ArrayList GetExpiredPortals()
        {
            return CBO.FillCollection(DataProvider.Instance().GetExpiredPortals(), typeof(PortalInfo));
        }

        /// <summary>Determines whether the portal is child portal.</summary>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>
        ///   <c>true</c> if the portal is child portal; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsChildPortal(PortalInfo portal, string serverPath)
        {
            bool isChild = Null.NullBoolean;
            var arr = PortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).ToList();
            if (arr.Count > 0)
            {
                PortalAliasInfo portalAlias = arr[0];
                var portalName = Globals.GetPortalDomainName(portalAlias.HTTPAlias, null, true);
                if (portalAlias.HTTPAlias.IndexOf("/") > -1)
                {
                    portalName = GetPortalFolder(portalAlias.HTTPAlias);
                }

                if (!string.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName))
                {
                    isChild = true;
                }
            }

            return isChild;
        }

        public static bool IsMemberOfPortalGroup(int portalId)
        {
            var portal = Instance.GetPortal(portalId);

            return portal != null && portal.PortalGroupID > Null.NullInteger;
        }

        /// <summary>Deletes the portal setting (neutral and for all languages).</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        public static void DeletePortalSetting(int portalID, string settingName)
        {
            DeletePortalSetting(portalID, settingName, Null.NullString);
        }

        /// <summary>Deletes the portal setting in this language.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="cultureCode">The culture code.</param>
        public static void DeletePortalSetting(int portalID, string settingName, string cultureCode)
        {
            DataProvider.Instance().DeletePortalSetting(portalID, settingName, cultureCode.ToLowerInvariant());
            EventLogController.Instance.AddLog("SettingName", settingName + ((cultureCode == Null.NullString) ? string.Empty : " (" + cultureCode + ")"), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_SETTING_DELETED);
            DataCache.ClearHostCache(true);
        }

        /// <summary>Deletes all portal settings by portal id.</summary>
        /// <param name="portalID">The portal ID.</param>
        public static void DeletePortalSettings(int portalID)
        {
            DeletePortalSettings(portalID, Null.NullString);
        }

        /// <summary>Deletes all portal settings by portal id and for a given language (Null: all languages and neutral settings).</summary>
        /// <param name="portalID">The portal ID.</param>
        public static void DeletePortalSettings(int portalID, string cultureCode)
        {
            DataProvider.Instance().DeletePortalSettings(portalID, cultureCode);
            EventLogController.Instance.AddLog("PortalID", portalID.ToString() + ((cultureCode == Null.NullString) ? string.Empty : " (" + cultureCode + ")"), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_SETTING_DELETED);
            DataCache.ClearHostCache(true);
        }

        /// <summary>takes in a text value, decrypts it with a FIPS compliant algorithm and returns the value.</summary>
        /// <param name="settingName">the setting to read.</param>
        /// <param name="passPhrase">the pass phrase used for encryption/decryption.</param>
        /// <returns>The decrypted setting value.</returns>
        public static string GetEncryptedString(string settingName, int portalID, string passPhrase)
        {
            Requires.NotNullOrEmpty("key", settingName);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);

            var cipherText = GetPortalSetting(settingName, portalID, string.Empty);

            return Security.FIPSCompliant.DecryptAES(cipherText, passPhrase, Host.Host.GUID);
        }

        /// <summary>Gets the portal setting.</summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static string GetPortalSetting(string settingName, int portalID, string defaultValue)
        {
            var retValue = Null.NullString;
            try
            {
                string setting;
                Instance.GetPortalSettings(portalID).TryGetValue(settingName, out setting);
                retValue = string.IsNullOrEmpty(setting) ? defaultValue : setting;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting for a specific language (or neutral).</summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        public static string GetPortalSetting(string settingName, int portalID, string defaultValue, string cultureCode)
        {
            var retValue = Null.NullString;
            try
            {
                string setting;
                Instance.GetPortalSettings(portalID, cultureCode).TryGetValue(settingName, out setting);
                retValue = string.IsNullOrEmpty(setting) ? defaultValue : setting;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting as boolean.</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static bool GetPortalSettingAsBoolean(string key, int portalID, bool defaultValue)
        {
            bool retValue = Null.NullBoolean;
            try
            {
                string setting;
                Instance.GetPortalSettings(portalID).TryGetValue(key, out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting as boolean for a specific language (or neutral).</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        public static bool GetPortalSettingAsBoolean(string key, int portalID, bool defaultValue, string cultureCode)
        {
            bool retValue = Null.NullBoolean;
            try
            {
                string setting;
                GetPortalSettingsDictionary(portalID, cultureCode).TryGetValue(key, out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting as integer.</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static int GetPortalSettingAsInteger(string key, int portalID, int defaultValue)
        {
            int retValue = Null.NullInteger;
            try
            {
                string setting;
                Instance.GetPortalSettings(portalID).TryGetValue(key, out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = Convert.ToInt32(setting);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting as double.</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal Id.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static double GetPortalSettingAsDouble(string key, int portalId, double defaultValue)
        {
            double retValue = Null.NullDouble;
            try
            {
                string setting;
                Instance.GetPortalSettings(portalId).TryGetValue(key, out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = Convert.ToDouble(setting);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting as integer for a specific language (or neutral).</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting (for specified lang, otherwise return defaultValue.</returns>
        public static int GetPortalSettingAsInteger(string key, int portalID, int defaultValue, string cultureCode)
        {
            int retValue = Null.NullInteger;
            try
            {
                string setting;
                GetPortalSettingsDictionary(portalID, cultureCode).TryGetValue(key, out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = Convert.ToInt32(setting);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>takes in a text value, encrypts it with a FIPS compliant algorithm and stores.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">host settings key.</param>
        /// <param name="settingValue">host settings value.</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption.</param>
        public static void UpdateEncryptedString(int portalID, string settingName, string settingValue, string passPhrase)
        {
            Requires.NotNullOrEmpty("key", settingName);
            Requires.PropertyNotNull("value", settingValue);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);

            var cipherText = Security.FIPSCompliant.EncryptAES(settingValue, passPhrase, Host.Host.GUID);

            UpdatePortalSetting(portalID, settingName, cipherText);
        }

        /// <summary>Updates a single neutral (not language specific) portal setting and clears it from the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue)
        {
            UpdatePortalSetting(portalID, settingName, settingValue, true);
        }

        /// <summary>Updates a single neutral (not language specific) portal setting, optionally without clearing the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <c>true</c> [clear cache].</param>
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache)
        {
            UpdatePortalSetting(portalID, settingName, settingValue, clearCache, Null.NullString, false);
        }

        /// <summary>Updates a language specific or neutral portal setting and clears it from the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="cultureCode">culture code for language specific settings, null string ontherwise.</param>
        [Obsolete("Deprecated in DNN 9.2.0. Use the overloaded one with the 'isSecure' parameter instead. Scheduled removal in v11.0.0.")]
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue, string cultureCode)
        {
            UpdatePortalSetting(portalID, settingName, settingValue, true, cultureCode, false);
        }

        /// <summary>Updates a language specific or neutral portal setting and optionally clears it from the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <c>true</c> [clear cache].</param>
        /// <param name="cultureCode">culture code for language specific settings, null string ontherwise.</param>
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode)
        {
            UpdatePortalSetting(portalID, settingName, settingValue, clearCache, cultureCode, false);
        }

        /// <summary>
        /// Updates a language specific or neutral portal setting and optionally clears it from the cache.
        /// All overloaded methors will not encrypt the setting value. Therefore, call this method whenever
        /// there is a need to encrypt the setting value before storing it in the datanbase.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <c>true</c> [clear cache].</param>
        /// <param name="cultureCode">culture code for language specific settings, null string ontherwise.</param>
        /// <param name="isSecure">When true it encrypt the value before storing it in the database.</param>
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode, bool isSecure)
        {
            Instance.UpdatePortalSetting(portalID, settingName, settingValue, clearCache, cultureCode, isSecure);
        }

        /// <summary>Checks the desktop modules whether is installed.</summary>
        /// <param name="nav">The nav.</param>
        /// <returns>Empty string if the module hasn't been installed, otherwise return the friendly name.</returns>
        public static string CheckDesktopModulesInstalled(XPathNavigator nav)
        {
            string friendlyName;
            DesktopModuleInfo desktopModule;
            StringBuilder modulesNotInstalled = new StringBuilder();

            foreach (XPathNavigator desktopModuleNav in nav.Select("portalDesktopModule"))
            {
                friendlyName = XmlUtils.GetNodeValue(desktopModuleNav, "friendlyname");

                if (!string.IsNullOrEmpty(friendlyName))
                {
                    desktopModule = DesktopModuleController.GetDesktopModuleByFriendlyName(friendlyName);
                    if (desktopModule == null)
                    {
                        // PE and EE templates have HTML as friendly name so check to make sure
                        // there is really no HTML module installed
                        if (friendlyName == "HTML")
                        {
                            desktopModule = DesktopModuleController.GetDesktopModuleByFriendlyName("HTML Pro");
                            if (desktopModule == null)
                            {
                                modulesNotInstalled.Append(friendlyName);
                                modulesNotInstalled.Append("<br/>");
                            }
                        }
                        else
                        {
                            modulesNotInstalled.Append(friendlyName);
                            modulesNotInstalled.Append("<br/>");
                        }
                    }
                }
            }

            return modulesNotInstalled.ToString();
        }

        /// <summary>
        ///   function provides the language for portalinfo requests
        ///   in case where language has not been installed yet, will return the core install default of en-us.
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns>The language identifier.</returns>
        public static string GetActivePortalLanguage(int portalID)
        {
            // get Language
            string language = Localization.SystemLocale;
            string tmpLanguage = GetPortalDefaultLanguage(portalID);
            var isDefaultLanguage = false;
            if (!string.IsNullOrEmpty(tmpLanguage))
            {
                language = tmpLanguage;
                isDefaultLanguage = true;
            }

            // handles case where portalcontroller methods invoked before a language is installed
            if (portalID > Null.NullInteger && Globals.Status == Globals.UpgradeStatus.None && Localization.ActiveLanguagesByPortalID(portalID) == 1)
            {
                return language;
            }

            if (HttpContext.Current != null && Globals.Status == Globals.UpgradeStatus.None)
            {
                if (HttpContext.Current.Request.QueryString["language"] != null)
                {
                    language = HttpContext.Current.Request.QueryString["language"];
                }
                else
                {
                    PortalSettings portalSettings = GetCurrentPortalSettingsInternal();
                    if (portalSettings != null && portalSettings.ActiveTab != null && !string.IsNullOrEmpty(portalSettings.ActiveTab.CultureCode))
                    {
                        language = portalSettings.ActiveTab.CultureCode;
                    }
                    else
                    {
                        // PortalSettings IS Nothing - probably means we haven't set it yet (in Begin Request)
                        // so try detecting the user's cookie
                        if (HttpContext.Current.Request["language"] != null)
                        {
                            language = HttpContext.Current.Request["language"];
                            isDefaultLanguage = false;
                        }

                        // if no cookie - try detecting browser
                        if ((string.IsNullOrEmpty(language) || isDefaultLanguage) && EnableBrowserLanguageInDefault(portalID))
                        {
                            CultureInfo culture = Localization.GetBrowserCulture(portalID);

                            if (culture != null)
                            {
                                language = culture.Name;
                            }
                        }
                    }
                }
            }

            return language;
        }

        /// <summary>return the current DefaultLanguage value from the Portals table for the requested Portalid.</summary>
        /// <param name="portalID"></param>
        /// <returns>The language identifier.</returns>
        public static string GetPortalDefaultLanguage(int portalID)
        {
            string cacheKey = string.Format("PortalDefaultLanguage_{0}", portalID);
            return CBO.GetCachedObject<string>(new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, portalID), GetPortalDefaultLanguageCallBack);
        }

        /// <summary>
        ///   set the required DefaultLanguage in the Portals table for a particular portal
        ///   saves having to update an entire PortalInfo object.
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="cultureCode"></param>
        public static void UpdatePortalDefaultLanguage(int portalID, string cultureCode)
        {
            DataProvider.Instance().UpdatePortalDefaultLanguage(portalID, cultureCode);

            // ensure localization record exists as new portal default language may be relying on fallback chain
            // of which it is now the final part
            DataProvider.Instance().EnsureLocalizationExists(portalID, cultureCode);
        }

        public static void IncrementCrmVersion(int portalID)
        {
            int currentVersion;
            var versionSetting = GetPortalSetting(ClientResourceSettings.VersionKey, portalID, "1");
            if (int.TryParse(versionSetting, out currentVersion))
            {
                var newVersion = currentVersion + 1;
                UpdatePortalSetting(portalID, ClientResourceSettings.VersionKey, newVersion.ToString(CultureInfo.InvariantCulture), true);
            }
        }

        public static void IncrementOverridingPortalsCrmVersion()
        {
            foreach (PortalInfo portal in Instance.GetPortals())
            {
                string setting = GetPortalSetting(ClientResourceSettings.OverrideDefaultSettingsKey, portal.PortalID, "False");
                bool overriden;

                // if this portal is overriding the host level...
                if (bool.TryParse(setting, out overriden) && overriden)
                {
                    // increment its version
                    IncrementCrmVersion(portal.PortalID);
                }
            }
        }

        /// <summary>Creates a new portal alias.</summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="portalAlias">Portal Alias to be created.</param>
        public void AddPortalAlias(int portalId, string portalAlias)
        {
            // Check if the Alias exists
            PortalAliasInfo portalAliasInfo = PortalAliasController.Instance.GetPortalAlias(portalAlias, portalId);

            // If alias does not exist add new
            if (portalAliasInfo == null)
            {
                portalAliasInfo = new PortalAliasInfo { PortalID = portalId, HTTPAlias = portalAlias, IsPrimary = true };
                PortalAliasController.Instance.AddPortalAlias(portalAliasInfo);
            }
        }

        /// <summary>Copies the page template.</summary>
        /// <param name="templateFile">The template file.</param>
        /// <param name="mappedHomeDirectory">The mapped home directory.</param>
        public void CopyPageTemplate(string templateFile, string mappedHomeDirectory)
        {
            string hostTemplateFile = string.Format("{0}Templates\\{1}", Globals.HostMapPath, templateFile);
            if (File.Exists(hostTemplateFile))
            {
                string portalTemplateFolder = string.Format("{0}Templates\\", mappedHomeDirectory);
                if (!Directory.Exists(portalTemplateFolder))
                {
                    // Create Portal Templates folder
                    Directory.CreateDirectory(portalTemplateFolder);
                }

                string portalTemplateFile = portalTemplateFolder + templateFile;
                if (!File.Exists(portalTemplateFile))
                {
                    File.Copy(hostTemplateFile, portalTemplateFile);
                }
            }
        }

        /// <summary>Creates the portal.</summary>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="adminUserId">The obj admin user.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="template"> </param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="childPath">The child path.</param>
        /// <param name="isChildPortal">if set to <c>true</c> means the portal is child portal.</param>
        /// <returns>Portal id.</returns>
        public int CreatePortal(string portalName, int adminUserId, string description, string keyWords, IPortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            // Attempt to create a new portal
            int portalId = CreatePortal(portalName, homeDirectory, template.CultureCode);

            // Log the portal if into http context, if exception occurred in next step, we can remove the portal which is not really created.
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items.Add("CreatingPortalId", portalId);
            }

            string message = Null.NullString;

            if (portalId != -1)
            {
                // add administrator
                int administratorId = adminUserId;
                var adminUser = new UserInfo();

                // add userportal record
                UserController.AddUserPortal(portalId, administratorId);

                // retrieve existing administrator
                try
                {
                    adminUser = UserController.GetUserById(portalId, administratorId);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                if (administratorId > 0)
                {
                    this.CreatePortalInternal(portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, ref message);
                }
            }
            else
            {
                message += Localization.GetString("CreatePortal.Error");
                throw new Exception(message);
            }

            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
                // should be no exception, but suppress just in case
            }

            // remove the portal id from http context as there is no exception.
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains("CreatingPortalId"))
            {
                HttpContext.Current.Items.Remove("CreatingPortalId");
            }

            return portalId;
        }

        /// <summary>Creates the portal.</summary>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="adminUser">The obj admin user.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="template"> </param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="childPath">The child path.</param>
        /// <param name="isChildPortal">if set to <c>true</c> means the portal is child portal.</param>
        /// <returns>Portal id.</returns>
        public int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, IPortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            // Attempt to create a new portal
            int portalId = CreatePortal(portalName, homeDirectory, template.CultureCode);

            // Log the portal if into http context, if exception occurred in next step, we can remove the portal which is not really created.
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items.Add("CreatingPortalId", portalId);
            }

            string message = Null.NullString;

            if (portalId != -1)
            {
                // add administrator
                int administratorId = Null.NullInteger;
                adminUser.PortalID = portalId;
                try
                {
                    UserCreateStatus createStatus = UserController.CreateUser(ref adminUser);
                    if (createStatus == UserCreateStatus.Success)
                    {
                        administratorId = adminUser.UserID;

                        // reload the UserInfo as when it was first created, it had no portal id and therefore
                        // used host profile definitions
                        adminUser = UserController.GetUserById(adminUser.PortalID, adminUser.UserID);
                    }
                    else
                    {
                        message += UserController.GetUserCreateStatus(createStatus);
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                    message += Localization.GetString("CreateAdminUser.Error") + exc.Message + exc.StackTrace;
                }

                if (administratorId > 0)
                {
                    this.CreatePortalInternal(portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, ref message);
                }
            }
            else
            {
                message += Localization.GetString("CreatePortal.Error");
                throw new Exception(message);
            }

            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
                // should be no exception, but suppress just in case
            }

            // remove the portal id from http context as there is no exception.
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains("CreatingPortalId"))
            {
                HttpContext.Current.Items.Remove("CreatingPortalId");
            }

            return portalId;
        }

        /// <summary>Get all the available portal templates grouped by culture.</summary>
        /// <returns>List of PortalTemplateInfo objects.</returns>
        [Obsolete("Deprecated in DNN 9.11.1. Use DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.GetPortalTemplates instead. Scheduled removal in v11.0.0.")]
        public IList<PortalTemplateInfo> GetAvailablePortalTemplates()
        {
            var list = new List<PortalTemplateInfo>();

            var templateFilePaths = PortalTemplateIO.Instance.EnumerateTemplates();
            var languageFileNames = PortalTemplateIO.Instance.EnumerateLanguageFiles().Select(Path.GetFileName).ToList();

            foreach (string templateFilePath in templateFilePaths)
            {
                var currentFileName = Path.GetFileName(templateFilePath);
                var langs = languageFileNames
                                .Where(x => this.GetTemplateName(x).Equals(currentFileName, StringComparison.InvariantCultureIgnoreCase))
                                .Select(x => this.GetCultureCode(x))
                                .Distinct()
                                .ToList();

                if (langs.Any())
                {
                    langs.ForEach(x => list.Add(new PortalTemplateInfo(templateFilePath, x)));
                }
                else
                {
                    list.Add(new PortalTemplateInfo(templateFilePath, string.Empty));
                }
            }

            return list;
        }

        /// <summary>  Gets information of a portal.</summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <returns>PortalInfo object with portal definition.</returns>
        public PortalInfo GetPortal(int portalId)
        {
            if (portalId == -1)
            {
                return null;
            }

            string defaultLanguage = GetActivePortalLanguage(portalId);
            PortalInfo portal = this.GetPortal(portalId, defaultLanguage);
            if (portal == null)
            {
                // Active language may not be valid, so fallback to default language
                defaultLanguage = GetPortalDefaultLanguage(portalId);
                portal = this.GetPortal(portalId, defaultLanguage);
            }

            return portal;
        }

        /// <summary>Gets information of a portal.</summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>The <see cref="PortalInfo"/> instance or <see langword="null"/>.</returns>
        public PortalInfo GetPortal(int portalId, string cultureCode)
        {
            if (portalId == -1)
            {
                return null;
            }

            PortalInfo portal = GetPortalInternal(portalId, cultureCode);

            if (Localization.ActiveLanguagesByPortalID(portalId) > 1)
            {
                if (portal == null)
                {
                    // Get Fallback language
                    string fallbackLanguage = string.Empty;

                    if (string.IsNullOrEmpty(cultureCode))
                    {
                        cultureCode = GetPortalDefaultLanguage(portalId);
                    }

                    Locale userLocale = LocaleController.Instance.GetLocale(cultureCode);
                    if (userLocale != null && !string.IsNullOrEmpty(userLocale.Fallback))
                    {
                        fallbackLanguage = userLocale.Fallback;
                    }

                    if (string.IsNullOrEmpty(fallbackLanguage))
                    {
                        fallbackLanguage = Localization.SystemLocale;
                    }

                    portal = GetPortalInternal(portalId, fallbackLanguage);

                    // if we cannot find any fallback, it mean's it's a non portal default langauge
                    if (portal == null)
                    {
                        DataProvider.Instance().EnsureLocalizationExists(portalId, GetActivePortalLanguage(portalId));
                        DataCache.ClearHostCache(true);
                        portal = GetPortalInternal(portalId, GetActivePortalLanguage(portalId));
                    }
                }
            }

            return portal;
        }

        /// <summary>Gets the portal.</summary>
        /// <param name="uniqueId">The unique id.</param>
        /// <returns>Portal info.</returns>
        public PortalInfo GetPortal(Guid uniqueId)
        {
            return this.GetPortalList(Null.NullString).SingleOrDefault(p => p.GUID == uniqueId);
        }

        /// <summary>Gets information from all portals.</summary>
        /// <returns>ArrayList of PortalInfo objects.</returns>
        public ArrayList GetPortals()
        {
            return new ArrayList(this.GetPortalList(Null.NullString));
        }

        /// <summary>Get portals in specific culture.</summary>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="PortalInfo"/> instances.</returns>
        public List<PortalInfo> GetPortalList(string cultureCode)
        {
            string cacheKey = string.Format(DataCache.PortalCacheKey, Null.NullInteger, cultureCode);
            return CBO.GetCachedObject<List<PortalInfo>>(
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, cultureCode),
                c => CBO.FillCollection<PortalInfo>(DataProvider.Instance().GetPortals(cultureCode)));
        }

        /// <summary>Gets the portal settings dictionary.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>portal settings.</returns>
        public Dictionary<string, string> GetPortalSettings(int portalId)
        {
            return GetPortalSettingsDictionary(portalId, string.Empty);
        }

        /// <summary>Gets the portal settings dictionary.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>portal settings.</returns>
        public Dictionary<string, string> GetPortalSettings(int portalId, string cultureCode)
        {
            return GetPortalSettingsDictionary(portalId, cultureCode);
        }

        /// <summary>Load info for a portal template.</summary>
        /// <param name="templatePath">Full path to the portal template.</param>
        /// <param name="cultureCode">the culture code if any for the localization of the portal template.</param>
        /// <returns>A portal template.</returns>
        [Obsolete("Deprecated in DNN 9.11.1. Use DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.GetPortalTemplate instead. Scheduled removal in v11.0.0.")]
        public PortalTemplateInfo GetPortalTemplate(string templatePath, string cultureCode)
        {
            var template = new PortalTemplateInfo(templatePath, cultureCode);

            if (!string.IsNullOrEmpty(cultureCode) && template.CultureCode != cultureCode)
            {
                return null;
            }

            return template;
        }

        /// <summary>Gets the portal space used bytes.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>Space used in bytes.</returns>
        public long GetPortalSpaceUsedBytes(int portalId)
        {
            long size = 0;
            IDataReader dr = DataProvider.Instance().GetPortalSpaceUsed(portalId);
            try
            {
                if (dr.Read())
                {
                    if (dr["SpaceUsed"] != DBNull.Value)
                    {
                        size = Convert.ToInt64(dr["SpaceUsed"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return size;
        }

        /// <summary>Verifies if there's enough space to upload a new file on the given portal.</summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="fileSizeBytes">Size of the file being uploaded.</param>
        /// <returns>True if there's enough space available to upload the file.</returns>
        public bool HasSpaceAvailable(int portalId, long fileSizeBytes)
        {
            int hostSpace;
            if (portalId == -1)
            {
                hostSpace = 0;
            }
            else
            {
                PortalSettings ps = GetCurrentPortalSettingsInternal();
                if (ps != null && ps.PortalId == portalId)
                {
                    hostSpace = ps.HostSpace;
                }
                else
                {
                    PortalInfo portal = this.GetPortal(portalId);
                    hostSpace = portal.HostSpace;
                }
            }

            return (((this.GetPortalSpaceUsedBytes(portalId) + fileSizeBytes) / Math.Pow(1024, 2)) <= hostSpace) || hostSpace == 0;
        }

        /// <summary>
        ///   Remaps the Special Pages such as Home, Profile, Search
        ///   to their localized versions.
        /// </summary>
        public void MapLocalizedSpecialPages(int portalId, string cultureCode)
        {
            DataCache.ClearHostCache(true);
            DataProvider.Instance().EnsureLocalizationExists(portalId, cultureCode);

            PortalInfo defaultPortal = this.GetPortal(portalId, GetPortalDefaultLanguage(portalId));
            PortalInfo targetPortal = this.GetPortal(portalId, cultureCode);

            Locale targetLocale = LocaleController.Instance.GetLocale(cultureCode);
            TabInfo tempTab;
            if (defaultPortal.HomeTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.HomeTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.HomeTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.LoginTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.LoginTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.LoginTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.RegisterTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.RegisterTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.RegisterTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.SplashTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.SplashTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.SplashTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.UserTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.UserTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.UserTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.SearchTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.SearchTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.SearchTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.Custom404TabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.Custom404TabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.Custom404TabId = tempTab.TabID;
                }
            }

            if (defaultPortal.Custom500TabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.Custom500TabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.Custom500TabId = tempTab.TabID;
                }
            }

            if (defaultPortal.TermsTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.TermsTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.TermsTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.PrivacyTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.PrivacyTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.PrivacyTabId = tempTab.TabID;
                }
            }

            this.UpdatePortalInternal(targetPortal, false);
        }

        /// <summary>Removes the related PortalLocalization record from the database, adds optional clear cache.</summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        public void RemovePortalLocalization(int portalId, string cultureCode, bool clearCache = true)
        {
            DataProvider.Instance().RemovePortalLocalization(portalId, cultureCode);
            if (clearCache)
            {
                DataCache.ClearPortalCache(portalId, false);
            }
        }

        /// <summary>Processess a template file for the new portal.</summary>
        /// <param name="portalId">PortalId of the new portal.</param>
        /// <param name="template">The template.</param>
        /// <param name="administratorId">UserId for the portal administrator. This is used to assign roles to this user.</param>
        /// <param name="mergeTabs">Flag to determine whether Module content is merged.</param>
        /// <param name="isNewPortal">Flag to determine is the template is applied to an existing portal or a new one.</param>
        /// <remarks>
        /// The roles and settings nodes will only be processed on the portal template file.
        /// </remarks>
        [Obsolete("Deprecated in DNN 9.11.1. Use DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.ApplyPortalTemplate instead. Scheduled removal in v11.0.0.")]
        public void ParseTemplate(int portalId, PortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            var t = new Templates.PortalTemplateInfo(template.TemplateFilePath, template.CultureCode);
            var portalTemplateImporter = new PortalTemplateImporter(t);
            portalTemplateImporter.ParseTemplate(portalId, administratorId, mergeTabs.ToNewEnum(), isNewPortal);
        }

        /// <summary>Processes the resource file for the template file selected.</summary>
        /// <param name="portalPath">New portal's folder.</param>
        /// <param name="resoureceFile">full path to the resource file.</param>
        /// <remarks>
        /// The resource file is a zip file with the same name as the selected template file and with
        /// an extension of .resources (to disable this file being downloaded).
        /// For example: for template file "portal.template" a resource file "portal.template.resources" can be defined.
        /// </remarks>
        public void ProcessResourceFileExplicit(string portalPath, string resoureceFile)
        {
            try
            {
                FileSystemUtils.UnzipResources(new ZipArchive(new FileStream(resoureceFile, FileMode.Open, FileAccess.Read)), portalPath);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

        /// <summary>Updates the portal expiry.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="cultureCode">The culture code.</param>
        public void UpdatePortalExpiry(int portalId, string cultureCode)
        {
            var portal = this.GetPortal(portalId, cultureCode);

            if (portal.ExpiryDate == Null.NullDate)
            {
                portal.ExpiryDate = DateTime.Now;
            }

            portal.ExpiryDate = portal.ExpiryDate.AddMonths(1);

            this.UpdatePortalInfo(portal);
        }

        /// <summary>Updates basic portal information.</summary>
        /// <param name="portal"></param>
        public void UpdatePortalInfo(PortalInfo portal)
        {
            this.UpdatePortalInternal(portal, true);
        }

        /// <summary>Gets the current portal settings.</summary>
        /// <returns>portal settings.</returns>
        PortalSettings IPortalController.GetCurrentPortalSettings()
        {
            return GetCurrentPortalSettingsInternal();
        }

        /// <inheritdoc/>
        IAbPortalSettings IPortalController.GetCurrentSettings()
        {
            return GetCurrentPortalSettingsInternal();
        }

        /// <inheritdoc/>
        [Obsolete("Deprecated in DNN 9.2.0. Use the overloaded one with the 'isSecure' parameter instead. Scheduled removal in v11.0.0.")]
        void IPortalController.UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode)
        {
            UpdatePortalSettingInternal(portalID, settingName, settingValue, clearCache, cultureCode, false);
        }

        /// <summary>Adds or Updates or Deletes a portal setting value.</summary>
        void IPortalController.UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode, bool isSecure)
        {
            UpdatePortalSettingInternal(portalID, settingName, settingValue, clearCache, cultureCode, isSecure);
        }

        [Obsolete("Deprecated in DNN 9.11.1. Use DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo instead. Scheduled removal in v11.0.0.")]
        public int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, PortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            return this.CreatePortal(portalName, adminUser, description, keyWords, template.ToNewPortalTemplateInfo(), homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
        }

        [Obsolete("Deprecated in DNN 9.11.1. Use DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo instead. Scheduled removal in v11.0.0.")]
        public int CreatePortal(string portalName, int adminUserId, string description, string keyWords, PortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            return this.CreatePortal(portalName, adminUserId, description, keyWords, template.ToNewPortalTemplateInfo(), homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
        }

        internal static void EnsureRequiredEventLogTypesExist()
        {
            if (!DoesLogTypeExists(EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString()))
            {
                // Add 404 Log
                var logTypeInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString(),
                    LogTypeFriendlyName = "HTTP Error Code 404 Page Not Found",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeInfo);

                // Add LogType
                var logTypeConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = true,
                    LogTypeKey = EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString(),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeConf);
            }

            if (!DoesLogTypeExists(EventLogController.EventLogType.IP_LOGIN_BANNED.ToString()))
            {
                // Add IP filter log type
                var logTypeFilterInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.IP_LOGIN_BANNED.ToString(),
                    LogTypeFriendlyName = "HTTP Error Code Forbidden IP address rejected",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeFilterInfo);

                // Add LogType
                var logTypeFilterConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = true,
                    LogTypeKey = EventLogController.EventLogType.IP_LOGIN_BANNED.ToString(),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeFilterConf);
            }

            if (!DoesLogTypeExists(EventLogController.EventLogType.TABURL_CREATED.ToString()))
            {
                var logTypeInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.TABURL_CREATED.ToString(),
                    LogTypeFriendlyName = "TabURL created",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationSuccess",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeInfo);

                logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_UPDATED.ToString();
                logTypeInfo.LogTypeFriendlyName = "TabURL updated";
                LogController.Instance.AddLogType(logTypeInfo);

                logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_DELETED.ToString();
                logTypeInfo.LogTypeFriendlyName = "TabURL deleted";
                LogController.Instance.AddLogType(logTypeInfo);

                // Add LogType
                var logTypeUrlConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = false,
                    LogTypeKey = EventLogController.EventLogType.TABURL_CREATED.ToString(),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);

                logTypeUrlConf.LogTypeKey = EventLogController.EventLogType.TABURL_UPDATED.ToString();
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);

                logTypeUrlConf.LogTypeKey = EventLogController.EventLogType.TABURL_DELETED.ToString();
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);
            }

            if (!DoesLogTypeExists(EventLogController.EventLogType.SCRIPT_COLLISION.ToString()))
            {
                // Add IP filter log type
                var logTypeFilterInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.SCRIPT_COLLISION.ToString(),
                    LogTypeFriendlyName = "Javscript library registration resolved script collision",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeFilterInfo);

                // Add LogType
                var logTypeFilterConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = true,
                    LogTypeKey = EventLogController.EventLogType.SCRIPT_COLLISION.ToString(),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeFilterConf);
            }
        }

        /// <inheritdoc/>
        protected override Func<IPortalController> GetFactory()
        {
            return () => new PortalController();
        }

        private static int CreatePortal(string portalName, string homeDirectory, string cultureCode)
        {
            // add portal
            int portalId = -1;
            try
            {
                // Use host settings as default values for these parameters
                // This can be overwritten on the portal template
                var datExpiryDate = Host.Host.DemoPeriod > Null.NullInteger
                    ? Convert.ToDateTime(Globals.GetMediumDate(DateTime.Now.AddDays(Host.Host.DemoPeriod).ToString(CultureInfo.InvariantCulture)))
                    : Null.NullDate;

                portalId = DataProvider.Instance().CreatePortal(
                    portalName,
                    Host.Host.HostCurrency,
                    datExpiryDate,
                    Host.Host.HostFee,
                    Host.Host.HostSpace,
                    Host.Host.PageQuota,
                    Host.Host.UserQuota,
                    0, // site log history function has been removed.
                    homeDirectory,
                    cultureCode,
                    UserController.Instance.GetCurrentUserInfo().UserID);

                // clear portal cache
                DataCache.ClearHostCache(true);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
                // should be no exception, but suppress just in case
            }

            return portalId;
        }

        private static void DeletePortalInternal(int portalId)
        {
            UserController.DeleteUsers(portalId, false, true);

            var portal = Instance.GetPortal(portalId);

            DataProvider.Instance().DeletePortalInfo(portalId);

            try
            {
                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = EventLogController.EventLogType.PORTAL_DELETED.ToString(),
                };
                log.LogProperties.Add(new LogDetailInfo("Delete Portal:", portal.PortalName));
                log.LogProperties.Add(new LogDetailInfo("PortalID:", portal.PortalID.ToString()));
                LogController.Instance.AddLog(log);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            DataCache.ClearHostCache(true);

            // queue remove portal from search index
            var document = new SearchDocumentToDelete
            {
                PortalId = portalId,
            };

            DataProvider.Instance().AddSearchDeletedItems(document);
        }

        private static bool DoesLogTypeExists(string logTypeKey)
        {
            LogTypeInfo logType;
            Dictionary<string, LogTypeInfo> logTypeDictionary = LogController.Instance.GetLogTypeInfoDictionary();
            logTypeDictionary.TryGetValue(logTypeKey, out logType);
            if (logType == null)
            {
                return false;
            }

            return true;
        }

        private static PortalSettings GetCurrentPortalSettingsInternal()
        {
            PortalSettings objPortalSettings = null;
            if (HttpContext.Current != null)
            {
                objPortalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

            return objPortalSettings;
        }

        private static PortalInfo GetPortalInternal(int portalId, string cultureCode)
        {
            return Instance.GetPortalList(cultureCode).SingleOrDefault(p => p.PortalID == portalId);
        }

        private static object GetPortalDefaultLanguageCallBack(CacheItemArgs cacheItemArgs)
        {
            int portalID = (int)cacheItemArgs.ParamList[0];
            return DataProvider.Instance().GetPortalDefaultLanguage(portalID);
        }

        private static object GetPortalDictionaryCallback(CacheItemArgs cacheItemArgs)
        {
            var portalDic = new Dictionary<int, int>();
            if (Host.Host.PerformanceSetting != Globals.PerformanceSettings.NoCaching)
            {
                // get all tabs
                int intField = 0;
                IDataReader dr = DataProvider.Instance().GetTabPaths(Null.NullInteger, Null.NullString);
                try
                {
                    while (dr.Read())
                    {
                        // add to dictionary
                        portalDic[Convert.ToInt32(Null.SetNull(dr["TabID"], intField))] = Convert.ToInt32(Null.SetNull(dr["PortalID"], intField));
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.LogException(exc);
                }
                finally
                {
                    // close datareader
                    CBO.CloseDataReader(dr, true);
                }
            }

            return portalDic;
        }

        private static object GetPortalSettingsDictionaryCallback(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var dicSettings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (portalId <= -1)
            {
                return dicSettings;
            }

            var cultureCode = Convert.ToString(cacheItemArgs.ParamList[1]);
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = GetActivePortalLanguage(portalId);
            }

            var dr = DataProvider.Instance().GetPortalSettings(portalId, cultureCode);
            try
            {
                while (dr.Read())
                {
                    if (dr.IsDBNull(1))
                    {
                        continue;
                    }

                    var key = dr.GetString(0);
                    if (dicSettings.ContainsKey(key))
                    {
                        dicSettings[key] = dr.GetString(1);
                        var log = new LogInfo
                        {
                            LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString(),
                        };
                        log.AddProperty("Duplicate PortalSettings Key", key);
                        LogController.Instance.AddLog(log);
                    }
                    else
                    {
                        dicSettings.Add(key, dr.GetString(1));
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return dicSettings;
        }

        private static void EnsureRequiredProvidersForFolderTypes()
        {
            if (ComponentFactory.GetComponent<CryptographyProvider>() == null)
            {
                ComponentFactory.InstallComponents(new ProviderInstaller("cryptography", typeof(CryptographyProvider), typeof(FipsCompilanceCryptographyProvider)));
                ComponentFactory.RegisterComponentInstance<CryptographyProvider>(new FipsCompilanceCryptographyProvider());
            }
        }

        private static void EnsureFolderProviderRegistration<TAbstract>(FolderTypeConfig folderTypeConfig, XmlDocument webConfig)
            where TAbstract : class
        {
            var providerBusinessClassNode = webConfig.SelectSingleNode("configuration/dotnetnuke/folder/providers/add[@name='" + folderTypeConfig.Provider + "']");

            var typeClass = Type.GetType(providerBusinessClassNode.Attributes["type"].Value);
            if (typeClass != null)
            {
                ComponentFactory.RegisterComponentInstance<TAbstract>(folderTypeConfig.Provider, Activator.CreateInstance(typeClass));
            }
        }

        private static bool EnableBrowserLanguageInDefault(int portalId)
        {
            bool retValue = Null.NullBoolean;
            try
            {
                string setting;
                GetPortalSettingsDictionary(portalId, Localization.SystemLocale).TryGetValue("EnableBrowserLanguage", out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = Host.Host.EnableBrowserLanguage;
                }
                else
                {
                    retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        private static Dictionary<string, string> GetPortalSettingsDictionary(int portalId, string cultureCode)
        {
            var httpContext = HttpContext.Current;

            if (string.IsNullOrEmpty(cultureCode) && portalId > -1)
            {
                cultureCode = GetActivePortalLanguageFromHttpContext(httpContext, portalId);
            }

            // Get PortalSettings from Context or from cache
            var dictionaryKey = string.Format(HttpContextKeyPortalSettingsDictionary, portalId, cultureCode);
            Dictionary<string, string> dictionary = null;
            if (httpContext != null)
            {
                dictionary = httpContext.Items[dictionaryKey] as Dictionary<string, string>;
            }

            if (dictionary == null)
            {
                var cacheKey = string.Format(DataCache.PortalSettingsCacheKey, portalId, cultureCode);
                dictionary = CBO.GetCachedObject<Dictionary<string, string>>(
                    new CacheItemArgs(
                        cacheKey,
                        DataCache.PortalSettingsCacheTimeOut,
                        DataCache.PortalSettingsCachePriority,
                        portalId,
                        cultureCode),
                    GetPortalSettingsDictionaryCallback,
                    true);
                if (httpContext != null)
                {
                    httpContext.Items[dictionaryKey] = dictionary;
                }
            }

            return dictionary;
        }

        private static string GetActivePortalLanguageFromHttpContext(HttpContext httpContext, int portalId)
        {
            var cultureCode = string.Empty;

            // Lookup culturecode but cache it in the HttpContext for performance
            var activeLanguageKey = string.Format("ActivePortalLanguage{0}", portalId);
            if (httpContext != null)
            {
                cultureCode = (string)httpContext.Items[activeLanguageKey];
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = GetActivePortalLanguage(portalId);
                if (httpContext != null)
                {
                    httpContext.Items[activeLanguageKey] = cultureCode;
                }
            }

            return cultureCode;
        }

        private static void UpdatePortalSettingInternal(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode, bool isSecure)
        {
            string currentSetting = GetPortalSetting(settingName, portalID, string.Empty, cultureCode);

            if (isSecure && !string.IsNullOrEmpty(settingName) && !string.IsNullOrEmpty(settingValue))
            {
                settingValue = Security.FIPSCompliant.EncryptAES(settingValue, Config.GetDecryptionkey(), Host.Host.GUID);
            }

            if (currentSetting != settingValue)
            {
                DataProvider.Instance().UpdatePortalSetting(portalID, settingName, settingValue, UserController.Instance.GetCurrentUserInfo().UserID, cultureCode, isSecure);
                EventLogController.Instance.AddLog(settingName + ((cultureCode == Null.NullString) ? string.Empty : " (" + cultureCode + ")"), settingValue, GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
                if (clearCache)
                {
                    DataCache.ClearPortalCache(portalID, false);
                    DataCache.RemoveCache(DataCache.PortalDictionaryCacheKey);

                    var httpContext = HttpContext.Current;
                    if (httpContext != null)
                    {
                        var cultureCodeForKey = GetActivePortalLanguageFromHttpContext(httpContext, portalID);
                        var dictionaryKey = string.Format(HttpContextKeyPortalSettingsDictionary, portalID, cultureCodeForKey);
                        httpContext.Items[dictionaryKey] = null;
                    }
                }

                EventManager.Instance.OnPortalSettingUpdated(new PortalSettingUpdatedEventArgs
                {
                    PortalId = portalID,
                    SettingName = settingName,
                    SettingValue = settingValue,
                });
            }
        }

        private void CreatePortalInternal(int portalId, string portalName, UserInfo adminUser, string description, string keyWords, IPortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal, ref string message)
        {
            // Add default workflows
            try
            {
                SystemWorkflowManager.Instance.CreateSystemWorkflows(portalId);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            var portalTemplateImporter = new PortalTemplateImporter(template);

            if (string.IsNullOrEmpty(homeDirectory))
            {
                homeDirectory = "Portals/" + portalId;
            }

            string mappedHomeDirectory = string.Format(Globals.ApplicationMapPath + "\\" + homeDirectory + "\\").Replace("/", "\\");

            if (Directory.Exists(mappedHomeDirectory))
            {
                message += string.Format(Localization.GetString("CreatePortalHomeFolderExists.Error"), homeDirectory);
                throw new Exception(message);
            }

            message += portalTemplateImporter.CreateProfileDefinitions(portalId);

            if (string.IsNullOrEmpty(message))
            {
                try
                {
                    // the upload directory may already exist if this is a new DB working with a previously installed application
                    if (Directory.Exists(mappedHomeDirectory))
                    {
                        Globals.DeleteFolderRecursive(mappedHomeDirectory);
                    }
                }
                catch (Exception exc1)
                {
                    Logger.Error(exc1);
                    message += Localization.GetString("DeleteUploadFolder.Error") + exc1.Message + exc1.StackTrace;
                }

                // Set up Child Portal
                if (message == Null.NullString)
                {
                    if (isChildPortal)
                    {
                        message = CreateChildPortalFolder(childPath);
                    }
                }
                else
                {
                    throw new Exception(message);
                }

                if (message == Null.NullString)
                {
                    try
                    {
                        // create the upload directory for the new portal
                        Directory.CreateDirectory(mappedHomeDirectory);

                        // ensure that the Templates folder exists
                        string templateFolder = string.Format("{0}Templates", mappedHomeDirectory);
                        if (!Directory.Exists(templateFolder))
                        {
                            Directory.CreateDirectory(templateFolder);
                        }

                        // ensure that the Users folder exists
                        string usersFolder = string.Format("{0}Users", mappedHomeDirectory);
                        if (!Directory.Exists(usersFolder))
                        {
                            Directory.CreateDirectory(usersFolder);
                        }

                        // copy the default page template
                        this.CopyPageTemplate("Default.page.template", mappedHomeDirectory);

                        // process zip resource file if present
                        if (File.Exists(template.ResourceFilePath))
                        {
                            this.ProcessResourceFileExplicit(mappedHomeDirectory, template.ResourceFilePath);
                        }
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                        message += Localization.GetString("ChildPortal.Error") + exc1.Message + exc1.StackTrace;
                    }
                }
                else
                {
                    throw new Exception(message);
                }

                if (message == Null.NullString)
                {
                    try
                    {
                        FolderMappingController.Instance.AddDefaultFolderTypes(portalId);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                        message += Localization.GetString("DefaultFolderMappings.Error") + exc1.Message + exc1.StackTrace;
                    }
                }
                else
                {
                    throw new Exception(message);
                }

                LocaleCollection newPortalLocales = null;
                if (message == Null.NullString)
                {
                    try
                    {
                        this.CreatePredefinedFolderTypes(portalId);
                        portalTemplateImporter.ParseTemplateInternal(portalId, adminUser.UserID, PortalTemplateModuleAction.Replace.ToNewEnum(), true, out newPortalLocales);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                        message += Localization.GetString("PortalTemplate.Error") + exc1.Message + exc1.StackTrace;
                    }
                }
                else
                {
                    throw new Exception(message);
                }

                if (message == Null.NullString)
                {
                    var portal = this.GetPortal(portalId);
                    portal.Description = description;
                    portal.KeyWords = keyWords;
                    portal.UserTabId = TabController.GetTabByTabPath(portal.PortalID, "//UserProfile", portal.CultureCode);
                    if (portal.UserTabId == -1)
                    {
                        portal.UserTabId = TabController.GetTabByTabPath(portal.PortalID, "//ActivityFeed", portal.CultureCode);
                    }

                    portal.SearchTabId = TabController.GetTabByTabPath(portal.PortalID, "//SearchResults", portal.CultureCode);
                    this.UpdatePortalInfo(portal);

                    adminUser.Profile.PreferredLocale = portal.DefaultLanguage;
                    var portalSettings = new PortalSettings(portal);
                    adminUser.Profile.PreferredTimeZone = portalSettings.TimeZone;
                    UserController.UpdateUser(portal.PortalID, adminUser);

                    DesktopModuleController.AddDesktopModulesToPortal(portalId);

                    this.AddPortalAlias(portalId, portalAlias);

                    UpdatePortalSetting(portalId, "DefaultPortalAlias", portalAlias, false);

                    DataCache.ClearPortalCache(portalId, true);

                    if (newPortalLocales != null)
                    {
                        foreach (Locale newPortalLocale in newPortalLocales.AllValues)
                        {
                            Localization.AddLanguageToPortal(portalId, newPortalLocale.LanguageId, false);
                            if (portalSettings.ContentLocalizationEnabled)
                            {
                                this.MapLocalizedSpecialPages(portalId, newPortalLocale.Code);
                            }
                        }
                    }

                    try
                    {
                        RelationshipController.Instance.CreateDefaultRelationshipsForPortal(portalId);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                    }

                    // add profanity list to new portal
                    try
                    {
                        const string listName = "ProfanityFilter";
                        var listController = new ListController();
                        var entry = new ListEntryInfo
                        {
                            PortalID = portalId,
                            SystemList = false,
                            ListName = listName + "-" + portalId,
                        };
                        listController.AddListEntry(entry);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                    }

                    // add banned password list to new portal
                    try
                    {
                        const string listName = "BannedPasswords";
                        var listController = new ListController();
                        var entry = new ListEntryInfo
                        {
                            PortalID = portalId,
                            SystemList = false,
                            ListName = listName + "-" + portalId,
                        };
                        listController.AddListEntry(entry);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                    }

                    ServicesRoutingManager.ReRegisterServiceRoutesWhileSiteIsRunning();

                    try
                    {
                        var log = new LogInfo
                        {
                            BypassBuffering = true,
                            LogTypeKey = EventLogController.EventLogType.PORTAL_CREATED.ToString(),
                        };
                        log.LogProperties.Add(new LogDetailInfo("Install Portal:", portalName));
                        log.LogProperties.Add(new LogDetailInfo("FirstName:", adminUser.FirstName));
                        log.LogProperties.Add(new LogDetailInfo("LastName:", adminUser.LastName));
                        log.LogProperties.Add(new LogDetailInfo("Username:", adminUser.Username));
                        log.LogProperties.Add(new LogDetailInfo("Email:", adminUser.Email));
                        log.LogProperties.Add(new LogDetailInfo("Description:", description));
                        log.LogProperties.Add(new LogDetailInfo("Keywords:", keyWords));
                        log.LogProperties.Add(new LogDetailInfo("Template:", template.TemplateFilePath));
                        log.LogProperties.Add(new LogDetailInfo("TemplateCulture:", template.CultureCode));
                        log.LogProperties.Add(new LogDetailInfo("HomeDirectory:", homeDirectory));
                        log.LogProperties.Add(new LogDetailInfo("PortalAlias:", portalAlias));
                        log.LogProperties.Add(new LogDetailInfo("ServerPath:", serverPath));
                        log.LogProperties.Add(new LogDetailInfo("ChildPath:", childPath));
                        log.LogProperties.Add(new LogDetailInfo("IsChildPortal:", isChildPortal.ToString()));
                        LogController.Instance.AddLog(log);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                    }

                    EventManager.Instance.OnPortalCreated(new PortalCreatedEventArgs { PortalId = portalId });
                }
                else
                {
                    throw new Exception(message);
                }
            }
            else
            {
                DeletePortalInternal(portalId);
                throw new Exception(message);
            }
        }

        private void CreatePredefinedFolderTypes(int portalId)
        {
            try
            {
                EnsureRequiredProvidersForFolderTypes();
            }
            catch (Exception ex)
            {
                Logger.Error(Localization.GetString("CreatingConfiguredFolderMapping.Error"), ex);
            }

            var webConfig = Config.Load();
            foreach (FolderTypeConfig folderTypeConfig in FolderMappingsConfigController.Instance.FolderTypes)
            {
                try
                {
                    EnsureFolderProviderRegistration<FolderProvider>(folderTypeConfig, webConfig);
                    FolderMappingController.Instance.AddFolderMapping(this.GetFolderMappingFromConfig(
                        folderTypeConfig,
                        portalId));
                }
                catch (Exception ex)
                {
                    Logger.Error(Localization.GetString("CreatingConfiguredFolderMapping.Error") + ": " + folderTypeConfig.Name, ex);
                }
            }
        }

        private string EnsureSettingValue(string folderProviderType, FolderTypeSettingConfig settingNode, int portalId)
        {
            var ensuredSettingValue =
                settingNode.Value.Replace("{PortalId}", (portalId != -1) ? portalId.ToString(CultureInfo.InvariantCulture) : "_default").Replace("{HostId}", Host.Host.GUID);
            if (settingNode.Encrypt)
            {
                return FolderProvider.Instance(folderProviderType).EncryptValue(ensuredSettingValue);

                // return PortalSecurity.Instance.Encrypt(Host.Host.GUID, ensuredSettingValue.Trim());
            }

            return ensuredSettingValue;
        }

        private string GetCultureCode(string languageFileName)
        {
            // e.g. "default template.template.en-US.resx"
            return languageFileName.GetLocaleCodeFromFileName();
        }

        private FolderMappingInfo GetFolderMappingFromConfig(FolderTypeConfig node, int portalId)
        {
            var folderMapping = new FolderMappingInfo
            {
                PortalID = portalId,
                MappingName = node.Name,
                FolderProviderType = node.Provider,
            };

            foreach (FolderTypeSettingConfig settingNode in node.Settings)
            {
                var settingValue = this.EnsureSettingValue(folderMapping.FolderProviderType, settingNode, portalId);
                folderMapping.FolderMappingSettings.Add(settingNode.Name, settingValue);
            }

            return folderMapping;
        }

        private string GetTemplateName(string languageFileName)
        {
            // e.g. "default template.template.en-US.resx"
            return languageFileName.GetFileNameFromLocalizedResxFile();
        }

        private void UpdatePortalInternal(PortalInfo portal, bool clearCache)
        {
            var processorPassword = portal.ProcessorPassword;
            if (!string.IsNullOrEmpty(processorPassword))
            {
                processorPassword = Security.FIPSCompliant.EncryptAES(processorPassword, Config.GetDecryptionkey(), Host.Host.GUID);
            }

            DataProvider.Instance().UpdatePortalInfo(
                portal.PortalID,
                portal.PortalGroupID,
                portal.PortalName,
                portal.LogoFile,
                portal.FooterText,
                portal.ExpiryDate,
                portal.UserRegistration,
                portal.BannerAdvertising,
                portal.Currency,
                portal.AdministratorId,
                portal.HostFee,
                portal.HostSpace,
                portal.PageQuota,
                portal.UserQuota,
                portal.PaymentProcessor,
                portal.ProcessorUserId,
                processorPassword,
                portal.Description,
                portal.KeyWords,
                portal.BackgroundFile,
                0, // site log history function has been removed.
                portal.SplashTabId,
                portal.HomeTabId,
                portal.LoginTabId,
                portal.RegisterTabId,
                portal.UserTabId,
                portal.SearchTabId,
                portal.Custom404TabId,
                portal.Custom500TabId,
                portal.TermsTabId,
                portal.PrivacyTabId,
                portal.DefaultLanguage,
                portal.HomeDirectory,
                UserController.Instance.GetCurrentUserInfo().UserID,
                portal.CultureCode);

            EventLogController.Instance.AddLog("PortalId", portal.PortalID.ToString(), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_UPDATED);

            // ensure a localization item exists (in case a new default language has been set)
            DataProvider.Instance().EnsureLocalizationExists(portal.PortalID, portal.DefaultLanguage);

            // clear portal cache
            if (clearCache)
            {
                DataCache.ClearHostCache(true);
            }
        }

        [Obsolete("Deprecated in DNN 9.11.1. Use DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo instead. Scheduled removal in v11.0.0.")]
        public class PortalTemplateInfo
        {
            private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalController));
            private string resourceFilePath;
            private string originalCultureCode;

            /// <summary>Initializes a new instance of the <see cref="PortalTemplateInfo"/> class.</summary>
            /// <param name="templateFilePath"></param>
            /// <param name="cultureCode"></param>
            public PortalTemplateInfo(string templateFilePath, string cultureCode)
            {
                this.TemplateFilePath = templateFilePath;
                this.originalCultureCode = cultureCode;

                this.InitLocalizationFields(cultureCode);
                this.InitNameAndDescription();
            }

            public string ResourceFilePath
            {
                get
                {
                    if (this.resourceFilePath == null)
                    {
                        this.resourceFilePath = PortalTemplateIO.Instance.GetResourceFilePath(this.TemplateFilePath);
                    }

                    return this.resourceFilePath;
                }
            }

            public string Name { get; private set; }

            public string CultureCode { get; private set; }

            public string TemplateFilePath { get; private set; }

            public string LanguageFilePath { get; private set; }

            public string Description { get; private set; }

            internal DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo ToNewPortalTemplateInfo()
            {
                return new DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo(this.TemplateFilePath, this.originalCultureCode);
            }

            private static string ReadLanguageFileValue(XDocument xmlDoc, string name)
            {
                return (from f in xmlDoc.Descendants("data")
                        where (string)f.Attribute("name") == name
                        select (string)f.Element("value")).SingleOrDefault();
            }

            private void InitNameAndDescription()
            {
                if (!string.IsNullOrEmpty(this.LanguageFilePath))
                {
                    this.LoadNameAndDescriptionFromLanguageFile();
                }

                if (string.IsNullOrEmpty(this.Name))
                {
                    this.Name = Path.GetFileNameWithoutExtension(this.TemplateFilePath);
                }

                if (string.IsNullOrEmpty(this.Description))
                {
                    this.LoadDescriptionFromTemplateFile();
                }
            }

            private void LoadDescriptionFromTemplateFile()
            {
                try
                {
                    XDocument xmlDoc;
                    using (var reader = PortalTemplateIO.Instance.OpenTextReader(this.TemplateFilePath))
                    {
                        xmlDoc = XDocument.Load(reader);
                    }

                    this.Description = xmlDoc.Elements("portal").Elements("description").SingleOrDefault().Value;
                }
                catch (Exception e)
                {
                    Logger.Error("Error while parsing: " + this.TemplateFilePath, e);
                }
            }

            private void LoadNameAndDescriptionFromLanguageFile()
            {
                try
                {
                    using (var reader = PortalTemplateIO.Instance.OpenTextReader(this.LanguageFilePath))
                    {
                        var xmlDoc = XDocument.Load(reader);

                        this.Name = ReadLanguageFileValue(xmlDoc, "LocalizedTemplateName.Text");
                        this.Description = ReadLanguageFileValue(xmlDoc, "PortalDescription.Text");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error while parsing: " + this.TemplateFilePath, e);
                }
            }

            private void InitLocalizationFields(string cultureCode)
            {
                this.LanguageFilePath = PortalTemplateIO.Instance.GetLanguageFilePath(this.TemplateFilePath, cultureCode);
                if (string.IsNullOrEmpty(this.LanguageFilePath))
                {
                    var locales = new List<string>();
                    (cultureCode, locales) = PortalTemplateIO.Instance.GetTemplateLanguages(this.TemplateFilePath);
                    if (string.IsNullOrEmpty(cultureCode))
                    {
                        var portalSettings = PortalSettings.Current;
                        cultureCode = portalSettings != null ? PortalController.GetPortalDefaultLanguage(portalSettings.PortalId) : Localization.SystemLocale;
                    }
                }

                this.CultureCode = cultureCode;
            }
        }
    }
}
