// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users.Social
{
    public interface IFollowersController
    {
        void FollowUser(UserInfo targetUser);

        void FollowUser(UserInfo initiatingUser, UserInfo targetUser);

        void UnFollowUser(UserInfo targetUser);
    }
}
