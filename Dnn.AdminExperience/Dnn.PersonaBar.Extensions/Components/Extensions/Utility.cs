// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;

    public static class Utility
    {
        public static IEnumerable<ListItemDto> GetAllLanguagesList()
        {
            var locales = LocaleController.Instance.GetLocales(Null.NullInteger).Values;
            return locales.Select(l => new ListItemDto { Id = l.LanguageId, Name = l.EnglishName });
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
