// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Shims;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <inheritdoc />
    internal class ReplacePortalTabModuleStep : StepBase, IReplacePortalTabModuleStep
    {
        private readonly IModuleController moduleController;
        private readonly ITabController tabController;
        private readonly IDesktopModuleController desktopModuleController;
        private readonly IModuleDefinitionController moduleDefinitionController;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplacePortalTabModuleStep"/> class.
        /// </summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="moduleController">An instance of <see cref="IModuleController"/>.</param>
        /// <param name="tabController">An instance of <see cref="ITabController"/>.</param>
        /// <param name="desktopModuleController">An instance of <see cref="IDesktopModuleController"/>.</param>
        /// <param name="moduleDefinitionController">An instance of <see cref="IModuleDefinitionController"/>.</param>
        public ReplacePortalTabModuleStep(
            ILoggerSource loggerSource,
            ILocalizer localizer,
            IModuleController moduleController,
            ITabController tabController,
            IDesktopModuleController desktopModuleController,
            IModuleDefinitionController moduleDefinitionController)
            : base(loggerSource, localizer)
        {
            this.moduleController = moduleController ??
                throw new ArgumentNullException(nameof(moduleController));

            this.tabController = tabController ??
                throw new ArgumentNullException(nameof(tabController));

            this.desktopModuleController = desktopModuleController ??
                throw new ArgumentNullException(nameof(desktopModuleController));

            this.moduleDefinitionController = moduleDefinitionController ??
                throw new ArgumentNullException(nameof(moduleDefinitionController));
        }

        /// <inheritdoc />
        public override string Name => this.LocalizeFormat(
            "UninstallStepReplacePortalPageModule",
            this.OldModuleName,
            this.NewModuleName,
            this.PageName,
            this.PortalId);

        /// <inheritdoc />
        [Required]
        public IReplaceTabModuleStep ParentStep { get; set; }

        /// <inheritdoc />
        [Required]
        public int PortalId { get; set; }

        private string OldModuleName => this.ParentStep?.OldModuleName;

        private string NewModuleName => this.ParentStep?.NewModuleName;

        private string PageName => this.ParentStep?.PageName;

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
            var tab = this.tabController.GetTabByName(this.PageName, this.PortalId);

            if (tab is null)
            {
                this.Success = true;
                this.Notes = this.LocalizeFormat("UninstallStepPageNotFoundInPortal", this.PageName, this.PortalId);
                return;
            }

            var modules = tab.Modules
                .Cast<ModuleInfo>()
                .Where(m => this.OldModuleName.Equals(m.ModuleDefinition.DefinitionName, StringComparison.OrdinalIgnoreCase));

            if (!modules.Any())
            {
                this.Success = true;
                this.Notes = this.LocalizeFormat("UninstallStepModuleNotFoundInPage", this.OldModuleName, this.PageName);
            }

            foreach (var module in modules)
            {
                this.DeleteTheOldModule(module);
                this.AddTheNewModule(module);
            }

            this.Success = true;
        }

        private static ModuleInfo CloneModule(ModuleInfo module)
        {
            return new ModuleInfo
            {
                Alignment = module.Alignment,
                AllModules = module.AllModules,
                AllTabs = module.AllTabs,
                Border = module.Border,
                CacheMethod = module.CacheMethod,
                CacheTime = module.CacheTime,
                Color = module.Color,
                ContainerPath = module.ContainerPath,
                ContainerSrc = module.ContainerSrc,
                CultureCode = module.CultureCode,
                DesktopModuleID = module.DesktopModuleID,
                DisplayPrint = module.DisplayPrint,
                DisplaySyndicate = module.DisplaySyndicate,
                DisplayTitle = module.DisplayTitle,
                EndDate = module.EndDate,
                Footer = module.Footer,
                Header = module.Header,
                IconFile = module.IconFile,
                IsDefaultModule = module.IsDefaultModule,
                IsDeleted = module.IsDeleted,
                IsShareable = module.IsShareable,
                IsShareableViewOnly = module.IsShareableViewOnly,
                ModuleControlId = Null.NullInteger,
                OwnerPortalID = module.OwnerPortalID,
                PaneModuleCount = 0,
                PaneModuleIndex = 0,
                PaneName = module.PaneName,
                PortalID = module.PortalID,
                StartDate = module.StartDate,
                TabID = module.TabID,
                Visibility = module.Visibility,
            };
        }

        private int AddTheNewModule(ModuleInfo oldModule)
        {
            var tabModuleId = Null.NullInteger;

            var desktopModule = this.desktopModuleController.GetDesktopModuleByModuleName(
                this.NewModuleName,
                oldModule.PortalID);

            var position = oldModule.ModuleOrder;
            var permissionType = oldModule.InheritViewPermissions ? 0 : 1;

            var definitions = this.moduleDefinitionController
                .GetModuleDefinitionsByDesktopModuleID(desktopModule.DesktopModuleID)
                .Values;

            foreach (var definition in definitions)
            {
                var newModule = CloneModule(oldModule);

                newModule.DesktopModuleID = desktopModule.DesktopModuleID;
                newModule.ModuleDefID = definition.ModuleDefID;
                newModule.ModuleOrder = position;
                newModule.ModuleTitle = definition.FriendlyName;

                this.moduleController.InitialModulePermission(newModule, newModule.TabID, permissionType);
                this.moduleController.AddModule(newModule);

                if (tabModuleId == Null.NullInteger)
                {
                    tabModuleId = newModule.ModuleID;
                }

                position = this.moduleController.GetTabModule(newModule.TabModuleID).ModuleOrder + 1;
            }

            this.moduleController.ClearCache(oldModule.TabID);

            return tabModuleId;
        }

        private void DeleteTheOldModule(ModuleInfo module)
        {
            this.moduleController.DeleteTabModule(module.TabID, module.KeyID, softDelete: false);
            this.moduleController.ClearCache(module.TabID);
        }
    }
}
