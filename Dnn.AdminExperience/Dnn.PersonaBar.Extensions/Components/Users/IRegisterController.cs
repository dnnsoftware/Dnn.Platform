// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components
{
    using Dnn.PersonaBar.Users.Components.Contracts;
    using Dnn.PersonaBar.Users.Components.Dto;
    using DotNetNuke.Entities.Users;

    internal interface IRegisterController
    {
        UserBasicDto Register(RegisterationDetails registerationDetails);
    }
}
