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

namespace DotNetNuke.Entities.Users
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      RelationshipType
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The RelationshipType defines the core relationship types (Friend (2-way), Follower (1-way))
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class RelationshipType : BaseEntityInfo, IHydratable
    {
        private int _relationshipTypeId = -1;

        /// <summary>
        /// RelationshipId - The primary key
        /// </summary>
        [XmlAttribute]
        public int RelationshipTypeId
        {
            get
            {
                return _relationshipTypeId;
            }
            set
            {
                _relationshipTypeId = value;
            }
        }

        /// <summary>
        /// Relationship Type Name.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Relationship Description.
        /// </summary>
        [XmlAttribute]
        public string Description { get; set; }

        /// <summary>
        /// Relationship Direction.
        /// </summary>
        [XmlAttribute]
        public RelationshipDirection Direction { get; set; }

        /// <summary>
        /// IHydratable.KeyID.
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

            //add audit column data
            FillInternal(dr);
        }
    }
}
