// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Module
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Upgrade;

    /// <summary>Add Settings Module to Host -> HTML Editor Manager Page.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public class UpgradeController(IHostSettings hostSettings) : IUpgradeable
    {
        private readonly IHostSettings hostSettings = hostSettings ??
                                                      new HostSettings(
                                                          new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                                                              new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                                                              new Lazy<IPortalController>(() => PortalController.Instance)));

        /// <summary>Initializes a new instance of the <see cref="UpgradeController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public UpgradeController()
            : this(null)
        {
        }

        /// <summary>Upgrades the module.</summary>
        /// <param name="version">The <paramref name="version"/> number string.</param>
        /// <returns>Returns if Upgrade was Successfully or not.</returns>
        public string UpgradeModule(string version)
        {
            try
            {
                const string ResourceFile =
                    "~/Providers/HtmlEditorProviders/DNNConnect.CKE/App_LocalResources/EditorConfigManager.ascx.resx";

                var pageName = Localization.GetString("EditorMangerPageName.Text", ResourceFile);
                var moduleTitle = Localization.GetString("EditorMangerName.Text", ResourceFile);
                var pageDescription = Localization.GetString("EditorMangerPageDescription.Text", ResourceFile);

                // Remove wrongly created Host Page
                Upgrade.RemoveHostPage(moduleTitle);

                // Create Config Page (or get existing one)
                var editorManagerPage = Upgrade.AddHostPage(
                    pageName,
                    pageDescription,
                    "~/Providers/HtmlEditorProviders/DNNConnect.CKE/CKEditor/images/editor_config_small.png",
                    "~/Providers/HtmlEditorProviders/DNNConnect.CKE/CKEditor/images/editor_config_large.png",
                    false);

                // Add Module To Page
                var moduleDefId = GetModuleDefinitionID(this.hostSettings);

                Upgrade.AddModuleToPage(
                    editorManagerPage,
                    moduleDefId,
                    moduleTitle,
                    "~/Providers/HtmlEditorProviders/DNNConnect.CKE/LogoCKEditor.png",
                    true);
            }
            catch (Exception ex)
            {
                new ExceptionLogController().AddLog(ex);

                return "Failed";
            }

            return "Success";
        }

        /// <summary>Gets the module definition ID.</summary>
        /// <returns>Returns the module definition ID.</returns>
        private static int GetModuleDefinitionID(IHostSettings hostSettings)
        {
            var editorDesktopModule =
                DesktopModuleController.GetDesktopModuleByModuleName(hostSettings, "CKEditor.EditorConfigManager", Null.NullInteger);

            if (editorDesktopModule == null)
            {
                return -1;
            }

            // get module definition
            var editorModuleDefinition =
                ModuleDefinitionController.GetModuleDefinitionByFriendlyName(
                    "CKEditor Config Manager", editorDesktopModule.DesktopModuleID);

            return editorModuleDefinition?.ModuleDefID ?? -1;
        }
    }
}
