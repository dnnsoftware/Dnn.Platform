#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users.Social.Data;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Social.Notifications;

#endregion

namespace DotNetNuke.Entities.Users.Social
{
    internal class RelationshipControllerImpl : IRelationshipController
    {
        #region Constants

        internal const string FriendRequest = "FriendRequest";
        internal const string FollowerRequest = "FollowerRequest";
        internal const string FollowBackRequest = "FollowBackRequest";

        #endregion

        #region Private Variables

        private readonly IDataService _dataService;
        private readonly IEventLogController _eventLogController;

        #endregion

        #region Constructors

        public RelationshipControllerImpl()
            : this(DataService.Instance, EventLogController.Instance)
        {
        }

        public RelationshipControllerImpl(IDataService dataService, IEventLogController eventLogController)
        {
            //Argument Contract
            Requires.NotNull("dataService", dataService);
            Requires.NotNull("eventLogController", eventLogController);

            _dataService = dataService;
            _eventLogController = eventLogController;
        }

        #endregion

        #region IRelationshipController Implementation

        #region RelationshipType CRUD

        public void DeleteRelationshipType(RelationshipType relationshipType)
        {
            Requires.NotNull("relationshipType", relationshipType);

            _dataService.DeleteRelationshipType(relationshipType.RelationshipTypeId);

            //log event
            string logContent =
                string.Format(Localization.GetString("RelationshipType_Deleted", Localization.GlobalResourceFile),
                              relationshipType.Name, relationshipType.RelationshipTypeId);
            AddLog(logContent);

            //clear cache
            DataCache.RemoveCache(DataCache.RelationshipTypesCacheKey);
        }

        public IList<RelationshipType> GetAllRelationshipTypes()
        {
            var cacheArgs = new CacheItemArgs(DataCache.RelationshipTypesCacheKey,
                                              DataCache.RelationshipTypesCacheTimeOut,
                                              DataCache.RelationshipTypesCachePriority);
            return CBO.GetCachedObject<IList<RelationshipType>>(cacheArgs,
                                                                c =>
                                                                CBO.FillCollection<RelationshipType>(
                                                                    _dataService.GetAllRelationshipTypes()));
        }

        public RelationshipType GetRelationshipType(int relationshipTypeId)
        {
            return GetAllRelationshipTypes().FirstOrDefault(r => r.RelationshipTypeId == relationshipTypeId);
        }

        public void SaveRelationshipType(RelationshipType relationshipType)
        {
            Requires.NotNull("relationshipType", relationshipType);

            string localizationKey = (relationshipType.RelationshipTypeId == Null.NullInteger)
                                         ? "RelationshipType_Added"
                                         : "RelationshipType_Updated";

            relationshipType.RelationshipTypeId = _dataService.SaveRelationshipType(relationshipType,
                                                                                    UserController.Instance.GetCurrentUserInfo().
                                                                                        UserID);

            //log event
            string logContent = string.Format(Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                                              relationshipType.Name);
            AddLog(logContent);

            //clear cache
            DataCache.RemoveCache(DataCache.RelationshipTypesCacheKey);
        }

        #endregion

        #region Relationship CRUD

        public void DeleteRelationship(Relationship relationship)
        {
            Requires.NotNull("relationship", relationship);

            _dataService.DeleteRelationship(relationship.RelationshipId);

            //log event
            string logContent =
                string.Format(Localization.GetString("Relationship_Deleted", Localization.GlobalResourceFile),
                              relationship.Name, relationship.RelationshipId);
            AddLog(logContent);

            //clear cache
            ClearRelationshipCache(relationship);
        }

        public Relationship GetRelationship(int relationshipId)
        {
            return CBO.FillCollection<Relationship>(_dataService.GetRelationship(relationshipId)).FirstOrDefault();
        }

        public IList<Relationship> GetRelationshipsByUserId(int userId)
        {
            return CBO.FillCollection<Relationship>(_dataService.GetRelationshipsByUserId(userId));
        }

