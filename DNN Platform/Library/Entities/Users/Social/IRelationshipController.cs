#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System.Collections.Generic;

namespace DotNetNuke.Entities.Users.Social
{
    public interface IRelationshipController
    {
        #region RelationshipType

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete RelationshipType
        /// </summary>
        /// <param name="relationshipType">RelationshipType</param>        
        /// -----------------------------------------------------------------------------
        void DeleteRelationshipType(RelationshipType relationshipType);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get list of All RelationshipTypes defined in system
        /// </summary>        
        /// -----------------------------------------------------------------------------
        IList<RelationshipType> GetAllRelationshipTypes();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get RelationshipType By RelationshipTypeId
        /// </summary>        
        /// <param name="relationshipTypeId">RelationshipTypeId</param>        
        /// -----------------------------------------------------------------------------
        RelationshipType GetRelationshipType(int relationshipTypeId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save RelationshipType
        /// </summary>
        /// <param name="relationshipType">RelationshipType object</param>        
        /// <remarks>
        /// If RelationshipTypeId is -1 (Null.NullIntger), then a new RelationshipType is created, 
        /// else existing RelationshipType is updated
        /// </remarks>
        /// -----------------------------------------------------------------------------
        void SaveRelationshipType(RelationshipType relationshipType);

        #endregion

        #region Relationship

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete Relationship
        /// </summary>
        /// <param name="relationship">Relationship</param>        
        /// -----------------------------------------------------------------------------
        void DeleteRelationship(Relationship relationship);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get Relationship By RelationshipId
        /// </summary>        
        /// <param name="relationshipId">RelationshipId</param>        
        /// -----------------------------------------------------------------------------
        Relationship GetRelationship(int relationshipId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get Relationships By UserId
        /// </summary>        
        /// <param name="userId">UserId</param>        
        /// -----------------------------------------------------------------------------
        IList<Relationship> GetRelationshipsByUserId(int userId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get Relationships By PortalId
        /// </summary>        
        /// <param name="portalId">PortalId</param>        
        /// -----------------------------------------------------------------------------
        IList<Relationship> GetRelationshipsByPortalId(int portalId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save Relationship
        /// </summary>
        /// <param name="relationship">Relationship object</param>        
        /// <remarks>
        /// If RelationshipId is -1 (Null.NullIntger), then a new Relationship is created, 
        /// else existing Relationship is updated
        /// </remarks>
        /// -----------------------------------------------------------------------------
        void SaveRelationship(Relationship relationship);

        #endregion

        #region UserRelationship

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete UserRelationship
        /// </summary>
        /// <param name="userRelationship">UserRelationship to delete</param>        
        /// -----------------------------------------------------------------------------
        void DeleteUserRelationship(UserRelationship userRelationship);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationship By UserRelationshipId
        /// </summary>        
        /// <param name="userRelationshipId">UserRelationshipId</param>        
        /// -----------------------------------------------------------------------------
        UserRelationship GetUserRelationship(int userRelationshipId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationship by its members
        /// </summary>        
        /// <param name="user">User</param>        
        /// <param name="relatedUser">Related User</param>   
        /// <param name="relationship">Relationship Object</param>             
        /// -----------------------------------------------------------------------------
        UserRelationship GetUserRelationship(UserInfo user, UserInfo relatedUser, Relationship relationship);

        /// <summary>
        /// This method gets a Dictionary of all the relationships that a User belongs to and the members of thase relationships.
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>A Dictionary of Lists of UserRelationship, keyed by the Relationship</returns>
        IList<UserRelationship> GetUserRelationships(UserInfo user);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save UserRelationship
        /// </summary>
        /// <param name="userRelationship">UserRelationship object</param>        
        /// <remarks>
        /// If UserRelationshipId is -1 (Null.NullIntger), then a new UserRelationship is created, 
        /// else existing UserRelationship is updated
        /// </remarks>
        /// -----------------------------------------------------------------------------
        void SaveUserRelationship(UserRelationship userRelationship);

        #endregion

        #region UserRelationshipPreference

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Delete UserRelationshipPreference
        /// </summary>
        /// <param name="userRelationshipPreference">UserRelationshipPreference to delete</param>        
        /// -----------------------------------------------------------------------------
        void DeleteUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationshipPreference By RelationshipTypeId
        /// </summary>        
        /// <param name="preferenceId">PreferenceId</param>        
        /// -----------------------------------------------------------------------------
        UserRelationshipPreference GetUserRelationshipPreference(int preferenceId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get UserRelationshipPreference By UserId and RelationshipId
        /// </summary>        
        /// <param name="userId">UserId</param>        
        /// <param name="relationshipId">RelationshipId</param>        
        /// -----------------------------------------------------------------------------
        UserRelationshipPreference GetUserRelationshipPreference(int userId, int relationshipId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Save UserRelationshipPreference
        /// </summary>
        /// <param name="userRelationshipPreference">UserRelationshipPreference object</param>        
        /// <remarks>
        /// If PreferenceId is -1 (Null.NullIntger), then a new UserRelationshipPreference is created, 
        /// else existing UserRelationshipPreference is updated
        /// </remarks>
        /// -----------------------------------------------------------------------------
        void SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference);

        #endregion

        #region Relationship Business APIs

        UserRelationship InitiateUserRelationship(UserInfo initiatingUser, UserInfo targetUser, Relationship relationship);
       
        void AcceptUserRelationship(int userRelationshipId);

        void RemoveUserRelationship(int userRelationshipId);
                      
        #endregion

        #region Easy Wrapper APIs

        UserRelationship GetFollowerRelationship(UserInfo targetUser);
        UserRelationship GetFollowerRelationship(UserInfo initiatingUser, UserInfo targetUser);

        UserRelationship GetFollowingRelationship(UserInfo targetUser);
        UserRelationship GetFollowingRelationship(UserInfo initiatingUser, UserInfo targetUser);

        UserRelationship GetFriendRelationship(UserInfo targetUser);
        UserRelationship GetFriendRelationship(UserInfo initiatingUser, UserInfo targetUser);

        #endregion

        void CreateDefaultRelationshipsForPortal(int portalId);

        Relationship GetFriendsRelationshipByPortal(int portalId);

        Relationship GetFollowersRelationshipByPortal(int portalId);
    }
}