// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;

    [Serializable]
    public class Redirection : IRedirection, IHydratable
    {
        private int _id = -1;

        [XmlIgnore]
        private IList<IMatchRule> _matchRules;

        /// <summary>
        /// Gets or sets redirection's primary key.
        /// </summary>
        public int Id
        {
            get
            {
                return this._id;
            }

            set
            {
                this._id = value;
                this._matchRules = null;
            }
        }

        /// <summary>
        /// Gets or sets the portal Redirection is belong to.
        /// </summary>
        [XmlAttribute]
        public int PortalId { get; set; }

        /// <summary>
        /// Gets or sets redirection name.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the redirection's match source tab. if this value is Null.NullInteger(-1) means should redirect when request the whole current portal;
        /// otherwise means this redirection will be available for the specific tab.
        /// </summary>
        [XmlAttribute]
        public int SourceTabId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this value will be available when SourceTabId have a specific value, in that way when this value is true, page will rediect
        /// to target when request source tab and all child tabs under source tab.
        /// </summary>
        [XmlAttribute]
        public bool IncludeChildTabs { get; set; }

        /// <summary>
        /// Gets or sets redirection Type: Mobile, Tablet, Both or Other.
        /// </summary>
        [XmlAttribute]
        public RedirectionType Type { get; set; }

        /// <summary>
        /// Gets or sets when redirection type is RedirectionType.Other, should use this collection to match the request by capability info.
        /// </summary>
        [XmlIgnore]
        public IList<IMatchRule> MatchRules
        {
            get
            {
                if (this._matchRules == null)
                {
                    if (this._id == Null.NullInteger)
                    {
                        this._matchRules = new List<IMatchRule>();
                    }
                    else
                    {
                        // get from database
                        this._matchRules = CBO.FillCollection<MatchRule>(DataProvider.Instance().GetRedirectionRules(this.Id)).Cast<IMatchRule>().ToList();
                    }
                }

                return this._matchRules;
            }

            set
            {
                this._matchRules = value;
            }
        }

        /// <summary>
        /// Gets or sets redirection's target type, should be: Portal, Tab, Url.
        /// </summary>
        [XmlAttribute]
        public TargetType TargetType { get; set; }

        /// <summary>
        /// Gets or sets the redirection's target value, this value will determine by TargetType as:
        /// <list type="bullet">
        ///     <item>TargetType.Portal: this value should be a portal id.</item>
        /// <item>TargetType.Tab: this value should be a tab id.</item>
        /// <item>TargetType.Url: this value should be a valid url.</item>
        /// </list>
        /// </summary>
        [XmlAttribute]
        public object TargetValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether this redirection is available.
        /// </summary>
        [XmlAttribute]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets redirection's piority.
        /// </summary>
        [XmlAttribute]
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets iHydratable.KeyID.
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
