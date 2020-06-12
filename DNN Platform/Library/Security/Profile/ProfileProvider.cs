

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Security.Profile
{
    public abstract class ProfileProvider
    {
        public abstract bool CanEditProviderProperties { get; }

        // return the provider
        public static ProfileProvider Instance()
        {
            return ComponentFactory.GetComponent<ProfileProvider>();
        }

        public abstract void GetUserProfile(ref UserInfo user);

        public abstract void UpdateUserProfile(UserInfo user);
    }
}
