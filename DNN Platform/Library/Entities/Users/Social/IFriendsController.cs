// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Users.Social
{
    public interface IFriendsController
    {
        void AcceptFriend(UserInfo targetUser);

        void AddFriend(UserInfo targetUser);
        void AddFriend(UserInfo initiatingUser, UserInfo targetUser);

        void DeleteFriend(UserInfo targetUser);
        void DeleteFriend(UserInfo initiatingUser, UserInfo targetUser);

    }
}
