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
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
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
                        var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Themes");
                        
                        if (moduleDefinition != null)
                        {
                            //Add Module to Admin Page for all Portals
                            Upgrade.AddAdminPages("Themes",
                                                    "Manage the installed themes, and how they're applied on the site.",
                                                    "~/Icons/Sigma/Skins_16X16_Standard.png",
                                                    "~/Icons/Sigma/Skins_32X32_Standard.png",
                                                    true,
                                                    moduleDefinition.ModuleDefID,
                                                    "Themes",
                                                    "~/Icons/Sigma/Skins_32X32_Standard.png",
                                                    true);

                            // add the theme attributes module to the same admin page
                            var themePage = TabController.Instance.GetTabByName("Themes", portalSettings.PortalId);
                            if (themePage != null)
                            {
                                var attributeDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName("ThemeDesigner");
                                AddAttributeModule(portalSettings.PortalId, themePage, attributeDefinition);
                            }
                        }

                        // update the Skins references to Themes
                        UpdateModuleReferences();

                        // delete the Skins page
                        DeleteSkinsPage(portalSettings.PortalId);

                        // uninstall the old skin modules
                        UninstallOldModules(portalSettings.PortalId);

                        break;
                }

                return "Success";
            }
            catch (Exception)
            {
                return "Failed";
            }
        }

        private void DeleteSkinsPage(int portalId)
        {
            var skinsPage = TabController.Instance.GetTabByName("Skins", portalId);

            if (skinsPage != null)
            {
                TabController.Instance.DeleteTab(skinsPage.TabID, portalId);
            }
        }

        private void UpdateModuleReferences()
        {
            var oldSkinModuleId = GetModuleDefinitionID("Skins");
            var newSkinModuleId = GetModuleDefinitionID("Themes");

            if (oldSkinModuleId > Null.NullInteger && newSkinModuleId > Null.NullInteger)
            {
                UpdateModuleReference(oldSkinModuleId, newSkinModuleId);
            }

            var oldAttributeModuleId = GetModuleDefinitionID("Skin Designer");
            var newAttributeModuleId = GetModuleDefinitionID("Theme Designer");

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
                    string.Format(
                        "UPDATE {databaseOwner}[{objectQualifier}Modules] SET [ModuleDefID] = {0} WHERE [ModuleDefID] = {1}",
                        newModuleDefinitionId, oldModuleDefinitionId));
        }

        private int GetModuleDefinitionID(string friendlyName)
        {
            var definition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(friendlyName);

            return definition.ModuleDefID > Null.NullInteger ? definition.ModuleDefID : Null.NullInteger;
        }

        private void UninstallOldModules(int portalId)
        {
            UninstallOldModule("Skins", portalId);
            UninstallOldModule("SkinDesigner", portalId);
        }

        private void UninstallOldModule(string moduleName, int portalId)
        {
            var dm = DesktopModuleController.GetDesktopModuleByModuleName(moduleName, portalId);
            var package = PackageController.Instance.GetExtensionPackage(portalId, p => p.PackageID == dm.PackageID);
            var installer = new Installer(package, DotNetNuke.Common.Globals.ApplicationMapPath);

            installer.UnInstall(true);
        }

        private void AddAttributeModule(int portalId, TabInfo themeTab, ModuleDefinitionInfo moduleDefinition)
        {
            var objModule = new ModuleInfo();
            
            objModule.Initialize(portalId);

            objModule.PortalID = portalId;
            objModule.TabID = themeTab.TabID;
            objModule.ModuleTitle = moduleDefinition.FriendlyName;
            objModule.PaneName = Globals.glbDefaultPane;
            objModule.ModuleDefID = moduleDefinition.ModuleDefID;
            objModule.InheritViewPermissions = true;
            objModule.AllTabs = false;
            objModule.ModuleOrder = 3;

            ModuleController.Instance.AddModule(objModule);
        }
    }
}
