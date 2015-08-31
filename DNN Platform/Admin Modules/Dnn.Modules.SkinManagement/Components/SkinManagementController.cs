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
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Upgrade;

namespace Dnn.Modules.SkinManagement.Components
{
    /// <summary>
    /// 
    /// </summary>
    public class SkinManagementController : IUpgradeable
    {
        private const string SKIN_NAME = "Skins";
        private const string SKIN_DESIGNER_NAME = "Skin Designer";
        private const string THEME_NAME = "Themes";
        private const string THEME_DESIGNER_NAME = "Theme Designer";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public string UpgradeModule(string version)
        {
            try
            {
                switch (version)
                {
                    case "01.01.00":
                        var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                        var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(THEME_NAME);

                        if (moduleDefinition != null)
                        {
                            //Add Module to Admin Page for all Portals
                            Upgrade.AddAdminPages(THEME_NAME,
                                                    "Manage the installed themes, and how they're applied on the site.",
                                                    "~/Icons/Sigma/Skins_16X16_Standard.png",
                                                    "~/Icons/Sigma/Skins_32X32_Standard.png",
                                                    true,
                                                    moduleDefinition.ModuleDefID,
                                                    THEME_NAME,
                                                    "~/Icons/Sigma/Skins_32X32_Standard.png",
                                                    true);

                            // add the theme attributes module to the same admin page
                            var themePage = TabController.Instance.GetTabByName(THEME_NAME, portalSettings.PortalId);
                            if (themePage != null)
                            {
                                var attributeDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(THEME_DESIGNER_NAME);

                                // only add the attributes module if it's not on the page already
                                if (themePage.Modules.Cast<ModuleInfo>().ToList().All(m => m.ModuleDefID != attributeDefinition.ModuleDefID))
                                {
                                    AddAttributeModule(portalSettings.PortalId, themePage, attributeDefinition);
                                }
                            }
                        }

                        // delete the Skins page
                        DeleteSkinsPage(portalSettings.PortalId);

                        // update the Skins references to Themes
                        UpdateModuleReferences();

                        // uninstall the old skin modules
                        UninstallOldModules();

                        break;
                }

                return "Success";
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return "Failed";
            }
        }

        private void DeleteSkinsPage(int portalId)
        {
            var skinsPage = TabController.Instance.GetTabByName(SKIN_NAME, portalId);

            if (skinsPage != null)
            {
                TabController.Instance.DeleteTab(skinsPage.TabID, portalId);
            }
        }

        private void UpdateModuleReferences()
        {
            var oldSkinModuleId = GetModuleDefinitionID(SKIN_NAME);
            var newSkinModuleId = GetModuleDefinitionID(THEME_NAME);

            if (oldSkinModuleId > Null.NullInteger && newSkinModuleId > Null.NullInteger)
            {
                UpdateModuleReference(oldSkinModuleId, newSkinModuleId);
            }

            var oldAttributeModuleId = GetModuleDefinitionID(SKIN_DESIGNER_NAME);
            var newAttributeModuleId = GetModuleDefinitionID(THEME_DESIGNER_NAME);

            if (oldAttributeModuleId > Null.NullInteger && newAttributeModuleId > Null.NullInteger)
            {
                UpdateModuleReference(oldAttributeModuleId, newAttributeModuleId);
            }
        }

        private void UpdateModuleReference(int oldModuleDefinitionId, int newModuleDefinitionId)
        {
            // change the module referece from the original ID, to the new ID
            DataProvider.Instance()
                .ExecuteSQL(
                    string.Concat(
                        "UPDATE {databaseOwner}[{objectQualifier}Modules] SET [ModuleDefID] = ",
                        newModuleDefinitionId,
                        " WHERE [ModuleDefID] = ",
                        oldModuleDefinitionId)
                );
        }

        private int GetModuleDefinitionID(string friendlyName)
        {
            var definition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(friendlyName);

            if (definition != null)
            {
                return definition.ModuleDefID > Null.NullInteger ? definition.ModuleDefID : Null.NullInteger;
            }

            return Null.NullInteger;
        }

        private void UninstallOldModules()
        {
            UninstallOldModule(SKIN_NAME);
            UninstallOldModule("SkinDesigner");
        }

        private void UninstallOldModule(string moduleName)
        {
            var dm = DesktopModuleController.GetDesktopModuleByModuleName(moduleName, Null.NullInteger);

            if (dm == null) return;

            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == dm.PackageID);

            var installer = new Installer(package, Globals.ApplicationMapPath);

            installer.UnInstall(true);
        }

        private void AddAttributeModule(int portalId, TabInfo themeTab, ModuleDefinitionInfo moduleDefinition)
        {
            var objModule = new ModuleInfo();

            objModule.Initialize(portalId);

            objModule.PortalID = portalId;
            objModule.TabID = themeTab.TabID;
            objModule.ModuleTitle = moduleDefinition.FriendlyName;
            objModule.PaneName = Globals.glbDefaultPane; // ContentPane
            objModule.ModuleDefID = moduleDefinition.ModuleDefID;
            objModule.InheritViewPermissions = true;
            objModule.AllTabs = false;
            objModule.ModuleOrder = Null.NullInteger; // puts the module on the bottom

            ModuleController.Instance.AddModule(objModule);
        }
    }
}
