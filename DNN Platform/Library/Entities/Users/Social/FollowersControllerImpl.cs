#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Entities.Friends;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Social.Notifications;

namespace DotNetNuke.Entities.Users.Social.Internal
{
    internal class FollowersControllerImpl : IFollowersController
    {
        internal const string FollowerRequest = "FollowerRequest";
        internal const string FollowBackRequest = "FollowBackRequest";

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FollowUser - Current User initiates a Follow Request to the Target User
        /// </summary>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship object</returns>
        /// <remarks>If the Follow Relationship is setup for auto-acceptance (default) at the Portal level, the UserRelationship
        /// status is set as Accepted, otherwise it is set as Initiated.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void FollowUser(UserInfo targetUser)
        {
            FollowUser(UserController.Instance.GetCurrentUserInfo(), targetUser);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FollowUser - Initiating User initiates a Follow Request to the Target User
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship object</returns>
        /// <remarks>If the Follow Relationship is setup for auto-acceptance (default) at the Portal level, the UserRelationship
        /// status is set as Accepted, otherwise it is set as Initiated.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void FollowUser(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("user1", initiatingUser);

            var relationship = RelationshipController.Instance.InitiateUserRelationship(initiatingUser, targetUser,
                RelationshipController.Instance.GetFollowersRelationshipByPortal(initiatingUser.PortalID));

            AddFollowerRequestNotification(initiatingUser, targetUser);

            EventManager.Instance.OnFollowRequested(new RelationshipEventArgs(relationship, initiatingUser.PortalID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UnFollowUser - Current User initiates an UnFollow Request to the Target User
        /// </summary>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// -----------------------------------------------------------------------------
        public void UnFollowUser(UserInfo targetUser)
        {
            var initiatingUser = UserController.Instance.GetCurrentUserInfo();
            var followRelationship = RelationshipController.Instance.GetFollowerRelationship(initiatingUser, targetUser);

            RelationshipController.Instance.DeleteUserRelationship(followRelationship);

            EventManager.Instance.OnUnfollowRequested(new RelationshipEventArgs(followRelationship, initiatingUser.PortalID));
        }

        private static void AddFollowerRequestNotification(UserInfo initiatingUser, UserInfo targetUser)
        {
            var notificationType = NotificationsController.Instance.GetNotificationType(IsFollowing(targetUser, initiatingUser) ? FollowerRequest : FollowBackRequest);
            var subject = string.Format(Localization.GetString("AddFollowerRequestSubject", Localization.GlobalResourceFile),
                              initiatingUser.DisplayName);

            var body = string.Format(Localization.GetString("AddFollowerRequestBody", Localization.GlobalResourceFile),
                              initiatingUser.DisplayName);

            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = true,
                Context = initiatingUser.UserID.ToString(CultureInfo.InvariantCulture),
                SenderUserID = initiatingUser.UserID
            };

            NotificationsController.Instance.SendNotification(notification, initiatingUser.PortalID, null, new List<UserInfo> { targetUser });
        }

        private static bool IsFollowing(UserInfo user1, UserInfo user2)
        {
            var userRelationship = RelationshipController.Instance.GetFollowerRelationship(user1, user2);

            return (userRelationship != null && userRelationship.Status == RelationshipStatus.Accepted);
        }
    }
}