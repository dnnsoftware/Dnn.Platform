// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Web.Services.Description;

    using DotNetNuke.Entities.Users.Social;

    public class RelationshipEventArgs : EventArgs
    {
        internal RelationshipEventArgs(UserRelationship relationship, int portalId)
        {
            this.Relationship = relationship;
            this.PortalID = portalId;
        }

        public UserRelationship Relationship { get; private set; }

        public int PortalID { get; private set; }
    }
}
