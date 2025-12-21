// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Profile;

    /// <summary>this class handles profile changes.</summary>
    [Export(typeof(IProfileEventHandlers))]
    [ExportMetadata("MessageType", "ProfileEventHandler")]
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class ProfileEventHandler : IProfileEventHandlers
    {
        /// <summary>This method add the updated user id into cache to clear image from disk before returning to UI.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="profileArgs">The event arguments.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void ProfileUpdated(object sender, ProfileEventArgs profileArgs)
        {
            if (profileArgs?.User == null || profileArgs.OldProfile == null)
            {
                return;
            }

            // extract old and new user profile from args and clear both client and server caching
            var user = profileArgs.User;
            var newProfile = user.Profile;
            var oldProfile = profileArgs.OldProfile;
            var newPhotoVisibilityMode = newProfile.GetProperty(Entities.Users.UserProfile.USERPROFILE_Photo)?.ProfileVisibility.VisibilityMode;
            var oldPhotoVisibilityMode = oldProfile.GetProperty(Entities.Users.UserProfile.USERPROFILE_Photo)?.ProfileVisibility.VisibilityMode;
            if (newProfile.Photo != oldProfile.Photo || newPhotoVisibilityMode != oldPhotoVisibilityMode)
            {
                var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.UserIdListToClearDiskImageCacheKey, user.PortalID);
                Dictionary<int, DateTime> userIds;
                if ((userIds = DataCache.GetCache<Dictionary<int, DateTime>>(cacheKey)) == null)
                {
                    userIds = new Dictionary<int, DateTime>();
                }

                // Add the userid to the clear cache list, if not already in the list.
                userIds.Remove(user.UserID);

                userIds.Add(user.UserID, DateTime.UtcNow);
                DataCache.SetCache(cacheKey, userIds);
            }
        }
    }
}
