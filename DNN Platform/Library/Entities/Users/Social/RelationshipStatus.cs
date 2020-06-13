// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Enum:      RelationshipStatus
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The RelationshipStatus enum describes various UserRelationship statuses. E.g. Accepted, Pending.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public enum RelationshipStatus
    {
        /// <summary>
        /// Relationship Request is not present (lack of any other status)
        /// </summary>
        None = 0,

        /// <summary>
        /// Relationship Request is Initiated. E.g. User 1 sent a friend request to User 2.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Relationship Request is Accepted. E.g. User 2 has accepted User 1's friend request.
        /// </summary>
        Accepted = 2,
    }
}
