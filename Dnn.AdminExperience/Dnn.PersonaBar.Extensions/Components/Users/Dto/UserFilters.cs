// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Dto
{
    public enum UserFilters
    {
        /// <summary>Authorized users.</summary>
        Authorized = 0,

        /// <summary>Unauthorized users.</summary>
        UnAuthorized = 1,

        /// <summary>Deleted users.</summary>
        Deleted = 2,

        /// <summary>Superusers.</summary>
        SuperUsers = 3,

        /// <summary>All registered users.</summary>
        RegisteredUsers = 4,

        /// <summary>Users who have agreed to the site's terms.</summary>
        HasAgreedToTerms = 5,

        /// <summary>Users who have not yet agreed to the site's terms.</summary>
        HasNotAgreedToTerms = 6,

        /// <summary>Users who have requested to be removed from the site.</summary>
        RequestedRemoval = 7,

        /// <summary>All users.</summary>
        All = 8,
    }
}
