using System.Collections.Generic;
using Dnn.PersonaBar.Library.DTO.Tabs;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    public class DnnPagesRequest
    {
        public bool HasMissingLanguages { get; set; }
        public bool ErrorExists { get; set; }
        public IList<LocaleInfoDto> Locales { get; set; }
        public IList<DnnPageDto> Pages { get; set; }
        public IList<DnnModulesRequest> Modules { get; set; }
    }
}