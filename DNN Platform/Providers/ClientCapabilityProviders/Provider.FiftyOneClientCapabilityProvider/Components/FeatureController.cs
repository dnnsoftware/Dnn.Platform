#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System.Linq;

using DotNetNuke.Application;
using DotNetNuke.Providers.FiftyOneClientCapabilityProvider.Properties;
using DotNetNuke.Services.Mobile;

namespace DotNetNuke.Providers.FiftyOneClientCapabilityProvider.Components
{
    using System.Collections.Generic;
    using Common;
    using Common.Utilities;
    using Entities.Modules;
    using Entities.Modules.Definitions;
    using Entities.Tabs;
    using Services.Installer;
    using Services.Installer.Packages;
    using Services.Localization;
    using Services.Upgrade;
    using Constants = Constants;

    /// -----------------------------------------------------------------------------
    ///<summary>
    /// The FeatureController class for the modules.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// </history>
    /// -----------------------------------------------------------------------------

#if SEARCHABLE
    public class FeatureController : ModuleController, ISearchable, IUpgradeable
#else
    public class FeatureController : ModuleController, IUpgradeable
#endif
    {
        #region Constants

        private const string ResourceFileRelativePath = "~/DesktopModules/Admin/FiftyOneClientCapabilityProvider/App_LocalResources/SharedResources.resx";

        #endregion

        #region Interfaces

#if SEARCHABLE

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchItems implements the ISearchable Interface
        /// </summary>
        /// <param name="ModInfo">The ModuleInfo for the module to be Indexed</param>
        /// -----------------------------------------------------------------------------
        public DotNetNuke.Services.Search.SearchItemInfoCollection GetSearchItems(DotNetNuke.Entities.Modules.ModuleInfo ModInfo)
        {
            var items = new SearchItemInfoCollection();

            // Add the devices to the search index.
            foreach (var item in Factory.ActiveProvider.Devices)
            {
                string guid = String.Format("DeviceID={0}",
                    item.DeviceId);

                var device = new Device(item);

                items.Add(new SearchItemInfo(
                    device.HardwareCaption,
                    String.Format("{0} running {1} with browser {2}",
                        device.HardwareCaption,
                        device.SoftwareCaption,
                        device.BrowserCaption),
                    ModInfo.CreatedByUserID,
                    Factory.ActiveProvider.PublishedDate,
                    ModInfo.ModuleID,
                    device.DeviceID,
                    device.Content,
                    guid));
            }

            // Add the properties to the search.
            foreach (var property in Factory.ActiveProvider.Properties)
            {
                items.Add(new SearchItemInfo(
                    property.Name,
                    property.Description,
                    ModInfo.CreatedByUserID,
                    Factory.ActiveProvider.PublishedDate,
                    ModInfo.ModuleID,
                    property.Name,
                    property.Description));
                foreach (var value in property.Values)
                {
                    string name = String.Format("{0} - {1}", property.Name, value.Name);
                    items.Add(new SearchItemInfo(
                        name,
                        value.Description,
                        ModInfo.CreatedByUserID,
                        Factory.ActiveProvider.PublishedDate,
                        ModInfo.ModuleID,
                        name,
                        value.Description));
                }
            }

            return items;
        }

#endif

        /// <summary>
        /// Handles upgrading the module and adding the module to the hosts menu.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "06.01.05":
                    PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == Constants.PackageName);
                    IDictionary<int, TabInfo> moduleTabs = new TabController().GetTabsByPackageID(-1, package.PackageID, false);

                    if (moduleTabs.Count > 0)
                        return string.Empty;

                    AddClientResourceAdminHostPage();

