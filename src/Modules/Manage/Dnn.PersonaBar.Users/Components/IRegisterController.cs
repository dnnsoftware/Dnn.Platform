// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

using Dnn.PersonaBar.Users.Components.Contracts;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components
{
    internal interface IRegisterController
    {
        UserInfo Register(RegisterationDetails registerationDetails, out string message);
    }
}