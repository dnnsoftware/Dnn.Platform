// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Profile
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;

    [Serializable]
    public class ProfileVisibility
    {
        public ProfileVisibility()
        {
            this.RoleVisibilities = new List<RoleInfo>();
            this.RelationshipVisibilities = new List<Relationship>();
        }

        public ProfileVisibility(int portalId, string extendedVisibility)
            : this()
        {
            if (!string.IsNullOrEmpty(extendedVisibility))
            {
                var relationshipController = new RelationshipController();

                var lists = extendedVisibility.Split(';');

                if (!string.IsNullOrEmpty(lists[0].Substring(2).TrimEnd(',')))
                {
                    var roles = lists[0].Substring(2).TrimEnd(',').Split(',');
                    foreach (var role in roles)
                    {
                        int roleId = int.Parse(role);
                        RoleInfo userRole = RoleController.Instance.GetRole(portalId, r => r.RoleID == roleId);
                        this.RoleVisibilities.Add(userRole);
                    }
                }

                if (!string.IsNullOrEmpty(lists[1].Substring(2).TrimEnd(',')))
                {
                    var relationships = lists[1].Substring(2).TrimEnd(',').Split(',');
                    foreach (var relationship in relationships)
                    {
                        Relationship userRelationship = RelationshipController.Instance.GetRelationship(int.Parse(relationship));
                        this.RelationshipVisibilities.Add(userRelationship);
                    }
                }
            }
        }

        public UserVisibilityMode VisibilityMode { get; set; }

        public IList<RoleInfo> RoleVisibilities { get; set; }

        public IList<Relationship> RelationshipVisibilities { get; set; }

        public ProfileVisibility Clone()
        {
            var pv = new ProfileVisibility()
            {
                VisibilityMode = this.VisibilityMode,
                RoleVisibilities = new List<RoleInfo>(this.RoleVisibilities),
                RelationshipVisibilities = new List<Relationship>(this.RelationshipVisibilities),
            };
            return pv;
        }

        public string ExtendedVisibilityString()
        {
            if (this.VisibilityMode == UserVisibilityMode.FriendsAndGroups)
            {
                var sb = new StringBuilder();
                sb.Append("G:");
                foreach (var role in this.RoleVisibilities)
                {
                    sb.Append(role.RoleID.ToString(CultureInfo.InvariantCulture) + ",");
                }

                sb.Append(";R:");
                foreach (var relationship in this.RelationshipVisibilities)
                {
                    sb.Append(relationship.RelationshipId.ToString(CultureInfo.InvariantCulture) + ",");
                }

                return sb.ToString();
            }

            return string.Empty;
        }
    }
}
