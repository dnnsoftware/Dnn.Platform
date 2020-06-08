// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Web.Services.Description;
using DotNetNuke.Entities.Users.Social;
using System;

namespace DotNetNuke.Entities.Users
{
    public class RelationshipEventArgs : EventArgs
    {
        internal RelationshipEventArgs(UserRelationship relationship, int portalId)
        {
            Relationship = relationship;
            PortalID = portalId;
        }

        public UserRelationship Relationship { get; private set; }
        public int PortalID { get; private set; }
    }
}
