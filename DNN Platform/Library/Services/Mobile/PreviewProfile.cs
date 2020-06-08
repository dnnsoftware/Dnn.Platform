// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Mobile
{
	[Serializable]
	public class PreviewProfile : IPreviewProfile, IHydratable
	{
		private int _id = -1;

		/// <summary>
		/// Primary key.
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
		/// the profiles' effected portal.
		/// </summary>
		[XmlAttribute]
		public int PortalId
		{
			get;
			set;
		}

		/// <summary>
		/// profile's name.
		/// </summary>
		[XmlAttribute]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// the preview device's width.
		/// </summary>
		[XmlAttribute]
		public int Width { get; set; }

		/// <summary>
		/// the preview device's height.
		/// </summary>
		[XmlAttribute]
		public int Height { get; set; }

		/// <summary>
		/// the preview device's user agent.
		/// </summary>
		[XmlAttribute]
		public string UserAgent { get; set; }

		/// <summary>
		/// Profile's sort order.
		/// </summary>
		[XmlAttribute]
		public int SortOrder { get; set; }

		/// <summary>
		/// IHydratable.KeyID.
		/// </summary>
		[XmlIgnore]
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
		public void Fill(IDataReader dr)
		{
			this.Id = Convert.ToInt32(dr["Id"]);
			this.PortalId = Convert.ToInt32(dr["PortalId"]);
			this.Name = dr["Name"].ToString();
			this.Width = Convert.ToInt32(dr["Width"]);
			this.Height = Convert.ToInt32(dr["Height"]);
			this.UserAgent = dr["UserAgent"].ToString();
			this.SortOrder = Convert.ToInt32(dr["SortOrder"]);
		}
	}
}
