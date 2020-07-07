// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Entities.Modules;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserRelationship
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserRelationship class defines the membership of the relationship.
    /// The user initiating the relationship is UserId.
    /// The target of the relationship is RelatedUserId.
    /// Status tracks relationship status as Initiated, Approved, Rejected etc.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserRelationship : BaseEntityInfo, IHydratable
    {
        public UserRelationship()
        {
            this.UserRelationshipId = -1;
        }

        /// <summary>
        /// Gets or sets userRelationshipId - The primary key.
        /// </summary>
        [XmlAttribute]
        public int UserRelationshipId { get; set; }

        /// <summary>
        /// Gets or sets userId of the User that owns the relationship.
        /// </summary>
        [XmlAttribute]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the UserId of the Related User.
        /// </summary>
        [XmlAttribute]
        public int RelatedUserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the Relationship to which this Relation belongs to (e.g. Friend List or Coworkers).
        /// </summary>
        [XmlAttribute]
        public int RelationshipId { get; set; }

        /// <summary>
        /// Gets or sets the Status of the Relationship (e.g. Initiated, Accepted, Rejected).
        /// </summary>
        [XmlAttribute]
        public RelationshipStatus Status { get; set; }

        /// <summary>
        /// Gets or sets iHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return this.UserRelationshipId;
            }

            set
            {
                this.UserRelationshipId = value;
            }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.UserRelationshipId = Convert.ToInt32(dr["UserRelationshipID"]);
            this.UserId = Convert.ToInt32(dr["UserID"]);
            this.RelatedUserId = Convert.ToInt32(dr["RelatedUserID"]);
            this.RelationshipId = Convert.ToInt32(dr["RelationshipID"]);
            this.Status = (RelationshipStatus)Convert.ToInt32(dr["Status"]);

            // add audit column data
            this.FillInternal(dr);
        }
    }
}
