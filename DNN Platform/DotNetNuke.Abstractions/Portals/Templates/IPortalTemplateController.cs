// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals.Templates
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Work with Portal Templates.</summary>
    public interface IPortalTemplateController
    {
        /// <summary>Apply a portal template to an existing (can be newly created) portal.</summary>
        /// <param name="portalId">PortalId of the portal.</param>
        /// <param name="template">The template.</param>
        /// <param name="administratorId">UserId for the portal administrator. This is used to assign roles to this user.</param>
        /// <param name="mergeTabs">Flag to determine whether Module content is merged.</param>
        /// <param name="isNewPortal">Flag to determine whether the template is applied to an existing portal or a newly created one.</param>
        /// <remarks>
        /// When creating a new portal in DNN (PortalController.CreatePortal) the entry in the DB is first created and the
        /// necessary folders on disk. After that this method is run to finish setting up the new portal. But one can also apply
        /// a template to an existing portal. How clashes are handled is determined by the mergeTabs argument.
        /// The roles and settings nodes will only be processed on the portal template file.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        void ApplyPortalTemplate(int portalId, IPortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal);

        /// <summary>Export a portal into a portal template.</summary>
        /// <param name="portalId">PortalId of the portal to be exported.</param>
        /// <param name="fileName">The filename to use when writing the portal template to disk. Note it will be written to the host
        /// directory (Portals/_default).</param>
        /// <param name="description">Description of the template.</param>
        /// <param name="isMultiLanguage">Whether the template is a multi-language template.</param>
        /// <param name="locales">A list of locales that are to be exported.</param>
        /// <param name="localizationCulture">The default locale.</param>
        /// <param name="exportTabIds">A list of tab ids to export. These should be checked beforehand to ensure they form a continuous
        /// chain (i.e. no orphaned children or the resulting template can't be used).</param>
        /// <param name="includeContent">Whether to include module content.</param>
        /// <param name="includeFiles">Whether to include files found in the portal folder.</param>
        /// <param name="includeModules">Whether to include modules.</param>
        /// <param name="includeProfile">Whether to include user profile settings.</param>
        /// <param name="includeRoles">Whether to include the portal's roles.</param>
        /// <returns>A boolean indicating success and a string with an (success or error) message.</returns>
        (bool Success, string Message) ExportPortalTemplate(int portalId, string fileName, string description, bool isMultiLanguage, IEnumerable<string> locales, string localizationCulture, IEnumerable<int> exportTabIds, bool includeContent, bool includeFiles, bool includeModules, bool includeProfile, bool includeRoles);

        /// <summary>Load info for a portal template.</summary>
        /// <param name="templatePath">Full path to the portal template.</param>
        /// <param name="cultureCode">the culture code if any for the localization of the portal template.</param>
        /// <returns>A portal template.</returns>
        IPortalTemplateInfo GetPortalTemplate(string templatePath, string cultureCode);

        /// <summary>Get all the available portal templates grouped by culture.</summary>
        /// <returns>List of PortalTemplateInfo objects.</returns>
        IList<IPortalTemplateInfo> GetPortalTemplates();
    }
}
