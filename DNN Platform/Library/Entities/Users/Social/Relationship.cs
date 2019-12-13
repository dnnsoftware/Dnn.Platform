// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users.Social;

#endregion

namespace DotNetNuke.Entities.Users.Social
{
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
            RelationshipId = -1;
        }

        /// <summary>
        /// RelationshipId - The primary key
        /// </summary>
        [XmlAttribute]
        public int RelationshipId { get; set; }

        /// <summary>
        /// Relationship Name.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Relationship Description.
        /// </summary>
        [XmlAttribute]
        public string Description { get; set; }

        /// <summary>
        /// UserId of the User that owns the Relationship. A value of -1 indicates that it's a Portal-Level Relationship
        /// </summary>
        [XmlAttribute]
        public int UserId { get; set; }

        /// <summary>
        /// PortalId of the User that owns the Relationship. A value of -1 in UserID field indicates that it's a Portal-Level Relationship
        /// </summary>
        [XmlAttribute]
        public int PortalId { get; set; }

        /// <summary>
        /// The ID of the Relationship to which this Relation belongs to (e.g. Friend List or Coworkers)
        /// </summary>
        [XmlAttribute]
        public int RelationshipTypeId { get; set; }

        /// <summary>
        /// Default Relationship Status to be provided to any new Relationship Request
        /// </summary>
        [XmlAttribute]
        public RelationshipStatus DefaultResponse { get; set; }

        /// <summary>
        /// Is this a Portal-Level Relationship
        /// </summary>
        [XmlIgnore]
        public bool IsPortalList 
        { 
            get
            {
                return UserId == Null.NullInteger && PortalId >= 0;
            }
        }

        /// <summary>
        /// Is this a Host-Level Relationship (very uncommon)
        /// </summary>
        [XmlIgnore]
        public bool IsHostList
        {
            get
            {
                return UserId == Null.NullInteger && PortalId == Null.NullInteger;
            }
        }

        /// <summary>
        /// Is this a USer-Level Relationship 
        /// </summary>
        [XmlIgnore]
        public bool IsUserList
        {
            get
            {
                return UserId > 0 && PortalId >= 0;
            }
        }
        /// <summary>
        /// IHydratable.KeyID.
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

            //add audit column data
            FillInternal(dr);
        }
    }
}
