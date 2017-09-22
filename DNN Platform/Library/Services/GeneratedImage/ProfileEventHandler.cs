using System;
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.Entities.Profile;
using System.ComponentModel.Composition;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Services.GeneratedImage
{
    /// <summary>
    /// this class handles profile changes
    /// </summary>
    [Export(typeof(IProfileEventHandlers))]
    [ExportMetadata("MessageType", "ProfileEventHandler")]
    public class ProfileEventHandler : IProfileEventHandlers
    {
        /// <summary>
        /// This method add the updated user id into cache to clear image from disk before returning to UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="profileArgs"></param>
        public void ProfileUpdated(object sender, ProfileEventArgs profileArgs)
        {
            if (profileArgs?.User == null || profileArgs.OldProfile == null) return;
            //extract old and new user profile from args and clear both client and server caching
            var user = profileArgs.User;
            var oldProfile = profileArgs.OldProfile;
            if (user.Profile.Photo != oldProfile.Photo || user.Profile.GetProperty("Photo").ProfileVisibility.VisibilityMode !=
                oldProfile.ProfileProperties["Photo"].ProfileVisibility.VisibilityMode)
            {
                var cacheKey = string.Format(Constants.UserIdListToClearDiskImageCacheKey, user.PortalID);
                Dictionary<int, DateTime> userIds;
                if ((userIds = DataCache.GetCache<Dictionary<int, DateTime>>(cacheKey)) == null)
                    userIds = new Dictionary<int, DateTime>();
                //Add the userid to the clear cache list, if not already in the list.
                if (userIds.ContainsKey(user.UserID)) userIds.Remove(user.UserID);
                userIds.Add(user.UserID, DateTime.UtcNow);
                DataCache.SetCache(cacheKey, userIds);
            }
        }
    }
}
