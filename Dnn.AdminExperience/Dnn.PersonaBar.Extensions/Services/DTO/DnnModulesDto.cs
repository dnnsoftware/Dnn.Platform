// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [JsonObject]
    public class DnnModulesDto
    {
        public Guid UniqueId { get; set; }
        public List<DnnModuleDto> Modules { get; }

        public DnnModulesDto(IEnumerable<string> locales)
        {
            Modules = new List<DnnModuleDto>(); // one module for each language
            foreach (var locale in locales)
            {
                Modules.Add(new DnnModuleDto {CultureCode = locale});
            }
        }

        public DnnModuleDto Module(string locale)
        {
            return Modules.Single(mo => mo.CultureCode == locale);
        }
    }
}
