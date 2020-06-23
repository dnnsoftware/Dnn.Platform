// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components
{
    using Dnn.PersonaBar.Extensions.Components.Dto;

    public interface ICreateModuleController
    {
        int CreateModule(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage);
    }
}
