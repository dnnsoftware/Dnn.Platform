// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Profile
{
    using System;

    using DotNetNuke.Entities.Users;

    public class ProfileEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the user whom's profile has been changed. This includes the Profile property with the updated profile.
        /// </summary>
        public UserInfo User { get; set; }

        /// <summary>
        /// Gets or sets the user's profile, as it was before the change.
        /// </summary>
        public UserProfile OldProfile { get; set; }
    }
}
