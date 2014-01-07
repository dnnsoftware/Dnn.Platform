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
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Entities.Portals
{
    /// <summary>
    /// Do not implement.  This interface is meant for reference and unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IPortalController
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a new portal alias
        /// </summary>
        /// <param name="portalId">Id of the portal</param>
        /// <param name="portalAlias">Portal Alias to be created</param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]    01/11/2005  created
        /// </history>
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
        /// <param name="adminUser">The obj admin user.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="templatePath">The template path.</param>
        /// <param name="templateFile">The template file.</param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="childPath">The child path.</param>
        /// <param name="isChildPortal">if set to <c>true</c> means the portal is child portal.</param>
        /// <returns>Portal id.</returns>
        int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, string templatePath, string templateFile, string homeDirectory, string portalAlias,
                         string serverPath, string childPath, bool isChildPortal);


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a new portal.
        /// </summary>
        /// <param name="portalName">Name of the portal to be created</param>
        /// <param name="firstName">Portal Administrator's first name</param>
        /// <param name="lastName">Portal Administrator's last name</param>
        /// <param name="username">Portal Administrator's username</param>
        /// <param name="password">Portal Administrator's password</param>
        /// <param name="email">Portal Administrator's email</param>
        /// <param name="description">Description for the new portal</param>
        /// <param name="keyWords">KeyWords for the new portal</param>
        /// <param name="templatePath">Path where the templates are stored</param>
        /// <param name="templateFile">Template file</param>
        /// <param name="homeDirectory">Home Directory</param>
        /// <param name="portalAlias">Portal Alias String</param>
        /// <param name="serverPath">The Path to the root of the Application</param>
        /// <param name="childPath">The Path to the Child Portal Folder</param>
        /// <param name="isChildPortal">True if this is a child portal</param>
        /// <returns>PortalId of the new portal if there are no errors, -1 otherwise.</returns>
        /// <remarks>
        /// After the selected portal template is parsed the admin template ("admin.template") will be
        /// also processed. The admin template should only contain the "Admin" menu since it's the same
        /// on all portals. The selected portal template can contain a <settings/> node to specify portal
        /// properties and a <roles/> node to define the roles that will be created on the portal by default.
        /// </remarks>
        /// <history>
        /// 	[cnurse]	11/08/2004	created (most of this code was moved from SignUp.ascx.vb)
        /// </history>
        /// -----------------------------------------------------------------------------
        int CreatePortal(string portalName, string firstName, string lastName, string username, string password, string email, string description, string keyWords, string templatePath,
                         string templateFile, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal);

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
        int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords,
                         PortalController.PortalTemplateInfo template,
                         string homeDirectory, string portalAlias, string serverPath, string childPath,
                         bool isChildPortal);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a portal permanently
        /// </summary>
        /// <param name="portalId">PortalId of the portal to be deleted</param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	03/09/2004	Created
        /// 	[VMasanas]	26/10/2004	Remove dependent data (skins, modules)
        ///     [cnurse]    24/11/2006  Removal of Modules moved to sproc
        /// </history>
        /// -----------------------------------------------------------------------------
        void DeletePortalInfo(int portalId);



        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets information of a portal
        /// </summary>
        /// <param name = "portalId">Id of the portal</param>
        /// <returns>PortalInfo object with portal definition</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        PortalInfo GetPortal(int portalId);

        PortalInfo GetPortal(int portalId, string cultureCode);

        List<PortalInfo> GetPortalList(string cultureCode);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets information from all portals
        /// </summary>
        /// <returns>ArrayList of PortalInfo objects</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        ArrayList GetPortals();

        /// <summary>
        /// Gets the portal.
        /// </summary>
        /// <param name="uniqueId">The unique id.</param>
        /// <returns>Portal info.</returns>
        PortalInfo GetPortal(Guid uniqueId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the space used at the host level
        /// </summary>
        /// <returns>Space used in bytes</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	19/04/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        long GetPortalSpaceUsedBytes();

        /// <summary>
        /// Gets the portal space used bytes.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>Space used in bytes</returns>
        long GetPortalSpaceUsedBytes(int portalId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Verifies if there's enough space to upload a new file on the given portal
        /// </summary>
        /// <param name="portalId">Id of the portal</param>
        /// <param name="fileSizeBytes">Size of the file being uploaded</param>
        /// <returns>True if there's enough space available to upload the file</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	19/04/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        bool HasSpaceAvailable(int portalId, long fileSizeBytes);

        /// <summary>
        ///   Remaps the Special Pages such as Home, Profile, Search
        ///   to their localized versions
        /// </summary>
        /// <remarks>
        /// </remarks>
        void MapLocalizedSpecialPages(int portalId, string cultureCode);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Processess a template file for the new portal. This method will be called twice: for the portal template and for the admin template
        /// </summary>
        /// <param name="portalId">PortalId of the new portal</param>
        /// <param name="templatePath">Path for the folder where templates are stored</param>
        /// <param name="templateFile">Template file to process</param>
        /// <param name="administratorId">UserId for the portal administrator. This is used to assign roles to this user</param>
        /// <param name="mergeTabs">Flag to determine whether Module content is merged.</param>
        /// <param name="isNewPortal">Flag to determine is the template is applied to an existing portal or a new one.</param>
        /// <remarks>
        /// The roles and settings nodes will only be processed on the portal template file.
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	27/08/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        void ParseTemplate(int portalId, string templatePath, string templateFile, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Processes the resource file for the template file selected
        /// </summary>
        /// <param name="portalPath">New portal's folder</param>
        /// <param name="templateFile">Selected template file</param>
        /// <remarks>
        /// The resource file is a zip file with the same name as the selected template file and with
        /// an extension of .resources (to unable this file being downloaded).
        /// For example: for template file "portal.template" a resource file "portal.template.resources" can be defined.
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	10/09/2004	Created
        ///     [cnurse]    11/08/2004  Moved from SignUp to PortalController
        ///     [cnurse]    03/04/2005  made Public
        ///     [cnurse]    05/20/2005  moved most of processing to new method in FileSystemUtils
        /// </history>
        /// -----------------------------------------------------------------------------
        void ProcessResourceFile(string portalPath, string templateFile);

        /// <summary>
        /// Updates the portal expiry.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        void UpdatePortalExpiry(int portalId);

        /// <summary>
        /// Updates the portal expiry.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="cultureCode">The culture code.</param>
        void UpdatePortalExpiry(int portalId, string cultureCode);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates basic portal information
        /// </summary>
        /// <param name="portal"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	10/13/2004	created
        /// </history>
        /// -----------------------------------------------------------------------------
        void UpdatePortalInfo(PortalInfo portal);

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
        /// <returns>portal settings.</returns>
        PortalSettings GetCurrentPortalSettings();

        /// <summary>
        /// Get all the available portal templates grouped by culture
        /// </summary>
        /// <returns>List of PortalTemplateInfo objects</returns>
        IList<PortalController.PortalTemplateInfo> GetAvailablePortalTemplates();

        /// <summary>
        /// Load info for a portal template
        /// </summary>
        /// <param name="templateFileName">The file name of the portal template</param>
        /// <param name="cultureCode">the culture code if any for the localization of the portal template</param>
        /// <returns>A portal template</returns>
        PortalController.PortalTemplateInfo GetPortalTemplate(string templateFileName, string cultureCode);
    }
}
        
    
