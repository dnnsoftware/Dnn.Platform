// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