        public IList<Relationship> GetRelationshipsByPortalId(int portalId)
        {
            var pid = portalId;
            if (PortalController.IsMemberOfPortalGroup(portalId))
            {
                pid = PortalController.GetEffectivePortalId(portalId);
            }
            var cacheArgs = new CacheItemArgs(string.Format(DataCache.RelationshipByPortalIDCacheKey, pid),
                                              DataCache.RelationshipByPortalIDCacheTimeOut,
                                              DataCache.RelationshipByPortalIDCachePriority,
                                              pid);
            return CBO.GetCachedObject<IList<Relationship>>(cacheArgs,
                                                            c =>
                                                            CBO.FillCollection<Relationship>(
                                                                _dataService.GetRelationshipsByPortalId(
                                                                    (int) c.ParamList[0])));
        }

        public void SaveRelationship(Relationship relationship)
        {
            Requires.NotNull("relationship", relationship);

            string localizationKey = (relationship.RelationshipId == Null.NullInteger)
                                         ? "Relationship_Added"
                                         : "Relationship_Updated";

            relationship.RelationshipId = _dataService.SaveRelationship(relationship,
                                                                        UserController.Instance.GetCurrentUserInfo().UserID);

            //log event
            string logContent = string.Format(Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                                              relationship.Name);
            AddLog(logContent);

            //clear cache
            ClearRelationshipCache(relationship);
        }

        #endregion

        #region UserRelationship CRUD

        public void DeleteUserRelationship(UserRelationship userRelationship)
        {
            Requires.NotNull("userRelationship", userRelationship);

            _dataService.DeleteUserRelationship(userRelationship.UserRelationshipId);

            //log event
            string logContent =
                string.Format(Localization.GetString("UserRelationship_Deleted", Localization.GlobalResourceFile),
                              userRelationship.UserRelationshipId, userRelationship.UserId,
                              userRelationship.RelatedUserId);
            AddLog(logContent);

            //cache clear
            ClearUserCache(userRelationship);
        }

        public UserRelationship GetUserRelationship(int userRelationshipId)
        {
            return CBO.FillObject<UserRelationship>(_dataService.GetUserRelationship(userRelationshipId));
        }

        public UserRelationship GetUserRelationship(UserInfo user, UserInfo relatedUser, Relationship relationship)
        {
            UserRelationship userRelationship = null;
            if (relationship != null)
            {
                userRelationship = CBO.FillObject<UserRelationship>(_dataService.GetUserRelationship(user.UserID, relatedUser.UserID,
                                                                                  relationship.RelationshipId,
                                                                                  GetRelationshipType(
                                                                                      relationship.RelationshipTypeId).
                                                                                      Direction));
            }
            return userRelationship;

        }

        public IList<UserRelationship> GetUserRelationships(UserInfo user)
        {
            return CBO.FillCollection<UserRelationship>(_dataService.GetUserRelationships(user.UserID));
        }

        public void SaveUserRelationship(UserRelationship userRelationship)
        {
            Requires.NotNull("userRelationship", userRelationship);

            string localizationKey = (userRelationship.UserRelationshipId == Null.NullInteger)
                                         ? "UserRelationship_Added"
                                         : "UserRelationship_Updated";

            userRelationship.UserRelationshipId = _dataService.SaveUserRelationship(userRelationship,
                                                                                    UserController.Instance.GetCurrentUserInfo().
                                                                                        UserID);

            //log event            
            string logContent = string.Format(Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                                              userRelationship.UserRelationshipId, userRelationship.UserId,
                                              userRelationship.RelatedUserId);
            AddLog(logContent);

            //cache clear
            ClearUserCache(userRelationship);
        }

        #endregion

        #region UserRelationshipPreference CRUD

        public void DeleteUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference)
        {
            Requires.NotNull("userRelationshipPreference", userRelationshipPreference);

            _dataService.DeleteUserRelationshipPreference(userRelationshipPreference.PreferenceId);

            //log event
            string logContent =
                string.Format(
                    Localization.GetString("UserRelationshipPreference_Deleted", Localization.GlobalResourceFile),
                    userRelationshipPreference.PreferenceId, userRelationshipPreference.UserId,
                    userRelationshipPreference.RelationshipId);
            AddLog(logContent);
        }

        public UserRelationshipPreference GetUserRelationshipPreference(int preferenceId)
        {
            return
                CBO.FillObject<UserRelationshipPreference>(_dataService.GetUserRelationshipPreferenceById(preferenceId));
        }

        public UserRelationshipPreference GetUserRelationshipPreference(int userId, int relationshipId)
        {
            return
                CBO.FillObject<UserRelationshipPreference>(_dataService.GetUserRelationshipPreference(userId,
                                                                                                      relationshipId));
        }

