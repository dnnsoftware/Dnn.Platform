#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using System.Xml;
using System.Xml.Serialization;
using DotNetNuke.Services.Tokens;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

#endregion

namespace DotNetNuke.Services.Journal {
   public class JournalItem : IHydratable, IPropertyAccess {
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
        public virtual int KeyID {
            get {
                return JournalId;
            }
            set {
                JournalId = value;
            }
        }

        public void Fill(IDataReader dr) {
            JournalId = Null.SetNullInteger(dr["JournalId"]);
            JournalTypeId = Null.SetNullInteger(dr["JournalTypeId"]);
            PortalId = Null.SetNullInteger(dr["PortalId"]);
            UserId = Null.SetNullInteger(dr["UserId"]);
            ProfileId = Null.SetNullInteger(dr["ProfileId"]);
            SocialGroupId = Null.SetNullInteger(dr["GroupId"]);
            if (!String.IsNullOrEmpty(Null.SetNullString(dr["JournalXML"]))) {
                JournalXML = new XmlDocument();
                JournalXML.LoadXml(dr["JournalXML"].ToString());
                XmlNode xRoot = JournalXML.DocumentElement;
                XmlNode xNode = xRoot.SelectSingleNode("//items/item/body");
                if (xNode != null) {
                    Body = xNode.InnerText;
                }
            }
            DateCreated = Null.SetNullDateTime(dr["DateCreated"]);
            DateUpdated = Null.SetNullDateTime(dr["DateUpdated"]);
            ObjectKey = Null.SetNullString(dr["ObjectKey"]);
            AccessKey = Null.SetNullGuid(dr["AccessKey"]);
            Title = Null.SetNullString(dr["Title"]);
            Summary = Null.SetNullString(dr["Summary"]);
            string itemd = Null.SetNullString(dr["ItemData"]);
            ItemData = new ItemData();
            if (!string.IsNullOrEmpty(itemd)) {
                ItemData = itemd.FromJson<ItemData>();
            }
            ContentItemId = Null.SetNullInteger(dr["ContentItemId"]);
            JournalAuthor = new JournalEntity(dr["JournalAuthor"].ToString());
            JournalOwner = new JournalEntity(dr["JournalOwner"].ToString());
            JournalType = Null.SetNullString(dr["JournalType"]);

            IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]);
            CommentsDisabled = Null.SetNullBoolean(dr["CommentsDisabled"]);
            CommentsHidden = Null.SetNullBoolean(dr["CommentsHidden"]);
            SimilarCount = Null.SetNullInteger(dr["SimilarCount"]);
        }
        public CacheLevel Cacheability {
            get {
                return CacheLevel.fullyCacheable;
            }
        }

        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound) {
            string OutputFormat = string.Empty;
            if (format == string.Empty) {
                OutputFormat = "g";
            } else {
                OutputFormat = format;
            }
            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName) {
                case "journalid":
                    return PropertyAccess.FormatString(JournalId.ToString(), format);
                case "journaltypeid":
                    return PropertyAccess.FormatString(JournalTypeId.ToString(), format);
                case "profileid":
                    return PropertyAccess.FormatString(ProfileId.ToString(), format);
                case "socialgroupid":
                    return PropertyAccess.FormatString(SocialGroupId.ToString(), format);
                case "datecreated":
                    return PropertyAccess.FormatString(DateCreated.ToString(), format);
                case "title":
                    return PropertyAccess.FormatString(Title, format);
                case "summary":
                    return PropertyAccess.FormatString(Summary, format);
                case "body":
                    return PropertyAccess.FormatString(Body, format);
                case "timeframe":
                    return PropertyAccess.FormatString(TimeFrame, format);
                case "isdeleted":
                    return IsDeleted.ToString();
            }

            propertyNotFound = true;
            return string.Empty;

        }
    }
}
