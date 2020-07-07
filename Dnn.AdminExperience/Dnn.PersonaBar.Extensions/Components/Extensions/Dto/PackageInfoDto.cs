// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Components.Controllers.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    [JsonObject]
    public class PackageInfoDto
    {
        public PackageInfoDto()
        {
        }

        public PackageInfoDto(int portalId, PackageInfo package)
        {
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();

            this.PackageType = package.PackageType;
            this.FriendlyName = package.FriendlyName;
            this.Name = package.Name;
            this.PackageId = package.PackageID;
            this.Description = package.Description;
            this.IsInUse = ExtensionsController.IsPackageInUse(package, portalId);
            this.Version = package.Version.ToString(3);
            this.UpgradeUrl = ExtensionsController.UpgradeRedirect(package.Version, package.PackageType, package.Name);
            this.UpgradeIndicator = ExtensionsController.UpgradeIndicator(package.Version, package.PackageType, package.Name);
            this.PackageIcon = ExtensionsController.GetPackageIcon(package);
            this.License = package.License;
            this.ReleaseNotes = package.ReleaseNotes;
            this.Owner = package.Owner;
            this.Organization = package.Organization;
            this.Url = package.Url;
            this.Email = package.Email;
            this.CanDelete = !package.IsSystemPackage &&
                             package.PackageID > 0 &&
                             PackageController.CanDeletePackage(package, PortalSettings.Current);

            var authService = AuthenticationController.GetAuthenticationServiceByPackageID(this.PackageId);
            this.ReadOnly = authService != null && authService.AuthenticationType == Constants.DnnAuthTypeName;

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var tabId = portalSettings.ActiveTab.TabID;
            this.SiteSettingsLink = this.NavigationManager.NavigateURL(tabId, "EditExtension",
                new[]
                {
                    $"packageid={this.PackageId}",
                    "Display=editor",
                    "popUp=true",
                });
        }

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

        public PackageInfo ToPackageInfo()
        {
            System.Version ver;
            System.Version.TryParse(this.Version, out ver);

            return new PackageInfo
            {
                PackageType = this.PackageType,
                FriendlyName = this.FriendlyName,
                Name = this.Name,
                PackageID = this.PackageId,
                Description = this.Description,
                Version = ver,
                License = this.License,
                ReleaseNotes = this.ReleaseNotes,
                Owner = this.Owner,
                Organization = this.Organization,
                Url = this.Url,
                Email = this.Email,
                IconFile = this.PackageIcon,
            };
        }
    }
}
