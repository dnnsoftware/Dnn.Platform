// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;

    public class Controller : IUpgradeable, IPortable
    {
        public static readonly Regex AscxText1Regex = new Regex(Regex.Escape(@"Namespace=""DNNDoneRight.DDRMenu"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex AscxText2Regex = new Regex(Regex.Escape(@"Namespace=""DNNGarden.TemplateEngine"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex AscxText3Regex = new Regex(Regex.Escape(@"Assembly=""DNNDoneRight.DDRMenu"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex AscxText4Regex = new Regex(Regex.Escape(@"Assembly=""DNNGarden.DDRMenu"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private const string ddrMenuModuleName = "DDRMenu";
        private const string ddrMenuMmoduleDefinitionName = "DDR Menu";

        public string UpgradeModule(string version)
        {
            UpdateWebConfig();

            TidyModuleDefinitions();

            CleanOldAssemblies();

            CheckSkinReferences();

            return "UpgradeModule completed OK";
        }

        public string ExportModule(int moduleId)
        {
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
            var moduleSettings = module.ModuleSettings;

            var settings = new Settings
            {
                MenuStyle = Convert.ToString(moduleSettings["MenuStyle"]),
                NodeXmlPath = Convert.ToString(moduleSettings["NodeXmlPath"]),
                NodeSelector = Convert.ToString(moduleSettings["NodeSelector"]),
                IncludeNodes = Convert.ToString(moduleSettings["IncludeNodes"]),
                ExcludeNodes = Convert.ToString(moduleSettings["ExcludeNodes"]),
                NodeManipulator = Convert.ToString(moduleSettings["NodeManipulator"]),
                IncludeContext = Convert.ToBoolean(moduleSettings["IncludeContext"]),
                IncludeHidden = Convert.ToBoolean(moduleSettings["IncludeHidden"]),
                ClientOptions = Settings.ClientOptionsFromSettingString(Convert.ToString(moduleSettings["ClientOptions"])),
                TemplateArguments =
                                Settings.TemplateArgumentsFromSettingString(Convert.ToString(moduleSettings["TemplateArguments"])),
            };
            return settings.ToXml();
        }

        public void ImportModule(int moduleId, string content, string version, int userId)
        {
            var settings = Settings.FromXml(content);

            ModuleController.Instance.UpdateModuleSetting(moduleId, "MenuStyle", settings.MenuStyle ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "NodeXmlPath", settings.NodeXmlPath ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "NodeSelector", settings.NodeSelector ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "IncludeNodes", settings.IncludeNodes ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "ExcludeNodes", settings.ExcludeNodes ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "NodeManipulator", settings.NodeManipulator ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "IncludeContext", settings.IncludeContext.ToString());
            ModuleController.Instance.UpdateModuleSetting(moduleId, "IncludeHidden", settings.IncludeHidden.ToString());
            ModuleController.Instance.UpdateModuleSetting(moduleId, "TemplateArguments", Settings.ToSettingString(settings.TemplateArguments));
            ModuleController.Instance.UpdateModuleSetting(moduleId, "ClientOptions", Settings.ToSettingString(settings.ClientOptions));
        }

        private static void UpdateWebConfig()
        {
            const string navName = "DDRMenuNavigationProvider";
            const string navType = "DotNetNuke.Web.DDRMenu.DDRMenuNavigationProvider, DotNetNuke.Web.DDRMenu";

            var server = HttpContext.Current.Server;
            var webConfig = server.MapPath("~/web.config");

            var configXml = new XmlDocument { XmlResolver = null };
            configXml.Load(webConfig);
            var navProviders = configXml.SelectSingleNode("/configuration/dotnetnuke/navigationControl/providers") as XmlElement;

            // ReSharper disable PossibleNullReferenceException
            var addProvider = navProviders.SelectSingleNode("add[@name='" + navName + "']") as XmlElement;

            // ReSharper restore PossibleNullReferenceException
            var needsUpdate = true;
            if (addProvider == null)
            {
                addProvider = configXml.CreateElement("add");
                addProvider.SetAttribute("name", navName);
                navProviders.AppendChild(addProvider);
            }
            else
            {
                needsUpdate = addProvider.GetAttribute("type") != navType;
            }

            if (needsUpdate)
            {
                addProvider.SetAttribute("type", navType);
                configXml.Save(webConfig);
            }
        }

        private static void TidyModuleDefinitions()
        {
            RemoveLegacyModuleDefinitions(ddrMenuModuleName, ddrMenuMmoduleDefinitionName);
            RemoveLegacyModuleDefinitions("DDRMenuAdmin", "N/A");
        }

        private static void RemoveLegacyModuleDefinitions(string moduleName, string currentModuleDefinitionName)
        {
            var mdc = new ModuleDefinitionController();

            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(moduleName, Null.NullInteger);
            if (desktopModule == null)
            {
                return;
            }

            var desktopModuleId = desktopModule.DesktopModuleID;
            var modDefs = ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId);

            var currentModDefId = 0;
            foreach (var modDefKeyPair in modDefs)
            {
                if (modDefKeyPair.Value.FriendlyName.Equals(currentModuleDefinitionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentModDefId = modDefKeyPair.Value.ModuleDefID;
                }
            }

            foreach (var modDefKeyPair in modDefs)
            {
                var oldModDefId = modDefKeyPair.Value.ModuleDefID;
                if (oldModDefId != currentModDefId)
                {
                    foreach (ModuleInfo mod in ModuleController.Instance.GetAllModules())
                    {
                        if (mod.ModuleDefID == oldModDefId)
                        {
                            mod.ModuleDefID = currentModDefId;
                            ModuleController.Instance.UpdateModule(mod);
                        }
                    }

                    mdc.DeleteModuleDefinition(oldModDefId);
                }
            }

            modDefs = ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId);
            if (modDefs.Count == 0)
            {
                new DesktopModuleController().DeleteDesktopModule(desktopModuleId);
            }
        }

        private static void CleanOldAssemblies()
        {
            var assembliesToRemove = new[] { "DNNDoneRight.DDRMenu.dll", "DNNGarden.DDRMenu.dll" };

            var server = HttpContext.Current.Server;
            var assemblyPath = server.MapPath("~/bin/");
            foreach (var assembly in assembliesToRemove)
            {
                File.Delete(Path.Combine(assemblyPath, assembly));
            }
        }

        private static void CheckSkinReferences()
        {
            var server = HttpContext.Current.Server;
            var portalsRoot = server.MapPath("~/Portals/");
            foreach (var portal in Directory.GetDirectories(portalsRoot))
            {
                foreach (var skinControl in Directory.GetFiles(portal, "*.ascx", SearchOption.AllDirectories))
                {
                    try
                    {
                        var ascxText = File.ReadAllText(skinControl);
                        var originalText = ascxText;
                        ascxText = AscxText1Regex.Replace(ascxText, @"Namespace=""DotNetNuke.Web.DDRMenu.TemplateEngine""");
                        ascxText = AscxText2Regex.Replace(ascxText, @"Namespace=""DotNetNuke.Web.DDRMenu.TemplateEngine""");
                        ascxText = AscxText3Regex.Replace(ascxText, @"Assembly=""DotNetNuke.Web.DDRMenu""");
                        ascxText = AscxText4Regex.Replace(ascxText, @"Assembly=""DotNetNuke.Web.DDRMenu""");

                        if (!ascxText.Equals(originalText))
                        {
                            File.WriteAllText(skinControl, ascxText);
                        }
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (SecurityException)
                    {
                    }
                }
            }
        }
    }
}
