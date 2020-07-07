// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Friends
{
    using DotNetNuke.Entities.Users;

    public interface IFriendshipEventHandlers
    {
        void FriendshipRequested(object sender, RelationshipEventArgs args);

        void FriendshipAccepted(object sender, RelationshipEventArgs args);

        void FriendshipDeleted(object sender, RelationshipEventArgs args);
    }
}
