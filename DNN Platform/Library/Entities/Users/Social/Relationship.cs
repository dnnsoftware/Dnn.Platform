// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users.Social;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      Relationship
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Relationship class describes the relationships that a user or portal owns.
    /// A handful of default Portal-Level Relationships will be be present for every portal (e.g. Friends, Followers, Family).
    /// Portal-Level Relationship will have a -1 in UserId field.
    /// Any custom User-Level Relationship created by user will also be defined by this class (e.g. My InLaws, Engineering Group).
    /// User-Relationship will always have an associcated PortalId. User-Level Relationship will always be tied to a specific Portal.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class Relationship : BaseEntityInfo, IHydratable
    {
        public Relationship()
        {
            this.RelationshipId = -1;
        }

        /// <summary>
        /// Gets a value indicating whether is this a Portal-Level Relationship.
        /// </summary>
        [XmlIgnore]
        public bool IsPortalList
        {
            get
            {
                return this.UserId == Null.NullInteger && this.PortalId >= 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is this a Host-Level Relationship (very uncommon).
        /// </summary>
        [XmlIgnore]
        public bool IsHostList
        {
            get
            {
                return this.UserId == Null.NullInteger && this.PortalId == Null.NullInteger;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is this a USer-Level Relationship.
        /// </summary>
        [XmlIgnore]
        public bool IsUserList
        {
            get
            {
                return this.UserId > 0 && this.PortalId >= 0;
            }
        }

        /// <summary>
        /// Gets or sets relationshipId - The primary key.
        /// </summary>
        [XmlAttribute]
        public int RelationshipId { get; set; }

        /// <summary>
        /// Gets or sets relationship Name.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets relationship Description.
        /// </summary>
        [XmlAttribute]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets userId of the User that owns the Relationship. A value of -1 indicates that it's a Portal-Level Relationship.
        /// </summary>
        [XmlAttribute]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets portalId of the User that owns the Relationship. A value of -1 in UserID field indicates that it's a Portal-Level Relationship.
        /// </summary>
        [XmlAttribute]
        public int PortalId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the Relationship to which this Relation belongs to (e.g. Friend List or Coworkers).
        /// </summary>
        [XmlAttribute]
        public int RelationshipTypeId { get; set; }

        /// <summary>
        /// Gets or sets default Relationship Status to be provided to any new Relationship Request.
        /// </summary>
        [XmlAttribute]
        public RelationshipStatus DefaultResponse { get; set; }

        /// <summary>
        /// Gets or sets iHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return this.RelationshipId;
            }

            set
            {
                this.RelationshipId = value;
            }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.RelationshipId = Convert.ToInt32(dr["RelationshipID"]);
            this.UserId = Null.SetNullInteger(dr["UserID"]);
            this.PortalId = Null.SetNullInteger(dr["PortalID"]);
            this.Name = dr["Name"].ToString();
            this.Description = dr["Description"].ToString();
            this.DefaultResponse = (RelationshipStatus)Convert.ToInt32(dr["DefaultResponse"]);
            this.RelationshipTypeId = Convert.ToInt32(dr["RelationshipTypeID"]);

            // add audit column data
            this.FillInternal(dr);
        }
    }
}
