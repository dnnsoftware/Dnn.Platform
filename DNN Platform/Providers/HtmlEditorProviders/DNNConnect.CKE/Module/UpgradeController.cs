using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Upgrade;

namespace DNNConnect.CKEditorProvider.Module
{

    /// <summary>
    /// Add Settings Module to Host -> Html Editor Manager Page
    /// </summary>
    public class UpgradeController : IUpgradeable
    {
        /// <summary>
        /// Upgrades the module.
        /// </summary>
        /// <param name="version">The <paramref name="version"/> number string.</param>
        /// <returns>Returns if Upgrade was Success fully or not</returns>
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
                var moduleDefId = GetModuleDefinitionID();

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

        /// <summary>
        /// Gets the module definition ID.
        /// </summary>
        /// <returns>Returns the module definition ID</returns>
        private static int GetModuleDefinitionID()
        {
            var editorDesktopModule =
                DesktopModuleController.GetDesktopModuleByModuleName("CKEditor.EditorConfigManager", Null.NullInteger);

            if (editorDesktopModule == null)
            {
                return -1;
            }

            // get module definition
            var editorModuleDefinition =
                ModuleDefinitionController.GetModuleDefinitionByFriendlyName(
                    "CKEditor Config Manager", editorDesktopModule.DesktopModuleID);

            return editorModuleDefinition == null ? -1 : editorModuleDefinition.ModuleDefID;
        }
    }
}