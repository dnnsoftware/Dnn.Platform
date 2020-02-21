// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
