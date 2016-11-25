#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Library.Common
{
    internal class Utilities
    {
        /// <summary>
        /// Returns a relative URL for the user profile image while removing that of the deleted and super users
        /// </summary>
        /// <param name="user">user info</param>
        /// <param name="width">width in pixel</param>
        /// <param name="height">height in pixel</param>
        /// <param name="showSuperUsers">true if want show super users user profile picture, false otherwise</param>
        /// <returns>relative user profile picture url</returns>
        /// <returns></returns>
        internal static string GetProfileAvatar(UserInfo user, int width = Constants.AvatarWidth, int height = Constants.AvatarHeight, bool showSuperUsers = true)
        {
            var userId = user != null && user.UserID > 0 && !user.IsDeleted && (showSuperUsers || !user.IsSuperUser) ? user.UserID : 0;
            return UserController.Instance.GetUserProfilePictureUrl(userId, width, height);
        }
    }
}
