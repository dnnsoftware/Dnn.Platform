// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals.Templates
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals.Internal;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc cref="IPortalTemplateController"/>
    public class PortalTemplateController : ServiceLocator<IPortalTemplateController, PortalTemplateController>, IPortalTemplateController
    {
        private readonly ListController listController;
        private readonly IHostSettings hostSettings;
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalGroupController portalGroupController;
        private readonly IUserController userController;
        private readonly IFileContentTypeManager fileContentTypeManager;
        private readonly RoleProvider roleProvider;
        private readonly IRoleController roleController;
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly IEventLogger eventLogger;
        private readonly IPermissionDefinitionService permissionDefinitionService;

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public PortalTemplateController()
            : this(null, null, null, null, null, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateController"/> class.</summary>
        /// <param name="businessControllerProvider">The DI container.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public PortalTemplateController(IBusinessControllerProvider businessControllerProvider)
            : this(null, businessControllerProvider, null, null, null, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateController"/> class.</summary>
        /// <param name="businessControllerProvider">The DI container.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.3. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public PortalTemplateController(IBusinessControllerProvider businessControllerProvider, IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService)
            : this(permissionDefinitionService, businessControllerProvider, null, eventLogger, null, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateController"/> class.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="fileContentTypeManager">The file content type manager.</param>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="roleController">The role controller.</param>
        public PortalTemplateController(IPermissionDefinitionService permissionDefinitionService, IBusinessControllerProvider businessControllerProvider, ListController listController, IEventLogger eventLogger, IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IUserController userController, IFileContentTypeManager fileContentTypeManager, RoleProvider roleProvider, IRoleController roleController)
        {
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.DependencyProvider.GetRequiredService<IPermissionDefinitionService>();
            this.businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.listController = listController ?? Globals.DependencyProvider.GetRequiredService<ListController>();
            this.eventLogger = eventLogger ?? Globals.DependencyProvider.GetRequiredService<IEventLogger>();
            this.hostSettings = hostSettings ?? Globals.DependencyProvider.GetRequiredService<IHostSettings>();
            this.portalController = portalController ?? Globals.DependencyProvider.GetRequiredService<IPortalController>();
            this.appStatus = appStatus ?? Globals.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
            this.portalGroupController = portalGroupController ?? Globals.DependencyProvider.GetRequiredService<IPortalGroupController>();
            this.userController = userController ?? Globals.DependencyProvider.GetRequiredService<IUserController>();
            this.fileContentTypeManager = fileContentTypeManager ?? Globals.DependencyProvider.GetRequiredService<IFileContentTypeManager>();
            this.roleProvider = roleProvider ?? Globals.DependencyProvider.GetRequiredService<RoleProvider>();
            this.roleController = roleController ?? Globals.DependencyProvider.GetRequiredService<IRoleController>();
        }

        /// <inheritdoc />
        public void ApplyPortalTemplate(int portalId, IPortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            var importer = new PortalTemplateImporter(this.permissionDefinitionService, this.businessControllerProvider, this.listController, this.eventLogger, this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, this.userController, this.fileContentTypeManager, this.roleProvider, this.roleController, template);
            importer.ParseTemplate(portalId, administratorId, mergeTabs, isNewPortal);
        }

        /// <inheritdoc />
        public (bool Success, string Message) ExportPortalTemplate(int portalId, string fileName, string description, bool isMultiLanguage, IEnumerable<string> locales, string localizationCulture, IEnumerable<int> exportTabIds, bool includeContent, bool includeFiles, bool includeModules, bool includeProfile, bool includeRoles)
        {
            var exporter = new PortalTemplateExporter(this.businessControllerProvider, this.listController, this.portalController, this.hostSettings, this.roleProvider, this.appStatus, this.portalGroupController);
            return exporter.ExportPortalTemplate(portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles);
        }

        /// <inheritdoc />
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
                var langs = languageFileNames.Where(x => GetTemplateName(x).Equals(currentFileName, StringComparison.OrdinalIgnoreCase)).Select(x => GetCultureCode(x)).Distinct().ToList();

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
