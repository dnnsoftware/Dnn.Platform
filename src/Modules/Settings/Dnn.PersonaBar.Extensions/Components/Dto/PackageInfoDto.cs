#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [DataContract]
    public class PackageInfoDto
    {
        [DataMember(Name = "packageId")]
        public int PackageId { get; set; }

        [DataMember(Name = "packageType")]
        public string PackageType { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "friendlyName")]
        public string FriendlyName { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "inUse")]
        public string IsInUse { get; set; }

        [DataMember(Name = "upgradeUrl")]
        public string UpgradeUrl { get; set; }

        [DataMember(Name = "packageIcon")]
        public string PackageIcon { get; set; }

        [DataMember(Name = "license")]
        public string License { get; set; }

        [DataMember(Name = "releaseNotes")]
        public string ReleaseNotes { get; set; }

        [DataMember(Name = "owner")]
        public string Owner { get; set; }

        [DataMember(Name = "organization")]
        public string Organization { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "canDelete")]
        public bool CanDelete { get; set; }

        [JsonProperty("readOnly")]
        public bool ReadOnly { get; set; }

        public PackageInfoDto()
        {
            
        }

        public PackageInfoDto(int portalId, PackageInfo package)
        {
            PackageType = package.PackageType;
            FriendlyName = package.FriendlyName;
            Name = package.Name;
            PackageId = package.PackageID;
            Description = package.Description;
            IsInUse = ExtensionsController.IsPackageInUse(package, portalId);
            Version = package.Version.ToString(3);
            UpgradeUrl = ExtensionsController.UpgradeRedirect(package.Version, package.PackageType, package.Name);
            PackageIcon = ExtensionsController.GetPackageIcon(package);
            License = package.License;
            ReleaseNotes = package.ReleaseNotes;
            Owner = package.Owner;
            Organization = package.Organization;
            Url = package.Url;
            Email = package.Email;
            CanDelete = !package.IsSystemPackage && PackageController.CanDeletePackage(package, PortalSettings.Current);

            var authService = AuthenticationController.GetAuthenticationServiceByPackageID(PackageId);
            ReadOnly = authService != null &&  authService.AuthenticationType == Constants.DnnAuthTypeName;
        }

        public PackageInfo ToPackageInfo()
        {
            return new PackageInfo
            {
                PackageType = PackageType,
                FriendlyName = FriendlyName,
                Name = Name,
                PackageID = PackageId,
                Description = Description,
                Version = new Version(Version),
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