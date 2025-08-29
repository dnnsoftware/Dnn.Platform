// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    using DotNetNuke.Security.Roles;

    /// <summary>
    /// The UserSocial is a high-level class describing social details of a user.
    /// As an example, this class contains Friends, Followers, Follows lists.
    /// </summary>
    [Serializable]
    public class UserSocial
    {
        private readonly UserInfo userInfo;
        private IList<Relationship> relationships;
        private IList<UserRelationship> userRelationships;
        private IList<UserRoleInfo> roles;

        /// <summary>Initializes a new instance of the <see cref="UserSocial"/> class.</summary>
        /// <param name="userInfo">The user info.</param>
        public UserSocial(UserInfo userInfo)
        {
            this.userInfo = userInfo;
        }

        /// <summary>Gets the Friend Relationship (if it exists with the current User).</summary>
        public UserRelationship Friend
        {
            get
            {
                var friendsRelationship = RelationshipController.Instance.GetFriendsRelationshipByPortal(this.userInfo.PortalID);
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                return this.UserRelationships.SingleOrDefault(ur => (ur.RelationshipId == friendsRelationship.RelationshipId
                                                                &&
                                                                ((ur.UserId == this.userInfo.UserID &&
                                                                 ur.RelatedUserId == currentUser.UserID)
                                                                 ||
                                                                 (ur.UserId == currentUser.UserID &&
                                                                  ur.RelatedUserId == this.userInfo.UserID))));
            }
        }

        /// <summary>Gets the Follower Relationship. Does the user in object Follow the current User (with any status).</summary>
        public UserRelationship Follower
        {
            get
            {
                var followerRelationship = RelationshipController.Instance.GetFollowersRelationshipByPortal(this.userInfo.PortalID);
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                return this.UserRelationships.SingleOrDefault(ur => (ur.RelationshipId == followerRelationship.RelationshipId
                                                                &&
                                                                (ur.UserId == this.userInfo.UserID &&
                                                                 ur.RelatedUserId == currentUser.UserID)));
            }
        }

        /// <summary>Gets the Following Relationship (if it exists with the current User).</summary>
        public UserRelationship Following
        {
            get
            {
                var followerRelationship = RelationshipController.Instance.GetFollowersRelationshipByPortal(this.userInfo.PortalID);
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                return this.UserRelationships.SingleOrDefault(ur => (ur.RelationshipId == followerRelationship.RelationshipId
                                                                &&
                                                                (ur.UserId == currentUser.UserID &&
                                                                 ur.RelatedUserId == this.userInfo.UserID)));
            }
        }

        /// <summary>Gets a collection of all the relationships the user is a member of.</summary>
        public IList<UserRelationship> UserRelationships
        {
            get { return this.userRelationships ?? (this.userRelationships = RelationshipController.Instance.GetUserRelationships(this.userInfo)); }
        }

        /// <summary>Gets list of Relationships for the User.</summary>
        [XmlAttribute]
        public IList<Relationship> Relationships
        {
            get
            {
                if (this.relationships == null)
                {
                    this.relationships = RelationshipController.Instance.GetRelationshipsByPortalId(this.userInfo.PortalID);

                    foreach (var r in RelationshipController.Instance.GetRelationshipsByUserId(this.userInfo.UserID))
                    {
                        this.relationships.Add(r);
                    }
                }

                return this.relationships;
            }
        }

        /// <summary>Gets list of Roles/Groups for the User.</summary>
        [XmlAttribute]
        public IList<UserRoleInfo> Roles
        {
            get
            {
                return this.roles ??= this.userInfo.PortalID == -1 && this.userInfo.UserID == -1
                    ? new List<UserRoleInfo>(0)
                    : RoleController.Instance.GetUserRoles(this.userInfo, true);
            }
        }
    }
}
