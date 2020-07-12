// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.DTO.Tabs;
    using Newtonsoft.Json;

    [JsonObject]
    public class DnnPagesDto
    {
        public DnnPagesDto(IList<LocaleInfoDto> locales)
        {
            this.Locales = locales;
            this.Pages = new List<DnnPageDto>(); // one of each language
            this.Modules = new List<DnnModulesDto>(); // one for each module on the page
            foreach (var locale in locales)
            {
                this.Pages.Add(new DnnPageDto { CultureCode = locale.CultureCode });
            }
        }

        public IList<LocaleInfoDto> Locales { get; }
        public IList<DnnPageDto> Pages { get; }
        public IList<DnnModulesDto> Modules { get; }

        public bool HasMissingLanguages { get; set; }
        public bool ErrorExists { get; set; }

        public DnnPageDto Page(string locale)
        {
            return this.Pages.Single(pa => pa.CultureCode == locale);
        }

        public DnnModulesDto Module(Guid uniqueId)
        {
            var m = this.Modules.SingleOrDefault(dm => dm.UniqueId == uniqueId);
            if (m == null)
            {
                m = new DnnModulesDto(this.Locales.Select(l => l.CultureCode)) { UniqueId = uniqueId };
                this.Modules.Add(m);
            }
            return m;

        }

        public bool Error1(int moduleId, Guid uniqueId, string cultureCode)
        {
            return this.Modules.Any(dm => dm.UniqueId != uniqueId &&
                                     dm.Modules.Any(mm => mm.ModuleId == moduleId && mm.CultureCode != cultureCode));
        }

        public void RemoveLocale(string cultureCode)
        {
            var locale = this.Locales.FirstOrDefault(l => l.CultureCode == cultureCode);
            if (locale != null) this.Locales.Remove(locale);
        }
    }
}
