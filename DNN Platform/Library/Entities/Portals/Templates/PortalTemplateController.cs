// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals.Templates
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals.Internal;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc cref="IPortalTemplateController"/>
    public class PortalTemplateController : ServiceLocator<IPortalTemplateController, PortalTemplateController>, IPortalTemplateController
    {
        private readonly IBusinessControllerProvider businessControllerProvider;

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public PortalTemplateController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateController"/> class.</summary>
        /// <param name="businessControllerProvider">The DI container.</param>
        public PortalTemplateController(IBusinessControllerProvider businessControllerProvider)
        {
            this.businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
        }

        /// <inheritdoc/>
        public void ApplyPortalTemplate(int portalId, IPortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            var importer = new PortalTemplateImporter(template);
            importer.ParseTemplate(this.businessControllerProvider, portalId, administratorId, mergeTabs, isNewPortal);
        }

        /// <inheritdoc/>
        public (bool Success, string Message) ExportPortalTemplate(int portalId, string fileName, string description, bool isMultiLanguage, IEnumerable<string> locales, string localizationCulture, IEnumerable<int> exportTabIds, bool includeContent, bool includeFiles, bool includeModules, bool includeProfile, bool includeRoles)
        {
            var exporter = new PortalTemplateExporter();
            return exporter.ExportPortalTemplate(this.businessControllerProvider, portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles);
        }

        /// <inheritdoc/>
        public IPortalTemplateInfo GetPortalTemplate(string templatePath, string cultureCode)
        {
            var template = new PortalTemplateInfo(templatePath, cultureCode);

            if (!string.IsNullOrEmpty(cultureCode) && template.CultureCode != cultureCode)
            {
                return null;
            }

            return template;
        }

        public IList<IPortalTemplateInfo> GetPortalTemplates()
        {
            var list = new List<IPortalTemplateInfo>();

            var templateFilePaths = PortalTemplateIO.Instance.EnumerateTemplates();
            var languageFileNames = PortalTemplateIO.Instance.EnumerateLanguageFiles().Select(Path.GetFileName).ToList();

            foreach (string templateFilePath in templateFilePaths)
            {
                var currentFileName = Path.GetFileName(templateFilePath);
                var langs = languageFileNames.Where(x => GetTemplateName(x).Equals(currentFileName, StringComparison.InvariantCultureIgnoreCase)).Select(x => GetCultureCode(x)).Distinct().ToList();

                if (langs.Count != 0)
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

        /// <summary>Instantiates a new instance of the PortalTemplateController.</summary>
        /// <returns>An instance of IPortalTemplateController.</returns>
        protected override Func<IPortalTemplateController> GetFactory()
        {
            return Globals.DependencyProvider.GetRequiredService<IPortalTemplateController>;
        }

        private static string GetTemplateName(string languageFileName)
        {
            // e.g. "default template.template.en-US.resx"
            return languageFileName.GetFileNameFromLocalizedResxFile();
        }

        private static string GetCultureCode(string languageFileName)
        {
            // e.g. "default template.template.en-US.resx"
            return languageFileName.GetLocaleCodeFromFileName();
        }
    }
}
