using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using Dnn.PersonaBar.Library.Helper;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.UI;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public class JsLibraryPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JsLibraryPackageEditor));

        #region IPackageEditor Implementation

        public PackageDetailDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var library = JavaScriptLibraryController.Instance.GetLibrary(l => l.PackageID == package.PackageID);
            var detail = new CommonPackageDetailDto(portalId, package);

            detail.Settings.Add("name", library.LibraryName);
            detail.Settings.Add("version", library.Version.ToString());
            detail.Settings.Add("objectName", library.ObjectName);
            detail.Settings.Add("defaultCdn", library.CDNPath);
            detail.Settings.Add("fileName", library.FileName);
            detail.Settings.Add("location", library.PreferredScriptLocation.ToString());
            detail.Settings.Add("customCdn", HostController.Instance.GetString("CustomCDN_" + library.LibraryName));
            detail.Settings.Add("dependencies", package.Dependencies.Select(d => new ListItemDto {Id = d.PackageId, Name = d.PackageName}));

            var usedBy = PackageController.Instance.GetPackageDependencies(d => 
                            d.PackageName == package.Name && d.Version <= package.Version).Select(d => d.PackageId);

            var usedByPackages = from p in PackageController.Instance.GetExtensionPackages(portalId)
                                 where usedBy.Contains(p.PackageID)
                                 select new  { id = p.PackageID, name = p.Name, version = p.Version.ToString() };
            detail.Settings.Add("usedBy", usedByPackages);
            
            return detail;
        }

        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var library = JavaScriptLibraryController.Instance.GetLibrary(l => l.PackageID == packageSettings.PackageId);

                if(packageSettings.EditorActions.ContainsKey("customCdn")
                    && !string.IsNullOrEmpty(packageSettings.EditorActions["customCdn"]))
                {
                    HostController.Instance.Update("CustomCDN_" + library.LibraryName, packageSettings.EditorActions["customCdn"]);
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

        #endregion

        #region Private Methods

        private string GetSettingUrl(int portalId, int authSystemPackageId)
        {
            var module = ModuleController.Instance.GetModulesByDefinition(portalId, "Extensions")
                .Cast<ModuleInfo>().FirstOrDefault();
            if (module == null)
            {
                return string.Empty;
            }

            var tabId = TabController.Instance.GetTabsByModuleID(module.ModuleID).Keys.FirstOrDefault();
            if (tabId <= 0)
            {
                return string.Empty;
            }
            //ctl/Edit/mid/345/packageid/52
            return Globals.NavigateURL(tabId, PortalSettings.Current, "Edit", 
                                            "mid=" + module.ModuleID, 
                                            "packageid=" + authSystemPackageId,
                                            "popUp=true",
                                            "mode=settings");
        }

        #endregion
    }
}