// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Users.Social
{
    public interface IFollowersController
    {
        void FollowUser(UserInfo targetUser);
        void FollowUser(UserInfo initiatingUser, UserInfo targetUser);

        void UnFollowUser(UserInfo targetUser);
    }
}
