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
