#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
