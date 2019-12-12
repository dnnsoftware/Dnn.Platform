using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Extensions.Components
{
    public static class Utility
    {

        public static IEnumerable<ListItemDto> GetAllLanguagesList()
        {
            var locales = LocaleController.Instance.GetLocales(Null.NullInteger).Values;
            return locales.Select(l => new ListItemDto {Id = l.LanguageId, Name = l.EnglishName});
        }

        public static IEnumerable<ListItemDto> GetAllPackagesListExceptLangPacks()
        {
            var packages = new List<ListItemDto>();
            foreach (var p in PackageController.Instance.GetExtensionPackages(Null.NullInteger))
            {
                if (p.PackageType != "CoreLanguagePack" && p.PackageType != "ExtensionLanguagePack")
                {
                    packages.Add(new ListItemDto { Id = p.PackageID, Name = p.Name });
                }
            }
            return packages.OrderBy(p => p.Name);
        }

    }
}