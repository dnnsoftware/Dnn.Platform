// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.DTO.Tabs;

    public class DnnPagesRequest
    {
        public bool HasMissingLanguages { get; set; }
        public bool ErrorExists { get; set; }
        public IList<LocaleInfoDto> Locales { get; set; }
        public IList<DnnPageDto> Pages { get; set; }
        public IList<DnnModulesRequest> Modules { get; set; }
    }
}
