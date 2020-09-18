// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users.Social
{
    using System.Collections.Generic;

    public interface IRelationshipController
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete RelationshipType.
        /// </summary>
        /// <param name="relationshipType">RelationshipType.</param>
        /// -----------------------------------------------------------------------------
        void DeleteRelationshipType(RelationshipType relationshipType);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get list of All RelationshipTypes defined in system.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        IList<RelationshipType> GetAllRelationshipTypes();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get RelationshipType By RelationshipTypeId.
        /// </summary>
        /// <param name="relationshipTypeId">RelationshipTypeId.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        RelationshipType GetRelationshipType(int relationshipTypeId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save RelationshipType.
        /// </summary>
        /// <param name="relationshipType">RelationshipType object.</param>
        /// <remarks>
        /// If RelationshipTypeId is -1 (Null.NullIntger), then a new RelationshipType is created,
        /// else existing RelationshipType is updated.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        void SaveRelationshipType(RelationshipType relationshipType);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete Relationship.
        /// </summary>
        /// <param name="relationship">Relationship.</param>
        /// -----------------------------------------------------------------------------
        void DeleteRelationship(Relationship relationship);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get Relationship By RelationshipId.
        /// </summary>
        /// <param name="relationshipId">RelationshipId.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        Relationship GetRelationship(int relationshipId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get Relationships By UserId.
        /// </summary>
        /// <param name="userId">UserId.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        IList<Relationship> GetRelationshipsByUserId(int userId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get Relationships By PortalId.
        /// </summary>
        /// <param name="portalId">PortalId.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        IList<Relationship> GetRelationshipsByPortalId(int portalId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save Relationship.
        /// </summary>
        /// <param name="relationship">Relationship object.</param>
        /// <remarks>
        /// If RelationshipId is -1 (Null.NullIntger), then a new Relationship is created,
        /// else existing Relationship is updated.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        void SaveRelationship(Relationship relationship);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete UserRelationship.
        /// </summary>
        /// <param name="userRelationship">UserRelationship to delete.</param>
        /// -----------------------------------------------------------------------------
        void DeleteUserRelationship(UserRelationship userRelationship);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationship By UserRelationshipId.
        /// </summary>
        /// <param name="userRelationshipId">UserRelationshipId.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        UserRelationship GetUserRelationship(int userRelationshipId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationship by its members.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="relatedUser">Related User.</param>
        /// <param name="relationship">Relationship Object.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        UserRelationship GetUserRelationship(UserInfo user, UserInfo relatedUser, Relationship relationship);

        /// <summary>
        /// This method gets a Dictionary of all the relationships that a User belongs to and the members of thase relationships.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>A Dictionary of Lists of UserRelationship, keyed by the Relationship.</returns>
        IList<UserRelationship> GetUserRelationships(UserInfo user);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save UserRelationship.
        /// </summary>
        /// <param name="userRelationship">UserRelationship object.</param>
        /// <remarks>
        /// If UserRelationshipId is -1 (Null.NullIntger), then a new UserRelationship is created,
        /// else existing UserRelationship is updated.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        void SaveUserRelationship(UserRelationship userRelationship);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete UserRelationshipPreference.
        /// </summary>
        /// <param name="userRelationshipPreference">UserRelationshipPreference to delete.</param>
        /// -----------------------------------------------------------------------------
        void DeleteUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationshipPreference By RelationshipTypeId.
        /// </summary>
        /// <param name="preferenceId">PreferenceId.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        UserRelationshipPreference GetUserRelationshipPreference(int preferenceId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationshipPreference By UserId and RelationshipId.
        /// </summary>
        /// <param name="userId">UserId.</param>
        /// <param name="relationshipId">RelationshipId.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        UserRelationshipPreference GetUserRelationshipPreference(int userId, int relationshipId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save UserRelationshipPreference.
        /// </summary>
        /// <param name="userRelationshipPreference">UserRelationshipPreference object.</param>
        /// <remarks>
        /// If PreferenceId is -1 (Null.NullIntger), then a new UserRelationshipPreference is created,
        /// else existing UserRelationshipPreference is updated.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        void SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference);

        UserRelationship InitiateUserRelationship(UserInfo initiatingUser, UserInfo targetUser, Relationship relationship);

        void AcceptUserRelationship(int userRelationshipId);

        void RemoveUserRelationship(int userRelationshipId);

        UserRelationship GetFollowerRelationship(UserInfo targetUser);

        UserRelationship GetFollowerRelationship(UserInfo initiatingUser, UserInfo targetUser);

        UserRelationship GetFollowingRelationship(UserInfo targetUser);

        UserRelationship GetFollowingRelationship(UserInfo initiatingUser, UserInfo targetUser);

        UserRelationship GetFriendRelationship(UserInfo targetUser);

        UserRelationship GetFriendRelationship(UserInfo initiatingUser, UserInfo targetUser);

        void CreateDefaultRelationshipsForPortal(int portalId);

        Relationship GetFriendsRelationshipByPortal(int portalId);

        Relationship GetFollowersRelationshipByPortal(int portalId);
    }
}
