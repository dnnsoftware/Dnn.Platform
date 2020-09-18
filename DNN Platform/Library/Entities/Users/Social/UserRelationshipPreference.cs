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
    /// Class:      UserRelationshipPreference
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserRelationshipPreference class defines the relationship preference per user
    /// The user initiating the relationship is UserId.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserRelationshipPreference : BaseEntityInfo, IHydratable
    {
        public UserRelationshipPreference()
        {
            this.PreferenceId = -1;
        }

        /// <summary>
        /// Gets or sets preferenceId - The primary key.
        /// </summary>
        [XmlAttribute]
        public int PreferenceId { get; set; }

        /// <summary>
        /// Gets or sets userId of the User that owns the relationship.
        /// </summary>
        [XmlAttribute]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the Relationship to which this Relation belongs to (e.g. Friend List or Coworkers).
        /// </summary>
        [XmlAttribute]
        public int RelationshipId { get; set; }

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
                return this.PreferenceId;
            }

            set
            {
                this.PreferenceId = value;
            }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.PreferenceId = Convert.ToInt32(dr["PreferenceID"]);
            this.UserId = Convert.ToInt32(dr["UserID"]);
            this.RelationshipId = Convert.ToInt32(dr["RelationshipID"]);
            this.DefaultResponse = (RelationshipStatus)Convert.ToInt32(dr["DefaultResponse"]);

            // add audit column data
            this.FillInternal(dr);
        }
    }
}
