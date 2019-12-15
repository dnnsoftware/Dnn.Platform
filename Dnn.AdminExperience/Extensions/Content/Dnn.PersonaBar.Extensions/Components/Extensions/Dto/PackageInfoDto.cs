// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Components.Controllers.Models;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class PackageInfoDto
    {
        [JsonProperty("packageId")]
        public int PackageId { get; set; }

        [JsonProperty("packageType")]
        public string PackageType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("inUse")]
        public string IsInUse { get; set; }

        [JsonProperty("upgradeUrl")]
        public string UpgradeUrl { get; set; }

        [JsonProperty("upgradeIndicator")]
        public string UpgradeIndicator { get; set; }

        [JsonProperty("packageIcon")]
        public string PackageIcon { get; set; }

        [JsonProperty("license")]
        public string License { get; set; }

        [JsonProperty("releaseNotes")]
        public string ReleaseNotes { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("canDelete")]
        public bool CanDelete { get; set; }

        [JsonProperty("readOnly")]
        public bool ReadOnly { get; set; }

        [JsonProperty("siteSettingsLink", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SiteSettingsLink { get; set; }

        protected INavigationManager NavigationManager { get; }

        public PackageInfoDto()
        {

        }

        public PackageInfoDto(int portalId, PackageInfo package)
        {
            NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();

            PackageType = package.PackageType;
            FriendlyName = package.FriendlyName;
            Name = package.Name;
            PackageId = package.PackageID;
            Description = package.Description;
            IsInUse = ExtensionsController.IsPackageInUse(package, portalId);
            Version = package.Version.ToString(3);
            UpgradeUrl = ExtensionsController.UpgradeRedirect(package.Version, package.PackageType, package.Name);
            UpgradeIndicator = ExtensionsController.UpgradeIndicator(package.Version, package.PackageType, package.Name);
            PackageIcon = ExtensionsController.GetPackageIcon(package);
            License = package.License;
            ReleaseNotes = package.ReleaseNotes;
            Owner = package.Owner;
            Organization = package.Organization;
            Url = package.Url;
            Email = package.Email;
            CanDelete = !package.IsSystemPackage &&
                package.PackageID > 0 &&
                PackageController.CanDeletePackage(package, PortalSettings.Current);

            var authService = AuthenticationController.GetAuthenticationServiceByPackageID(PackageId);
            ReadOnly = authService != null && authService.AuthenticationType == Constants.DnnAuthTypeName;

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var tabId = portalSettings.ActiveTab.TabID;
            SiteSettingsLink = NavigationManager.NavigateURL(tabId, "EditExtension",
                    new[]
                    {
                        $"packageid={PackageId}",
                        "Display=editor",
                        "popUp=true",
                    });
        }

        public PackageInfo ToPackageInfo()
        {
            System.Version ver;
            System.Version.TryParse(Version, out ver);

            return new PackageInfo
            {
                PackageType = PackageType,
                FriendlyName = FriendlyName,
                Name = Name,
                PackageID = PackageId,
                Description = Description,
                Version = ver,
                License = License,
                ReleaseNotes = ReleaseNotes,
                Owner = Owner,
                Organization = Organization,
                Url = Url,
                Email = Email,
                IconFile = PackageIcon,
            };
        }
    }
}
