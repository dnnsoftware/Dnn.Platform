// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Journal
{
    using System;
    using System.Data;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Web.Script.Serialization;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Tokens;

    public class JournalItem : IHydratable, IPropertyAccess
    {
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public int JournalId { get; set; }

        public int JournalTypeId { get; set; }

        public int PortalId { get; set; }

        public int UserId { get; set; }

        public int ProfileId { get; set; }

        public int SocialGroupId { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public string Body { get; set; }

        public ItemData ItemData { get; set; }

        public XmlDocument JournalXML { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public string ObjectKey { get; set; }

        public Guid AccessKey { get; set; }

        public string SecuritySet { get; set; }

        public int ContentItemId { get; set; }

        public JournalEntity JournalAuthor { get; set; }

        public JournalEntity JournalOwner { get; set; }

        public string TimeFrame { get; set; }

        public bool CurrentUserLikes { get; set; }

        public string JournalType { get; set; }

        public bool IsDeleted { get; set; }

        public bool CommentsDisabled { get; set; }

        public bool CommentsHidden { get; set; }

        public int SimilarCount { get; set; }

        /// <summary>
        /// Gets or sets the key ID.
        /// </summary>
        /// <value>
        /// The key ID.
        /// </value>
        /// <remarks>
        /// If you derive class has its own key id, please override this property and set the value to your own key id.
        /// </remarks>
        [XmlIgnore]
        public virtual int KeyID
        {
            get
            {
                return this.JournalId;
            }

            set
            {
                this.JournalId = value;
            }
        }

        public void Fill(IDataReader dr)
        {
            this.JournalId = Null.SetNullInteger(dr["JournalId"]);
            this.JournalTypeId = Null.SetNullInteger(dr["JournalTypeId"]);
            this.PortalId = Null.SetNullInteger(dr["PortalId"]);
            this.UserId = Null.SetNullInteger(dr["UserId"]);
            this.ProfileId = Null.SetNullInteger(dr["ProfileId"]);
            this.SocialGroupId = Null.SetNullInteger(dr["GroupId"]);
            if (!string.IsNullOrEmpty(Null.SetNullString(dr["JournalXML"])))
            {
                this.JournalXML = new XmlDocument { XmlResolver = null };
                this.JournalXML.LoadXml(dr["JournalXML"].ToString());
                XmlNode xRoot = this.JournalXML.DocumentElement;
                XmlNode xNode = xRoot.SelectSingleNode("//items/item/body");
                if (xNode != null)
                {
                    this.Body = xNode.InnerText;
                }
            }

            this.DateCreated = Null.SetNullDateTime(dr["DateCreated"]);
            this.DateUpdated = Null.SetNullDateTime(dr["DateUpdated"]);
            this.ObjectKey = Null.SetNullString(dr["ObjectKey"]);
            this.AccessKey = Null.SetNullGuid(dr["AccessKey"]);
            this.Title = Null.SetNullString(dr["Title"]);
            this.Summary = Null.SetNullString(dr["Summary"]);
            string itemd = Null.SetNullString(dr["ItemData"]);
            this.ItemData = new ItemData();
            if (!string.IsNullOrEmpty(itemd))
            {
                this.ItemData = itemd.FromJson<ItemData>();
            }

            this.ContentItemId = Null.SetNullInteger(dr["ContentItemId"]);
            this.JournalAuthor = new JournalEntity(dr["JournalAuthor"].ToString());
            this.JournalOwner = new JournalEntity(dr["JournalOwner"].ToString());
            this.JournalType = Null.SetNullString(dr["JournalType"]);

            this.IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]);
            this.CommentsDisabled = Null.SetNullBoolean(dr["CommentsDisabled"]);
            this.CommentsHidden = Null.SetNullBoolean(dr["CommentsHidden"]);
            this.SimilarCount = Null.SetNullInteger(dr["SimilarCount"]);
        }

        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            string OutputFormat = string.Empty;
            if (format == string.Empty)
            {
                OutputFormat = "g";
            }
            else
            {
                OutputFormat = format;
            }

            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
            {
                case "journalid":
                    return PropertyAccess.FormatString(this.JournalId.ToString(), format);
                case "journaltypeid":
                    return PropertyAccess.FormatString(this.JournalTypeId.ToString(), format);
                case "profileid":
                    return PropertyAccess.FormatString(this.ProfileId.ToString(), format);
                case "socialgroupid":
                    return PropertyAccess.FormatString(this.SocialGroupId.ToString(), format);
                case "datecreated":
                    return PropertyAccess.FormatString(this.DateCreated.ToString(), format);
                case "title":
                    return PropertyAccess.FormatString(this.Title, format);
                case "summary":
                    return PropertyAccess.FormatString(this.Summary, format);
                case "body":
                    return PropertyAccess.FormatString(this.Body, format);
                case "timeframe":
                    return PropertyAccess.FormatString(this.TimeFrame, format);
                case "isdeleted":
                    return this.IsDeleted.ToString();
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
