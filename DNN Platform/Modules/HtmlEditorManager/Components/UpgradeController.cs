#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

namespace DotNetNuke.Modules.HtmlEditorManager.Components
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Upgrade;

    /// <summary>
    /// Class that contains upgrade procedures
    /// </summary>
    public class UpgradeController : IUpgradeable
    {
        /// <summary>The module folder location</summary>
        private const string ModuleFolder = "~/DesktopModules/Admin/HtmlEditorManager";

        /// <summary>Called when a module is upgraded.</summary>
        /// <param name="version">The version.</param>
        /// <returns>Success if all goes well, otherwise, Failed</returns>
        public string UpgradeModule(string version)
        {
            try
            {
                switch (version)
                {
                    case "07.04.00":
                        const string ResourceFile = ModuleFolder + "/App_LocalResources/ProviderConfiguration.ascx.resx";
                        string pageName = Localization.GetString("HTMLEditorPageName", ResourceFile);
                        string pageDescription = Localization.GetString("HTMLEditorPageDescription", ResourceFile);

                        // Create HTML Editor Config Page (or get existing one)
                        TabInfo editorPage = Upgrade.AddHostPage(pageName, pageDescription, ModuleFolder + "/images/HtmlEditorManager_Standard_16x16.png", ModuleFolder + "/images/HtmlEditorManager_Standard_32x32.png", false);

                        // Find the RadEditor control and remove it
                        Upgrade.RemoveModule("RadEditor Manager", editorPage.TabName, editorPage.ParentId, false);

                        // Add Module To Page
                        int moduleDefId = this.GetModuleDefinitionID("DotNetNuke.HtmlEditorManager", "Html Editor Management");
                        Upgrade.AddModuleToPage(editorPage, moduleDefId, pageName, ModuleFolder + "/images/HtmlEditorManager_Standard_32x32.png", true);

                        foreach (var item in DesktopModuleController.GetDesktopModules(Null.NullInteger))
                        {
                            DesktopModuleInfo moduleInfo = item.Value;

                            if (moduleInfo.ModuleName == "DotNetNuke.HtmlEditorManager")
                            {
                                moduleInfo.Category = "Host";
                                DesktopModuleController.SaveDesktopModule(moduleInfo, false, false);
                            }
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                var xlc = new ExceptionLogController();
                xlc.AddLog(ex);

                return "Failed";
            }

            return "Success";
        }

        /// <summary>Gets the module definition identifier.</summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="moduleDefinitionName">Name of the module definition.</param>
        /// <returns>The Module Id for the HTML Editor Management module</returns>
        private int GetModuleDefinitionID(string moduleName, string moduleDefinitionName)
        {
            // get desktop module
            DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(moduleName, Null.NullInteger);
            if (desktopModule == null)
            {
                return -1;
            }

            // get module definition
            ModuleDefinitionInfo moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByDefinitionName(
                moduleDefinitionName,
                desktopModule.DesktopModuleID);
            if (moduleDefinition == null)
            {
                return -1;
            }

            return moduleDefinition.ModuleDefID;
        }
    } 
}