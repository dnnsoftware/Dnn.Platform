// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Mobile
{
    [Serializable]
	public class MatchRule : IMatchRule, IHydratable
	{
		private int _id = -1;

		/// <summary>
		/// Match rule's primary key.
		/// </summary>
        [XmlAttribute]
		public int Id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		/// <summary>
		/// Capability's name.
		/// </summary>
        [XmlAttribute]
        public string Capability
		{
			get;
			set;
		}

		/// <summary>
		/// The value to match the capability from request.
		/// </summary>
        [XmlAttribute]
        public string Expression
		{
			get;
			set;
		}

		/// <summary>
		/// IHydratable.KeyID.
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
