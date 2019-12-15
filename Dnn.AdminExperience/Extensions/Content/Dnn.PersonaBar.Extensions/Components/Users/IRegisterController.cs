// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components
{
    internal interface IRegisterController
    {
        UserBasicDto Register(RegisterationDetails registerationDetails);
    }
}
