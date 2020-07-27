// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.XPath;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Users;

    /// <summary>
    /// IPortalController provides business layer APIs for the portal.
    /// </summary>
    /// <remarks>
    /// DotNetNuke supports the concept of virtualised sites in a single install. This means that multiple sites,
    /// each potentially with multiple unique URL's, can exist in one instance of DotNetNuke i.e. one set of files and one database.
    /// </remarks>
    public interface IPortalController
    {
        /// <summary>
        /// Creates a new portal alias.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="portalAlias">Portal Alias to be created.</param>
        void AddPortalAlias(int portalId, string portalAlias);

        /// <summary>
        /// Copies the page template.
        /// </summary>
        /// <param name="templateFile">The template file.</param>
        /// <param name="mappedHomeDirectory">The mapped home directory.</param>
        void CopyPageTemplate(string templateFile, string mappedHomeDirectory);

        /// <summary>
        /// Creates the portal.
        /// </summary>
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
        int CreatePortal(string portalName, int adminUserId, string description, string keyWords, PortalController.PortalTemplateInfo template,
                                            string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal);

        /// <summary>
        /// Creates the portal.
        /// </summary>
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
        int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, PortalController.PortalTemplateInfo template,
                         string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal);

        /// <summary>
        /// Get all the available portal templates grouped by culture.
        /// </summary>
        /// <returns>List of PortalTemplateInfo objects.</returns>
        IList<PortalController.PortalTemplateInfo> GetAvailablePortalTemplates();

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
        /// <returns>portal settings.</returns>
        [Obsolete("Deprecated in Platform 9.4.2. Scheduled removal in v11.0.0. Use GetCurrentSettings instead.")]
        PortalSettings GetCurrentPortalSettings();

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
        /// <returns>portal settings.</returns>
        IPortalSettings GetCurrentSettings();

        /// <summary>
        ///   Gets information of a portal.
        /// </summary>
        /// <param name = "portalId">Id of the portal.</param>
        /// <returns>PortalInfo object with portal definition.</returns>
        PortalInfo GetPortal(int portalId);

        /// <summary>
        ///   Gets information of a portal.
        /// </summary>
        /// <param name = "portalId">Id of the portal.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>PortalInfo object with portal definition.</returns>
        PortalInfo GetPortal(int portalId, string cultureCode);

        /// <summary>
        /// Gets the portal.
        /// </summary>
        /// <param name="uniqueId">The unique id.</param>
        /// <returns>Portal info.</returns>
        PortalInfo GetPortal(Guid uniqueId);

        /// <summary>
        /// Get portals in specific culture.
        /// </summary>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns></returns>
        List<PortalInfo> GetPortalList(string cultureCode);

        /// <summary>
        /// Gets information from all portals.
        /// </summary>
        /// <returns>ArrayList of PortalInfo objects.</returns>
        ArrayList GetPortals();

        /// <summary>
        /// Gets the portal settings dictionary.
        /// </summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>portal settings.</returns>
        Dictionary<string, string> GetPortalSettings(int portalId);

        /// <summary>
        /// Gets the portal settings dictionary.
        /// </summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>portal settings.</returns>
        Dictionary<string, string> GetPortalSettings(int portalId, string cultureCode);

        /// <summary>
        /// Gets the portal space used bytes.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>Space used in bytes.</returns>
        long GetPortalSpaceUsedBytes(int portalId = -1);

        /// <summary>
        /// Load info for a portal template.
        /// </summary>
        /// <param name="templateFileName">The file name of the portal template.</param>
        /// <param name="cultureCode">the culture code if any for the localization of the portal template.</param>
        /// <returns>A portal template.</returns>
        PortalController.PortalTemplateInfo GetPortalTemplate(string templateFileName, string cultureCode);

        /// <summary>
        /// Verifies if there's enough space to upload a new file on the given portal.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="fileSizeBytes">Size of the file being uploaded.</param>
        /// <returns>True if there's enough space available to upload the file.</returns>
        bool HasSpaceAvailable(int portalId, long fileSizeBytes);

        /// <summary>
        ///   Remaps the Special Pages such as Home, Profile, Search
        ///   to their localized versions.
        /// </summary>
        /// <remarks>
        /// </remarks>
        void MapLocalizedSpecialPages(int portalId, string cultureCode);

        /// <summary>
        /// Removes the related PortalLocalization record from the database, adds optional clear cache.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        void RemovePortalLocalization(int portalId, string cultureCode, bool clearCache = true);

        /// <summary>
        /// Processess a template file for the new portal.
        /// </summary>
        /// <param name="portalId">PortalId of the new portal.</param>
        /// <param name="template">The template.</param>
        /// <param name="administratorId">UserId for the portal administrator. This is used to assign roles to this user.</param>
        /// <param name="mergeTabs">Flag to determine whether Module content is merged.</param>
        /// <param name="isNewPortal">Flag to determine is the template is applied to an existing portal or a new one.</param>
        /// <remarks>
        /// The roles and settings nodes will only be processed on the portal template file.
        /// </remarks>
        void ParseTemplate(int portalId, PortalController.PortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal);

        /// <summary>
        /// Processes the resource file for the template file selected.
        /// </summary>
        /// <param name="portalPath">New portal's folder.</param>
        /// <param name="resoureceFile">full path to the resource file.</param>
        /// <remarks>
        /// The resource file is a zip file with the same name as the selected template file and with
        /// an extension of .resources (to disable this file being downloaded).
        /// For example: for template file "portal.template" a resource file "portal.template.resources" can be defined.
        /// </remarks>
        void ProcessResourceFileExplicit(string portalPath, string resoureceFile);

        /// <summary>
        /// Updates the portal expiry.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="cultureCode">The culture code.</param>
        void UpdatePortalExpiry(int portalId, string cultureCode);

        /// <summary>
        /// Updates basic portal information.
        /// </summary>
        /// <param name="portal"></param>
        void UpdatePortalInfo(PortalInfo portal);

        [Obsolete("Deprecated in DNN 9.2.0. Use the overloaded one with the 'isSecure' parameter instead. Scheduled removal in v11.0.0.")]
        void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode);

        /// <summary>
        /// Adds or Updates or Deletes a portal setting value.
        /// </summary>
        void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode, bool isSecure);

        /// <summary>
        /// Adds the portal dictionary.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        void AddPortalDictionary(int portalId, int tabId);

        /// <summary>
        /// Creates the root folder for a child portal.
        /// </summary>
        /// <remarks>
        /// If call this method, it will create the specific folder if the folder doesn't exist;
        /// and will copy subhost.aspx to the folder if there is no 'Default.aspx'.
        /// </remarks>
        /// <param name="ChildPath">The child path.</param>
        /// <returns>
        /// If the method executed successful, it will return NullString, otherwise return error message.
        /// </returns>
        /// <example>
        /// <code lang="C#">
        /// string childPhysicalPath = Server.MapPath(childPath);
        /// message = PortalController.CreateChildPortalFolder(childPhysicalPath);
        /// </code>
        /// </example>
        string CreateChildPortalFolder(string childPath);

        /// <summary>
        /// Deletes all expired portals.
        /// </summary>
        /// <param name="serverPath">The server path.</param>
        void DeleteExpiredPortals(string serverPath);

        /// <summary>
        /// Deletes the portal.
        /// </summary>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>If the method executed successful, it will return NullString, otherwise return error message.</returns>
        string DeletePortal(PortalInfo portal, string serverPath);

        /// <summary>
        /// Get the portal folder froma child portal alias.
        /// </summary>
        /// <param name="alias">portal alias.</param>
        /// <returns>folder path of the child portal.</returns>
        string GetPortalFolder(string alias);

        /// <summary>Delete the child portal folder and try to remove its parent when parent folder is empty.</summary>
        /// <param name="serverPath">the server path.</param>
        /// <param name="portalFolder">the child folder path.</param>
        void DeletePortalFolder(string serverPath, string portalFolder);

        /// <summary>
        /// Gets the portal dictionary.
        /// </summary>
        /// <returns>portal dictionary. the dictionary's Key -> Value is: TabId -> PortalId.</returns>
        Dictionary<int, int> GetPortalDictionary();

        /// <summary>
        /// GetPortalsByName gets all the portals whose name matches a provided filter expression.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="nameToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of PortalInfo objects.</returns>
        ArrayList GetPortalsByName(string nameToMatch, int pageIndex, int pageSize, ref int totalRecords);

        ArrayList GetPortalsByUser(int userId);

        int GetEffectivePortalId(int portalId);

        /// <summary>
        /// Gets all expired portals.
        /// </summary>
        /// <returns>all expired portals as array list.</returns>
        ArrayList GetExpiredPortals();

        /// <summary>
        /// Determines whether the portal is child portal.
        /// </summary>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>
        ///   <c>true</c> if the portal is child portal; otherwise, <c>false</c>.
        /// </returns>
        bool IsChildPortal(PortalInfo portal, string serverPath);

        bool IsMemberOfPortalGroup(int portalId);

        /// <summary>
        /// Deletes the portal setting (neutral and for all languages).
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        void DeletePortalSetting(int portalID, string settingName);

        /// <summary>
        /// Deletes the portal setting in this language.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="cultureCode">The culture code.</param>
        void DeletePortalSetting(int portalID, string settingName, string cultureCode);

        /// <summary>
        /// Deletes all portal settings by portal id.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        void DeletePortalSettings(int portalID);

        /// <summary>
        /// Deletes all portal settings by portal id and for a given language (Null: all languages and neutral settings).
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        void DeletePortalSettings(int portalID, string cultureCode);

        /// <summary>
        /// takes in a text value, decrypts it with a FIPS compliant algorithm and returns the value.
        /// </summary>
        /// <param name="settingName">the setting to read.</param>
        /// <param name="passPhrase">the pass phrase used for encryption/decryption.</param>
        /// <returns></returns>
        string GetEncryptedString(string settingName, int portalID, string passPhrase);

        /// <summary>
        /// Gets the portal setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        string GetPortalSetting(string settingName, int portalID, string defaultValue);

        /// <summary>
        /// Gets the portal setting for a specific language (or neutral).
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        string GetPortalSetting(string settingName, int portalID, string defaultValue, string cultureCode);

        /// <summary>
        /// Gets the portal setting as boolean.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        bool GetPortalSettingAsBoolean(string key, int portalID, bool defaultValue);

        /// <summary>
        /// Gets the portal setting as boolean for a specific language (or neutral).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        bool GetPortalSettingAsBoolean(string key, int portalID, bool defaultValue, string cultureCode);

        /// <summary>
        /// Gets the portal setting as integer.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        int GetPortalSettingAsInteger(string key, int portalID, int defaultValue);

        /// <summary>
        /// Gets the portal setting as double.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal Id.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        double GetPortalSettingAsDouble(string key, int portalId, double defaultValue);

        /// <summary>
        /// Gets the portal setting as integer for a specific language (or neutral).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting (for specified lang, otherwise return defaultValue.</returns>
        int GetPortalSettingAsInteger(string key, int portalID, int defaultValue, string cultureCode);

        /// <summary>
        /// takes in a text value, encrypts it with a FIPS compliant algorithm and stores.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">host settings key.</param>
        /// <param name="settingValue">host settings value.</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption.</param>
        void UpdateEncryptedString(int portalID, string settingName, string settingValue, string passPhrase);

        /// <summary>
        /// Updates a single neutral (not language specific) portal setting and clears it from the cache.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        void UpdatePortalSetting(int portalID, string settingName, string settingValue);

        /// <summary>
        /// Updates a single neutral (not language specific) portal setting, optionally without clearing the cache.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <c>true</c> [clear cache].</param>
        void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache);

        /// <summary>
        /// Checks the desktop modules whether is installed.
        /// </summary>
        /// <param name="nav">The nav.</param>
        /// <returns>Empty string if the module hasn't been installed, otherwise return the friendly name.</returns>
        string CheckDesktopModulesInstalled(XPathNavigator nav);

        /// <summary>
        ///   function provides the language for portalinfo requests
        ///   in case where language has not been installed yet, will return the core install default of en-us.
        /// </summary>
        /// <param name="portalID"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        string GetActivePortalLanguage(int portalID);

        /// <summary>
        ///   return the current DefaultLanguage value from the Portals table for the requested Portalid.
        /// </summary>
        /// <param name = "portalID"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        string GetPortalDefaultLanguage(int portalID);

        /// <summary>
        ///   set the required DefaultLanguage in the Portals table for a particular portal
        ///   saves having to update an entire PortalInfo object.
        /// </summary>
        /// <param name="portalID"></param>
        /// <param name="CultureCode"></param>
        /// <remarks>
        /// </remarks>
        void UpdatePortalDefaultLanguage(int portalID, string cultureCode);

        void IncrementCrmVersion(int portalID);

        void IncrementOverridingPortalsCrmVersion();
    }
}
