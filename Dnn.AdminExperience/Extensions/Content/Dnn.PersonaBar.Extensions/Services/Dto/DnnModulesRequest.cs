using System;
using System.Collections.Generic;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    public class DnnModulesRequest
    {
        public Guid UniqueId { get; set; }
        public List<DnnModuleDto> Modules { get; set; }
    }
}