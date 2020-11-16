// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
