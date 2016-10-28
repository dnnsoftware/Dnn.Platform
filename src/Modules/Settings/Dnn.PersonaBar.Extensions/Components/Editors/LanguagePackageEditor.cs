using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public class LanguagePackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguagePackageEditor));

        #region IPackageEditor Implementation

        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var locales = LocaleController.Instance.GetLocales(Null.NullInteger).Values;
            var languagePack = LanguagePackController.GetLanguagePackByPackage(package.PackageID);
            var languagesTab = TabController.GetTabByTabPath(portalId, "//Admin//Languages", Null.NullString);

            var detail = new LanguagePackageDetailDto(portalId, package)
            {
                Locales = locales.Select(l => new ListItemDto {Id = l.LanguageId, Name = l.EnglishName}),
                LanguageId = languagePack.LanguageID,
                EditUrlFormat = Globals.NavigateURL(languagesTab, "", "Locale={0}")
            };

            if (languagePack.PackageType == LanguagePackType.Extension)
            {
                //Get all the packages but only bind to combo if not a language package
                var packages = new List<ListItemDto>();
                foreach (var p in PackageController.Instance.GetExtensionPackages(Null.NullInteger))
                {
                    if (p.PackageType != "CoreLanguagePack" && p.PackageType != "ExtensionLanguagePack")
                    {
                        packages.Add(new ListItemDto {Id = p.PackageID, Name = p.Name});
                    }
                }

                detail.Packages = packages;
            }

            return detail;
        }

        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        #endregion
    }
}