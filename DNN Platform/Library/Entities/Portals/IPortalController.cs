// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Users;

    /// <summary>
    /// Do not implement.  This interface is meant for reference and unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IPortalController
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a new portal alias.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="portalAlias">Portal Alias to be created.</param>
        /// -----------------------------------------------------------------------------
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
    }
}