                    RemoveWurflProvider();
                    break;
            }

            return Localization.GetString("SuccessMessage", ResourceFileRelativePath);
        }

        private void RemoveWurflProvider()
        {
            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name =="DotNetNuke.WURFLClientCapabilityProvider");
            if(package != null)
            {
                var installer = new Installer(package, Globals.ApplicationMapPath);
                installer.UnInstall(true);
            }

            UpdateRules();
        }

        private void UpdateRules()
        {
            var mapCapabilites = CreateMappedCapabilities();
            IRedirectionController controller = new RedirectionController();
            var redirections = controller.GetAllRedirections();
            foreach (var redirection in redirections.Where(redirection => redirection.MatchRules.Count > 0))
            {
                var deletedRules = new List<IMatchRule>();
                foreach (var rule in redirection.MatchRules)
                {
                    if (rule.Capability == "pointing_method")
                    {
                        switch (rule.Expression)
                        {
                            case "clickwheel":
                                rule.Capability = "HasClickWheel";
                                rule.Expression = "True";
                                break;
                            case "touchscreen":
                                rule.Capability = "HasTouchScreen";
                                rule.Expression = "True";
                                break;
                            default:
                                deletedRules.Add(rule);
                                break;
                        }
                    }
                    else
                    {
                        if (mapCapabilites.ContainsKey(rule.Capability))
                        {
                            rule.Capability = mapCapabilites[rule.Capability];
                            switch (rule.Expression)
                            {
                                case "true":
                                    rule.Expression = "True";
                                    break;
                                case "false":
                                    rule.Expression = "False";
                                    break;
                            }
                        }
                        else
                        {
                            deletedRules.Add(rule);
                        }
                    }
                    
                }

                //remove the deleted rules
                foreach (var deletedRule in deletedRules)
                {
                    controller.DeleteRule(redirection.PortalId, redirection.Id, deletedRule.Id);
                    redirection.MatchRules.Remove(deletedRule);
                }

                controller.Save(redirection);
            }

        }

        private IDictionary<string, string> CreateMappedCapabilities()
        {
            var mappingCapabilites = new Dictionary<string, string>();
            mappingCapabilites.Add("is_wireless_device", "IsMobile");
            mappingCapabilites.Add("resolution_width", "ScreenPixelsWidth");
            mappingCapabilites.Add("resolution_height", "ScreenPixelsHeight");
            if (DotNetNukeContext.Current.Application.Name != "DNNCORP.CE")
            {
                mappingCapabilites.Add("is_tablet", "IsTablet");
                mappingCapabilites.Add("device_os", "PlatformName");
                mappingCapabilites.Add("mobile_browser", "BrowserName");
                mappingCapabilites.Add("mobile_browser_version", "BrowserVersion");
                mappingCapabilites.Add("device_os_version", "PlatformVersion");
                mappingCapabilites.Add("brand_name", "HardwareVendor");
                mappingCapabilites.Add("cookie_support", "CookiesCapable");
                mappingCapabilites.Add("model_name", "HardwareModel");
                mappingCapabilites.Add("physical_screen_height", "ScreenMMHeight");
                mappingCapabilites.Add("physical_screen_width", "ScreenMMWidth");
            }
            return mappingCapabilites;
        }

        private static void AddClientResourceAdminHostPage()
        {
            DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(Constants.ModuleName, Null.NullInteger);
            ModuleDefinitionInfo moduleDefinition = desktopModule.ModuleDefinitions[Constants.ModuleDefinitionName];

            // Remove the page if it already exists to ensure the page can be added.
            // Handles cases where the page has been removed by the user.
            try
            {
                Upgrade.RemoveHostPage(Localization.GetString("PageName", ResourceFileRelativePath));
            }
            catch
            {
                // Do nothing.
            }

            TabInfo hostPage = Upgrade.AddHostPage(Localization.GetString("PageName", ResourceFileRelativePath),
                                                   Localization.GetString("PageDescription", ResourceFileRelativePath),
                                                   Constants.ConfigIconFileThumbNail,
                                                   Constants.ConfigIconFileLarge, true);

            Upgrade.AddModuleToPage(hostPage, moduleDefinition.ModuleDefID, Localization.GetString("ModuleTitle", ResourceFileRelativePath), Constants.ConfigIconFileLarge, true);
        }

        #endregion
    }
}

