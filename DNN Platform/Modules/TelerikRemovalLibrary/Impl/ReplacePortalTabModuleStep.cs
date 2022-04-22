// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary.Impl
{
    using System;
    using System.Linq;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;

    /// <inheritdoc />
    internal class ReplacePortalTabModuleStep : StepBase, IReplacePortalTabModuleStep
    {
        private readonly IModuleController moduleController;
        private readonly ITabController tabController;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplacePortalTabModuleStep"/> class.
        /// </summary>
        /// <param name="moduleController">An instance of <see cref="IModuleController"/>.</param>
        /// <param name="tabController">An instance of <see cref="ITabController"/>.</param>
        public ReplacePortalTabModuleStep(IModuleController moduleController, ITabController tabController)
        {
            this.moduleController = moduleController ??
                throw new ArgumentNullException(nameof(moduleController));

            this.tabController = tabController ??
                throw new ArgumentNullException(nameof(tabController));
        }

        /// <inheritdoc />
        public override string Name => string.Format(
            "Replace '{0}' with '{1}' in page '{2}', portal ID '{3}'",
            this.OldModuleName,
            this.NewModuleName,
            this.PageName,
            this.PortalId);

        /// <inheritdoc />
        public IReplaceTabModuleStep ParentStep { get; set; }

        /// <inheritdoc />
        public int PortalId { get; set; }

        private string OldModuleName => this.ParentStep?.OldModuleName;

        private string NewModuleName => this.ParentStep?.NewModuleName;

        private string PageName => this.ParentStep?.PageName;

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
            if (this.ParentStep == null)
            {
                throw new InvalidOperationException("Parent step not set.");
            }

            var tab = this.tabController.GetTabByName(this.PageName, this.PortalId);

            if (tab is null)
            {
                this.Success = true;
                this.Notes = $"Page '{this.PageName}' not found in portal '{this.PortalId}'.";
                return;
            }

            var modules = tab.Modules
                .Cast<ModuleInfo>()
                .Where(m => this.OldModuleName.Equals(m.ModuleDefinition.DefinitionName, StringComparison.OrdinalIgnoreCase));

            if (!modules.Any())
            {
                this.Success = true;
                this.Notes = $"'{this.OldModuleName}' module not found in '{this.PageName}' page.";
            }

            foreach (var module in modules)
            {
                this.moduleController.DeleteTabModule(tab.TabID, module.KeyID, false);
            }

            this.moduleController.ClearCache(tab.TabID);

            this.Success = true;
        }
    }
}
