// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging
{
    using DotNetNuke.Entities.Users;

    /// <summary>
    /// This class is responsible to manage the Messaging User Preference.
    /// </summary>
    public interface IUserPreferencesController
    {
        /// <summary>
        /// Set the User Messaging Preference.
        /// </summary>
        /// <param name="userPreference">User Preference.</param>
        void SetUserPreference(UserPreference userPreference);

        /// <summary>
        /// Get the User Messaging Preference.
        /// </summary>
        /// <param name="userinfo">User info.</param>
        /// <returns>User Messaging Preference.</returns>
        UserPreference GetUserPreference(UserInfo userinfo);
    }
}
