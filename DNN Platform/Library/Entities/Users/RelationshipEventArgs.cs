// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
