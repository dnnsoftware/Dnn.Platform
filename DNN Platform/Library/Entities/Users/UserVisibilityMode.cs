// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    public enum UserVisibilityMode
    {
        /// <summary>Visible to all users.</summary>
        AllUsers = 0,

        /// <summary>Visible only to members of the site.</summary>
        MembersOnly = 1,

        /// <summary>Visible only to administrators of the site.</summary>
        AdminOnly = 2,

        /// <summary>Visible to the user's friends and groups.</summary>
        FriendsAndGroups = 3,
    }
}
