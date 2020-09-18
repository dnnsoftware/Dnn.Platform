// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    [Serializable]
    public class MatchRule : IMatchRule, IHydratable
    {
        private int _id = -1;

        /// <summary>
        /// Gets or sets match rule's primary key.
        /// </summary>
        [XmlAttribute]
        public int Id
        {
            get
            {
                return this._id;
            }

            set
            {
                this._id = value;
            }
        }

        /// <summary>
        /// Gets or sets capability's name.
        /// </summary>
        [XmlAttribute]
        public string Capability
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value to match the capability from request.
        /// </summary>
        [XmlAttribute]
        public string Expression
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets iHydratable.KeyID.
        /// </summary>
        [XmlAttribute]
        public int KeyID
        {
            get
            {
                return this.Id;
            }

            set
            {
                this.Id = value;
            }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(System.Data.IDataReader dr)
        {
            this.Id = Convert.ToInt32(dr["Id"]);
            this.Capability = dr["Capability"].ToString();
            this.Expression = dr["Expression"].ToString();
        }
    }
}
