// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Entities.Modules;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      RelationshipType
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The RelationshipType defines the core relationship types (Friend (2-way), Follower (1-way)).
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class RelationshipType : BaseEntityInfo, IHydratable
    {
        private int _relationshipTypeId = -1;

        /// <summary>
        /// Gets or sets relationshipId - The primary key.
        /// </summary>
        [XmlAttribute]
        public int RelationshipTypeId
        {
            get
            {
                return this._relationshipTypeId;
            }

            set
            {
                this._relationshipTypeId = value;
            }
        }

        /// <summary>
        /// Gets or sets relationship Type Name.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets relationship Description.
        /// </summary>
        [XmlAttribute]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets relationship Direction.
        /// </summary>
        [XmlAttribute]
        public RelationshipDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets iHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return this.RelationshipTypeId;
            }

            set
            {
                this.RelationshipTypeId = value;
            }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.RelationshipTypeId = Convert.ToInt32(dr["RelationshipTypeID"]);
            this.Name = dr["Name"].ToString();
            this.Description = dr["Description"].ToString();
            this.Direction = (RelationshipDirection)Convert.ToInt32(dr["Direction"]);

            // add audit column data
            this.FillInternal(dr);
        }
    }
}
