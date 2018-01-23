#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Providers.AspNetClientCapabilityProvider.Properties;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mobile;

namespace DotNetNuke.Providers.AspNetClientCapabilityProvider.Components
{

    /// -----------------------------------------------------------------------------
    ///<summary>
    /// The FeatureController class for the modules.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// </history>
    /// -----------------------------------------------------------------------------

    public class FeatureController : IUpgradeable
    {
        #region Constants

        private const string ResourceFileRelativePath = "~/Providers/ClientCapabilityProviders/AspNetClientCapabilityProvider/App_LocalResources/SharedResources.resx";

        #endregion

        #region Interfaces

        /// <summary>
        /// Handles upgrading the module and adding the module to the hosts menu.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "08.00.00":
                    var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == Constants.PackageName);
                    var moduleTabs = TabController.Instance.GetTabsByPackageID(-1, package.PackageID, false);

                    if (moduleTabs.Count > 0)
                        return string.Empty;

                    RemoveWurflProvider();
                    break;
            }

            return Localization.GetString("SuccessMessage", ResourceFileRelativePath);
        }

        private void RemoveWurflProvider()
        {
            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.WURFLClientCapabilityProvider");
            if (package != null)
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

        private static IDictionary<string, string> CreateMappedCapabilities()
        {
            var mappingCapabilites = new Dictionary<string, string>
            {
                {"is_wireless_device", "IsMobile"},
                {"resolution_width", "ScreenPixelsWidth"},
                {"resolution_height", "ScreenPixelsHeight"}
            };

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

        #endregion
    }
}