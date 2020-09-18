// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    [JsonObject]
    public class DnnModulesDto
    {
        public DnnModulesDto(IEnumerable<string> locales)
        {
            this.Modules = new List<DnnModuleDto>(); // one module for each language
            foreach (var locale in locales)
            {
                this.Modules.Add(new DnnModuleDto { CultureCode = locale });
            }
        }

        public List<DnnModuleDto> Modules { get; }

        public Guid UniqueId { get; set; }

        public DnnModuleDto Module(string locale)
        {
            return this.Modules.Single(mo => mo.CultureCode == locale);
        }
    }
}
