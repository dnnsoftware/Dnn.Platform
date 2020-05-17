﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

using DotNetNuke.Security.Roles;

#endregion

namespace DotNetNuke.Entities.Users.Social
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserSocial
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserSocial is a high-level class describing social details of a user. 
    /// As an example, this class contains Friends, Followers, Follows lists.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserSocial
    {
        #region Private

        private IList<Relationship> _relationships;
        private IList<UserRelationship> _userRelationships;
        private  IList<UserRoleInfo> _roles;
        private readonly UserInfo _userInfo;

        #endregion

        #region Constructor

        public UserSocial(UserInfo userInfo)
        {
            _userInfo = userInfo;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the Friend Relationship (if it exists with the current User)
        /// </summary>
        public UserRelationship Friend
        {
            get
            {
                var _friendsRelationship = RelationshipController.Instance.GetFriendsRelationshipByPortal(_userInfo.PortalID);
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                return UserRelationships.SingleOrDefault(ur => (ur.RelationshipId == _friendsRelationship.RelationshipId
                                                                && 
                                                                (ur.UserId == _userInfo.UserID &&
                                                                 ur.RelatedUserId == currentUser.UserID
                                                                 ||
                                                                 (ur.UserId == currentUser.UserID &&
                                                                  ur.RelatedUserId == _userInfo.UserID)
                                                                )));
            }
        }

        /// <summary>
        /// Returns the Follower Relationship. Does the user in object Follow the current User (with any status)
        /// </summary>
        public UserRelationship Follower
        {
            get
            {
                var _followerRelationship = RelationshipController.Instance.GetFollowersRelationshipByPortal(_userInfo.PortalID);
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                return UserRelationships.SingleOrDefault(ur => (ur.RelationshipId == _followerRelationship.RelationshipId
                                                                &&
                                                                (ur.UserId == _userInfo.UserID &&
                                                                 ur.RelatedUserId == currentUser.UserID
                                                                )));
            }
        }

        /// <summary>
        /// Returns the Following Relationship (if it exists with the current User)
        /// </summary>
        public UserRelationship Following
        {
            get
            {
                var _followerRelationship = RelationshipController.Instance.GetFollowersRelationshipByPortal(_userInfo.PortalID);
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                return UserRelationships.SingleOrDefault(ur => (ur.RelationshipId == _followerRelationship.RelationshipId
                                                                &&
                                                                (ur.UserId == currentUser.UserID &&
                                                                 ur.RelatedUserId == _userInfo.UserID
                                                                )));
            }
        }

        /// <summary>
        /// A collection of all the relationships the user is a member of.
        /// </summary>
        public IList<UserRelationship> UserRelationships
        {
            get { return _userRelationships ?? (_userRelationships = RelationshipController.Instance.GetUserRelationships(_userInfo)); }
        }

        /// <summary>
        /// List of Relationships for the User
        /// </summary>
        [XmlAttribute]
        public IList<Relationship> Relationships
        {
            get
            {
                if (_relationships == null)
                {
                    _relationships = RelationshipController.Instance.GetRelationshipsByPortalId(_userInfo.PortalID);

                    foreach (var r in RelationshipController.Instance.GetRelationshipsByUserId(_userInfo.UserID))
                    {
                        _relationships.Add(r);
                    }
                }

                return _relationships;
            }
        }

        /// <summary>
        /// List of Roles/Groups for the User
        /// </summary>
        [XmlAttribute]
        public IList<UserRoleInfo> Roles
        {
            get 
            {
                return _roles ?? (_roles = (_userInfo.PortalID == -1 && _userInfo.UserID == -1)
                                            ? new List<UserRoleInfo>(0)
                                            : RoleController.Instance.GetUserRoles(_userInfo, true)
                                ); 
            }
        }

        #endregion
    }
}
