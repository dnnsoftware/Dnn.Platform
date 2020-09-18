// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Dto
{
    public enum UserFilters
    {
        Authorized = 0,
        UnAuthorized = 1,
        Deleted = 2,
        SuperUsers = 3,
        RegisteredUsers = 4,
        HasAgreedToTerms = 5,
        HasNotAgreedToTerms = 6,
        RequestedRemoval = 7,
        All = 8
    }
}