        public void SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference)
        {
            Requires.NotNull("userRelationshipPreference", userRelationshipPreference);

            string localizationKey = (userRelationshipPreference.PreferenceId == Null.NullInteger)
                                         ? "UserRelationshipPreference_Added"
                                         : "UserRelationshipPreference_Updated";

            userRelationshipPreference.PreferenceId =
                _dataService.SaveUserRelationshipPreference(userRelationshipPreference,
                                                            UserController.Instance.GetCurrentUserInfo().UserID);

            //log event            
            string logContent = string.Format(Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                                              userRelationshipPreference.PreferenceId, userRelationshipPreference.UserId,
                                              userRelationshipPreference.RelationshipId);
            AddLog(logContent);
        }

        #endregion

        #region Relationship Business APIs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initiate an UserRelationship Request
        /// </summary>
        /// <param name="initiatingUser">UserInfo of the user initiating the request</param>        
        /// <param name="targetUser">UserInfo of the user being solicited for initiating the request</param>        
        /// <param name="relationship">Relationship to associate this request to (Portal-Level Relationship or User-Level Relationship)</param>        
        /// <remarks>
        /// If all conditions are met UserRelationship object belonging to Initiating User is returned.
        /// </remarks>
        /// <returns>
        /// Relationship object belonging to the initiating user
        /// </returns>
        /// <exception cref="UserRelationshipBlockedException">Target user has Blocked any relationship request from Initiating user</exception>
        /// <exception cref="InvalidRelationshipTypeException">Relationship type does not exist</exception>
        /// -----------------------------------------------------------------------------
        public UserRelationship InitiateUserRelationship(UserInfo initiatingUser, UserInfo targetUser,
                                                         Relationship relationship)
        {
            Requires.NotNull("user1", initiatingUser);
            Requires.NotNull("user2", targetUser);
            Requires.NotNull("relationship", relationship);

            Requires.PropertyNotNegative("user1", "UserID", initiatingUser.UserID);
            Requires.PropertyNotNegative("user2", "UserID", targetUser.UserID);

            Requires.PropertyNotNegative("user1", "PortalID", initiatingUser.PortalID);
            Requires.PropertyNotNegative("user2", "PortalID", targetUser.PortalID);

            Requires.PropertyNotNegative("relationship", "RelationshipId", relationship.RelationshipId);

            //cannot be same user
            if (initiatingUser.UserID == targetUser.UserID)
            {
                throw new UserRelationshipForSameUsersException(
                    Localization.GetExceptionMessage("UserRelationshipForSameUsersError",
                                                     "Initiating and Target Users cannot have same UserID '{0}'.",
                                                     initiatingUser.UserID));
            }

            //users must be from same portal
            if (initiatingUser.PortalID != targetUser.PortalID)
            {
                throw new UserRelationshipForDifferentPortalException(
                    Localization.GetExceptionMessage("UserRelationshipForDifferentPortalError",
                                                     "Portal ID '{0}' of Initiating User is different from Portal ID '{1}' of Target  User.",
                                                     initiatingUser.PortalID, targetUser.PortalID));
            }

            //check for existing UserRelationship record
            UserRelationship existingRelationship = GetUserRelationship(initiatingUser, targetUser, relationship);
            if (existingRelationship != null)
            {
                throw new UserRelationshipExistsException(Localization.GetExceptionMessage(
                    "UserRelationshipExistsError",
                    "Relationship already exists for Initiating User '{0}' Target User '{1}' RelationshipID '{2}'.",
                    initiatingUser.UserID, targetUser.UserID, relationship.RelationshipId));
            }

            //no existing UserRelationship record found 


            //use Relationship DefaultResponse as status
            RelationshipStatus status = relationship.DefaultResponse;


            UserRelationshipPreference preference = GetUserRelationshipPreference(targetUser.UserID,
                                                                                  relationship.RelationshipId);
            if (preference != null)
            {
                status = preference.DefaultResponse;
            }
            

            if (status == RelationshipStatus.None)
            {
                status = RelationshipStatus.Pending;
            }

            var userRelationship = new UserRelationship
                                       {
                                           UserRelationshipId = Null.NullInteger,
                                           UserId = initiatingUser.UserID,
                                           RelatedUserId = targetUser.UserID,
                                           RelationshipId = relationship.RelationshipId,
                                           Status = status
                                       };

            SaveUserRelationship(userRelationship);

            return userRelationship;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Accept an existing UserRelationship Request
        /// </summary>
        /// <param name="userRelationshipId">UserRelationshipId of the UserRelationship</param>        
        /// <remarks>
        /// Method updates the status of the UserRelationship to Accepted.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void AcceptUserRelationship(int userRelationshipId)
        {
            ManageUserRelationshipStatus(userRelationshipId, RelationshipStatus.Accepted);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Remove an existing UserRelationship Request
        /// </summary>
        /// <param name="userRelationshipId">UserRelationshipId of the UserRelationship</param>        
        /// <remarks>
        /// UserRelationship record is physically removed.
        /// </remarks>
        /// -----------------------------------------------------------------------------  
        public void RemoveUserRelationship(int userRelationshipId)
        {
            UserRelationship userRelationship = VerifyUserRelationshipExist(userRelationshipId);
            if (userRelationship != null)
            {
                DeleteUserRelationship(userRelationship);
            }
        }

        #endregion

        #region Easy Wrapper APIs
        
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFollowerRelationship - Get the UserRelationship between Current User and the Target Users in Follower Relationship
        /// </summary>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship</returns>
        /// <remarks>UserRelationship object is returned if a Follower Relationship exists between the two Users. 
        /// The relation status can be Any (Initiated / Accepted / Blocked). Follower Relationship can be initited by either of the Users.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship GetFollowerRelationship(UserInfo targetUser)
        {
            return GetFollowerRelationship(UserController.Instance.GetCurrentUserInfo(), targetUser);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFollowerRelationship - Get the UserRelationship between InitiatingUser User and the Target Users in Follower Relationship
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship</returns>
        /// <remarks>UserRelationship object is returned if a Follower Relationship exists between the two Users. 
        /// The relation status can be Any (Initiated / Accepted / Blocked). Follower Relationship can be initited by either of the Users.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship GetFollowerRelationship(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("user1", initiatingUser);
            Requires.NotNull("user2", targetUser);

            return GetUserRelationship(initiatingUser, targetUser,
                                       GetFollowersRelationshipByPortal(initiatingUser.PortalID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFollowingRelationship - Get the UserRelationship between Current User and the Target Users in Following Relationship
        /// </summary>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship</returns>
        /// <remarks>UserRelationship object is returned if a Following Relationship exists between the two Users. 
        /// The relation status can be Any (Initiated / Accepted / Blocked).
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship GetFollowingRelationship(UserInfo targetUser)
        {
            return GetFollowingRelationship(UserController.Instance.GetCurrentUserInfo(), targetUser);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFollowingRelationship - Get the UserRelationship between InitiatingUser User and the Target Users in Following Relationship
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship</returns>
        /// <remarks>UserRelationship object is returned if a Following Relationship exists between the two Users. 
        /// The relation status can be Any (Initiated / Accepted / Blocked).
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship GetFollowingRelationship(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("user1", initiatingUser);
            Requires.NotNull("user2", targetUser);

            return GetUserRelationship(targetUser, initiatingUser,
                                       GetFollowersRelationshipByPortal(initiatingUser.PortalID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFriendRelationship - Get the UserRelationship between Current User and the Target Users in Friend Relationship
        /// </summary>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship</returns>
        /// <remarks>UserRelationship object is returned if a Friend Relationship exists between the two Users. 
        /// The relation status can be Any (Initiated / Accepted / Blocked). Friend Relationship can be initited by either of the Users.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship GetFriendRelationship(UserInfo targetUser)
        {
            return GetFriendRelationship(UserController.Instance.GetCurrentUserInfo(), targetUser);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFriendRelationship - Get the UserRelationship between InitiatingUser User and the Target Users in Friend Relationship
        /// </summary>        
        /// <param name="initiatingUser">UserInfo for Initiating User</param>        
        /// <param name="targetUser">UserInfo for Target User</param>        
        /// <returns>UserRelationship</returns>
        /// <remarks>UserRelationship object is returned if a Friend Relationship exists between the two Users. 
        /// The relation status can be Any (Initiated / Accepted / Blocked). Friend Relationship can be initited by either of the Users.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public UserRelationship GetFriendRelationship(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("user1", initiatingUser);
            Requires.NotNull("user2", targetUser);

            return GetUserRelationship(initiatingUser, targetUser,
                                       GetFriendsRelationshipByPortal(initiatingUser.PortalID));
        }

        #endregion

        public void CreateDefaultRelationshipsForPortal(int portalId)
        {
            //create default Friend Relationship
            if (GetFriendsRelationshipByPortal(portalId) == null)
            {
                var friendRelationship = new Relationship
                                             {
                                                 RelationshipId = Null.NullInteger,
                                                 Name = DefaultRelationshipTypes.Friends.ToString(),
                                                 Description = DefaultRelationshipTypes.Friends.ToString(),
                                                 PortalId = portalId,
                                                 UserId = Null.NullInteger,
                                                 DefaultResponse = RelationshipStatus.None,
                                                 //default response is None
                                                 RelationshipTypeId = (int) DefaultRelationshipTypes.Friends
                                             };
                SaveRelationship(friendRelationship);
            }

            //create default Follower Relationship
            if (GetFollowersRelationshipByPortal(portalId) == null)
            {
                var followerRelationship = new Relationship
                                               {
                                                   RelationshipId = Null.NullInteger,
                                                   Name = DefaultRelationshipTypes.Followers.ToString(),
                                                   Description = DefaultRelationshipTypes.Followers.ToString(),
                                                   PortalId = portalId,
                                                   UserId = Null.NullInteger,
                                                   DefaultResponse = RelationshipStatus.Accepted,
                                                   //default response is Accepted
                                                   RelationshipTypeId = (int) DefaultRelationshipTypes.Followers
                                               };
                SaveRelationship(followerRelationship);
            }
        }

        public Relationship GetFriendsRelationshipByPortal(int portalId)
        {
           return GetRelationshipsByPortalId(portalId).FirstOrDefault(re => re.RelationshipTypeId == (int)DefaultRelationshipTypes.Friends);
        }

        public Relationship GetFollowersRelationshipByPortal(int portalId)
        {
            
            return GetRelationshipsByPortalId(portalId).FirstOrDefault(re => re.RelationshipTypeId == (int)DefaultRelationshipTypes.Followers);
        }


        #endregion

        #region Private Methods

        private void AddLog(string logContent)
        {
            _eventLogController.AddLog("Message", logContent, EventLogController.EventLogType.ADMIN_ALERT);
        }

        private void ClearRelationshipCache(Relationship relationship)
        {
            if (relationship.UserId == Null.NullInteger)
            {
                DataCache.RemoveCache(string.Format(DataCache.RelationshipByPortalIDCacheKey, relationship.PortalId));
            }
        }

        private void ClearUserCache(UserRelationship userRelationship)
        {
            //Get Portal
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();

            if (settings != null)
            {
                //Get User
                UserInfo user = UserController.GetUserById(settings.PortalId, userRelationship.UserId);

                if (user != null)
                {
                    DataCache.ClearUserCache(settings.PortalId, user.Username);
                }

                //Get Related User
                UserInfo relatedUser = UserController.GetUserById(settings.PortalId, userRelationship.RelatedUserId);

                if (relatedUser != null)
                {
                    DataCache.ClearUserCache(settings.PortalId, relatedUser.Username);
                }
            }
        }

        private void ManageUserRelationshipStatus(int userRelationshipId, RelationshipStatus newStatus)
        {
            UserRelationship userRelationship = VerifyUserRelationshipExist(userRelationshipId);
            if (userRelationship == null)
            {
                return;
            }

            //TODO - apply business rules - throw exception if newStatus requested is not allowed
            bool save = false;
            switch (newStatus)
            {
                case RelationshipStatus.None:
                    save = true;
                    break;
                case RelationshipStatus.Pending:
                    save = true;
                    break;
                case RelationshipStatus.Accepted:
                    save = true;
                    break;
            }

            if (save)
            {
                userRelationship.Status = newStatus;
                SaveUserRelationship(userRelationship);
            }
        }

        private UserRelationship VerifyUserRelationshipExist(int userRelationshipId)
        {
            UserRelationship userRelationship = GetUserRelationship(userRelationshipId);
            if (userRelationship == null)
            {
                throw new UserRelationshipDoesNotExistException(
                    Localization.GetExceptionMessage("UserRelationshipDoesNotExistError",
                                                     "UserRelationshipID '{0}' does not exist.", userRelationshipId));
            }

            return userRelationship;
        }

        #endregion
    }
}