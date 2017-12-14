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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;

namespace DotNetNuke.Entities.Profile
{
    [Serializable]
    public class ProfileVisibility
    {
        public ProfileVisibility()
        {
            RoleVisibilities = new List<RoleInfo>();
            RelationshipVisibilities = new List<Relationship>();
        }

        public ProfileVisibility(int portalId, string extendedVisibility) : this()
        {
            if (!String.IsNullOrEmpty(extendedVisibility))
            {
                var relationshipController = new RelationshipController();

                var lists = extendedVisibility.Split(';');

                if (!String.IsNullOrEmpty(lists[0].Substring(2).TrimEnd(',')))
                {
                    var roles = lists[0].Substring(2).TrimEnd(',').Split(',');
                    foreach (var role in roles)
                    {
                        int roleId = Int32.Parse(role);
                        RoleInfo userRole = RoleController.Instance.GetRole(portalId, r => r.RoleID == roleId);
                        RoleVisibilities.Add(userRole);
                    }
                }
                if (!String.IsNullOrEmpty(lists[1].Substring(2).TrimEnd(',')))
                {
                    var relationships = lists[1].Substring(2).TrimEnd(',').Split(',');
                    foreach (var relationship in relationships)
                    {
                        Relationship userRelationship = RelationshipController.Instance.GetRelationship(Int32.Parse(relationship));
                        RelationshipVisibilities.Add(userRelationship);
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
                             VisibilityMode = VisibilityMode,
                             RoleVisibilities = new List<RoleInfo>(RoleVisibilities),
                             RelationshipVisibilities = new List<Relationship>(RelationshipVisibilities)
                         };
            return pv;
        }

        public string ExtendedVisibilityString()
        {
            if (VisibilityMode == UserVisibilityMode.FriendsAndGroups)
            {
                var sb = new StringBuilder();
                sb.Append("G:");
                foreach (var role in RoleVisibilities)
                {
                    sb.Append(role.RoleID.ToString(CultureInfo.InvariantCulture) + ",");
                }
                sb.Append(";R:");
                foreach (var relationship in RelationshipVisibilities)
                {
                    sb.Append(relationship.RelationshipId.ToString(CultureInfo.InvariantCulture) + ",");
                }

                return sb.ToString();
            }

            return String.Empty;
        }
    }
}