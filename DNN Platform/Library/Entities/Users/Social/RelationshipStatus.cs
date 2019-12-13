#region Usings

using System;

#endregion

namespace DotNetNuke.Entities.Users.Social
{
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
