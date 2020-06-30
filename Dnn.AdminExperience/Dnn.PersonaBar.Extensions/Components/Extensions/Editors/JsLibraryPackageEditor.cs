// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    using System;
    using System.Linq;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Packages;

    public class JsLibraryPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JsLibraryPackageEditor));

        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var usedBy = PackageController.Instance.GetPackageDependencies(d =>
                            d.PackageName.Equals(package.Name, StringComparison.OrdinalIgnoreCase) &&
                            d.Version <= package.Version).Select(d => d.PackageId);

            var usedByPackages = from p in PackageController.Instance.GetExtensionPackages(portalId)
                                 where usedBy.Contains(p.PackageID)
                                 select new UsedByPackage { Id = p.PackageID, Name = p.Name, Version = p.Version.ToString() };

            var library = JavaScriptLibraryController.Instance.GetLibrary(l => l.PackageID == package.PackageID);
            var detail = new JsLibraryPackageDetailDto(portalId, package)
            {
                Name = library.LibraryName,
                Version = library.Version.ToString(),
                ObjectName = library.ObjectName,
                DefaultCdn = library.CDNPath,
                FileName = library.FileName,
                Location = library.PreferredScriptLocation.ToString(),
                CustomCdn = HostController.Instance.GetString("CustomCDN_" + library.LibraryName),
                Dependencies = package.Dependencies.Select(d => new UsedByPackage
                {
                    Id = d.PackageId,
                    Name = d.PackageName,
                    Version = d.Version.ToString()
                }),
                UsedBy = usedByPackages
            };

            return detail;
        }

        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                string value;
                if (packageSettings.EditorActions.TryGetValue("customCdn", out value)
                    && !string.IsNullOrEmpty(value))
                {
                    var library = JavaScriptLibraryController.Instance.GetLibrary(l => l.PackageID == packageSettings.PackageId);
                    HostController.Instance.Update("CustomCDN_" + library.LibraryName, value);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
