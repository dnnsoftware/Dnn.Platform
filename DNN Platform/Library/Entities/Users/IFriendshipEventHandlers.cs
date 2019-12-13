// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Entities.Friends
{
    public interface IFriendshipEventHandlers
    {
        void FriendshipRequested(object sender, RelationshipEventArgs args);
        void FriendshipAccepted(object sender, RelationshipEventArgs args);
        void FriendshipDeleted(object sender, RelationshipEventArgs args);
    }
}
