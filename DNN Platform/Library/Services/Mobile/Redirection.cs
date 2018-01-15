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
using System.Linq;
using System.ComponentModel;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Mobile
{
    [Serializable]
	public class Redirection : IRedirection, IHydratable
	{
		private int _id = -1;

		/// <summary>
		/// Redirection's primary key.
		/// </summary>
		public int Id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
				_matchRules = null;
			}
		}

        /// <summary>
		/// The portal Redirection is belong to.
		/// </summary>
        [XmlAttribute]
        public int PortalId { get; set; }
        
        /// <summary>
		/// Redirection name.
		/// </summary>
        [XmlAttribute]
        public string Name { get; set; }
        
		/// <summary>
		/// The redirection's match source tab. if this value is Null.NullInteger(-1) means should redirect when request the whole current portal;
		/// otherwise means this redirection will be available for the specific tab.
		/// </summary>
        [XmlAttribute]
		public int SourceTabId { get; set; }

		/// <summary>
		/// This value will be available when SourceTabId have a specific value, in that way when this value is true, page will rediect
		/// to target when request source tab and all child tabs under source tab.
		/// </summary>
		[XmlAttribute]
		public bool IncludeChildTabs { get; set; }
                
		/// <summary>
		/// Redirection Type: Mobile, Tablet, Both or Other.
		/// </summary>
        [XmlAttribute]
        public RedirectionType Type { get; set; }
        
        [XmlIgnore]
		private IList<IMatchRule> _matchRules;

		/// <summary>
		/// When redirection type is RedirectionType.Other, should use this collection to match the request by capability info.
		/// </summary>        
        [XmlIgnore]
		public IList<IMatchRule> MatchRules
		{
			get
			{
				if (_matchRules == null)
				{
					if (_id == Null.NullInteger)
					{
						_matchRules = new List<IMatchRule>();
					}
					else
					{
						//get from database
						_matchRules = CBO.FillCollection<MatchRule>(DataProvider.Instance().GetRedirectionRules(this.Id)).Cast<IMatchRule>().ToList();
					}
				}

				return _matchRules;
			}
			set
			{
				_matchRules = value;
			}
		}

		/// <summary>
		/// Redirection's target type, should be: Portal, Tab, Url
		/// </summary>
        [XmlAttribute]
		public TargetType TargetType { get; set; }
        
		/// <summary>
		/// the redirection's target value, this value will determine by TargetType as:
		/// <list type="bullet">
		///	<item>TargetType.Portal: this value should be a portal id.</item>
		/// <item>TargetType.Tab: this value should be a tab id.</item>
		/// <item>TargetType.Url: this value should be a valid url.</item>
		/// </list>
		/// </summary>
        [XmlAttribute]
        public object TargetValue { get; set; }
        
		/// <summary>
		/// Whether this redirection is available.
		/// </summary>
        [XmlAttribute]
		public bool Enabled { get; set; }
        
		/// <summary>
		/// Redirection's piority.
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
		public void Fill(System.Data.IDataReader dr)
		{
			this.Id = Convert.ToInt32(dr["Id"]);
			this.PortalId = Convert.ToInt32(dr["PortalId"]);
			this.Name = dr["Name"].ToString();
			this.Type = (RedirectionType)Convert.ToInt32(dr["Type"]);
			this.SourceTabId = Convert.ToInt32(dr["SourceTabId"]);
			this.IncludeChildTabs = Convert.ToBoolean(dr["IncludeChildTabs"]);
			this.SortOrder = Convert.ToInt32(dr["SortOrder"]);
			this.TargetType = (TargetType)Convert.ToInt32(dr["TargetType"]);
			this.TargetValue = dr["TargetValue"];
			this.Enabled = Convert.ToBoolean(dr["Enabled"]);
		}
	}
}
