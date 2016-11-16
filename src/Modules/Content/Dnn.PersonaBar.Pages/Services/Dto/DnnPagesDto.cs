using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.DTO.Tabs;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [JsonObject]
    public class DnnPagesDto
    {
        public bool HasMissingLanguages { get; set; }
        public bool ErrorExists { get; set; }
        public IList<LocaleInfoDto> Locales { get; }
        public IList<DnnPageDto> Pages { get; }
        public IList<DnnModulesDto> Modules { get; }

        public DnnPagesDto(IList<LocaleInfoDto> locales)
        {
            Locales = locales;
            Pages = new List<DnnPageDto>(); // one of each language
            Modules = new List<DnnModulesDto>(); // one for each module on the page
            foreach (var locale in locales)
            {
                Pages.Add(new DnnPageDto { CultureCode = locale.CultureCode });
            }
        }

        public DnnPageDto Page(string locale)
        {
            return Pages.Single(pa => pa.CultureCode == locale);
        }

        public DnnModulesDto Module(Guid uniqueId)
        {
            var m = Modules.SingleOrDefault(dm => dm.UniqueId == uniqueId);
            if (m == null)
            {
                m = new DnnModulesDto(Locales.Select(l => l.CultureCode)) { UniqueId = uniqueId };
                Modules.Add(m);
            }
            return m;

        }

        public bool Error1(int moduleId, Guid uniqueId, string cultureCode)
        {
            return Modules.Any(dm => dm.UniqueId != uniqueId &&
                                     dm.Modules.Any(mm => mm.ModuleId == moduleId && mm.CultureCode != cultureCode));
        }

        public void RemoveLocale(string cultureCode)
        {
            var locale = Locales.FirstOrDefault(l => l.CultureCode == cultureCode);
            if (locale != null) Locales.Remove(locale);
        }
    }
}