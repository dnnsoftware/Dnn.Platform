// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Extensions.Components.Dto;

namespace Dnn.PersonaBar.Extensions.Components
{
    public interface ICreateModuleController
    {
        int CreateModule(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage);
    }
}
