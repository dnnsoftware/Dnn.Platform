// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    public interface IFollowerEventHandlers
    {
        void FollowRequested(object sender, RelationshipEventArgs args);

        void UnfollowRequested(object sender, RelationshipEventArgs args);
    }
}
