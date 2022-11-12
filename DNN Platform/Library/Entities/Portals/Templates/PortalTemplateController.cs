// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Templates
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Framework;

    /// <inheritdoc/>
    public class PortalTemplateController : ServiceLocator<IPortalTemplateController, PortalTemplateController>, IPortalTemplateController
    {
        /// <inheritdoc/>
        public void ApplyPortalTemplate(int portalId, IPortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            var importer = new PortalTemplateImporter(template);
            importer.ParseTemplate(portalId, administratorId, mergeTabs, isNewPortal);
        }

        /// <inheritdoc/>
        public (bool success, string message) ExportPortalTemplate(int portalId, string fileName, string description, bool isMultiLanguage, IEnumerable<string> locales, string localizationCulture, IEnumerable<int> exportTabIds, bool includeContent, bool includeFiles, bool includeModules, bool includeProfile, bool includeRoles)
        {
            var exporter = new PortalTemplateExporter();
            return exporter.ExportPortalTemplate(portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles);
        }

        /// <summary>
        /// Instantiates a new instance of the PortalTemplateController.
        /// </summary>
        /// <returns>An instance of IPortalTemplateController.</returns>
        protected override Func<IPortalTemplateController> GetFactory()
        {
            return () => new PortalTemplateController();
        }
    }
}
