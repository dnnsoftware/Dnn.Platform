// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Serialization;

    using DotNetNuke.Security.Roles;

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
        private readonly UserInfo _userInfo;
        private IList<Relationship> _relationships;
        private IList<UserRelationship> _userRelationships;
        private IList<UserRoleInfo> _roles;

        public UserSocial(UserInfo userInfo)
        {
            this._userInfo = userInfo;
        }

        /// <summary>
        /// Gets the Friend Relationship (if it exists with the current User).
        /// </summary>
        public UserRelationship Friend
        {
            get
            {
                var _friendsRelationship = RelationshipController.Instance.GetFriendsRelationshipByPortal(this._userInfo.PortalID);
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                return this.UserRelationships.SingleOrDefault(ur => (ur.RelationshipId == _friendsRelationship.RelationshipId
                                                                &&
                                                                ((ur.UserId == this._userInfo.UserID &&
                                                                 ur.RelatedUserId == currentUser.UserID)
                                                                 ||
                                                                 (ur.UserId == currentUser.UserID &&
                                                                  ur.RelatedUserId == this._userInfo.UserID))));
            }
        }

        /// <summary>
        /// Gets the Follower Relationship. Does the user in object Follow the current User (with any status).
        /// </summary>
        public UserRelationship Follower
        {
            get
            {
                var _followerRelationship = RelationshipController.Instance.GetFollowersRelationshipByPortal(this._userInfo.PortalID);
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                return this.UserRelationships.SingleOrDefault(ur => (ur.RelationshipId == _followerRelationship.RelationshipId
                                                                &&
                                                                (ur.UserId == this._userInfo.UserID &&
                                                                 ur.RelatedUserId == currentUser.UserID)));
            }
        }

        /// <summary>
        /// Gets the Following Relationship (if it exists with the current User).
        /// </summary>
        public UserRelationship Following
        {
            get
            {
                var _followerRelationship = RelationshipController.Instance.GetFollowersRelationshipByPortal(this._userInfo.PortalID);
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                return this.UserRelationships.SingleOrDefault(ur => (ur.RelationshipId == _followerRelationship.RelationshipId
                                                                &&
                                                                (ur.UserId == currentUser.UserID &&
                                                                 ur.RelatedUserId == this._userInfo.UserID)));
            }
        }

        /// <summary>
        /// Gets a collection of all the relationships the user is a member of.
        /// </summary>
        public IList<UserRelationship> UserRelationships
        {
            get { return this._userRelationships ?? (this._userRelationships = RelationshipController.Instance.GetUserRelationships(this._userInfo)); }
        }

        /// <summary>
        /// Gets list of Relationships for the User.
        /// </summary>
        [XmlAttribute]
        public IList<Relationship> Relationships
        {
            get
            {
                if (this._relationships == null)
                {
                    this._relationships = RelationshipController.Instance.GetRelationshipsByPortalId(this._userInfo.PortalID);

                    foreach (var r in RelationshipController.Instance.GetRelationshipsByUserId(this._userInfo.UserID))
                    {
                        this._relationships.Add(r);
                    }
                }

                return this._relationships;
            }
        }

        /// <summary>
        /// Gets list of Roles/Groups for the User.
        /// </summary>
        [XmlAttribute]
        public IList<UserRoleInfo> Roles
        {
            get
            {
                return this._roles ?? (this._roles = (this._userInfo.PortalID == -1 && this._userInfo.UserID == -1)
                                            ? new List<UserRoleInfo>(0)
                                            : RoleController.Instance.GetUserRoles(this._userInfo, true));
            }
        }
    }
}
