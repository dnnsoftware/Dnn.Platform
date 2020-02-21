// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Users
{
    public interface IFollowerEventHandlers
    {
        void FollowRequested(object sender, RelationshipEventArgs args);
        void UnfollowRequested(object sender, RelationshipEventArgs args);
    }
}
