// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Data;
using System.Xml.Serialization;
using DotNetNuke.Entities.Modules;
#endregion

namespace DotNetNuke.Entities.Users.Social
{
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
            PreferenceId = -1;
        }

        /// <summary>
        /// PreferenceId - The primary key
        /// </summary>
        [XmlAttribute]
        public int PreferenceId { get; set; }

        /// <summary>
        /// UserId of the User that owns the relationship
        /// </summary>
        [XmlAttribute]
        public int UserId { get; set; }

        /// <summary>
        /// The ID of the Relationship to which this Relation belongs to (e.g. Friend List or Coworkers)
        /// </summary>
        [XmlAttribute]
        public int RelationshipId { get; set; }

        /// <summary>
        /// Default Relationship Status to be provided to any new Relationship Request
        /// </summary>
        [XmlAttribute]
        public RelationshipStatus DefaultResponse { get; set; }

        /// <summary>
        /// IHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return PreferenceId;
            }
            set
            {
                PreferenceId = value;
            }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            PreferenceId = Convert.ToInt32(dr["PreferenceID"]);
            UserId = Convert.ToInt32(dr["UserID"]);            
            RelationshipId = Convert.ToInt32(dr["RelationshipID"]);
            DefaultResponse = (RelationshipStatus)Convert.ToInt32(dr["DefaultResponse"]);

            //add audit column data
            FillInternal(dr);
        }
    }
}
