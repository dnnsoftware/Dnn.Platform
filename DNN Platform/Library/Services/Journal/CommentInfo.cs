using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;


namespace DotNetNuke.Services.Journal {
    public class CommentInfo : IHydratable, IPropertyAccess {
        public int CommentId { get; set; }
        public int JournalId { get; set; }
        public string Comment { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public XmlDocument CommentXML { get; set; }
        public int UserId { get; set; }
        public string DisplayName { get; set; }


        public int KeyID {
            get {
                return CommentId;
            }
            set {
                CommentId = value;
            }
        }
       
        public void Fill(System.Data.IDataReader dr) {
            CommentId = Null.SetNullInteger(dr["CommentId"]);
            JournalId = Null.SetNullInteger(dr["JournalId"]);
            Comment = Null.SetNullString(dr["Comment"]);
            DateCreated = Null.SetNullDateTime(dr["DateCreated"]);
            DateUpdated = Null.SetNullDateTime(dr["DateUpdated"]);
            if (!String.IsNullOrEmpty(Null.SetNullString(dr["CommentXML"]))) {
                CommentXML = new XmlDocument();
                CommentXML.LoadXml(dr["CommentXML"].ToString());
            }
            UserId = Null.SetNullInteger(dr["UserId"]);
            DisplayName = Null.SetNullString(dr["DisplayName"]);


        }

        public CacheLevel Cacheability {
            get {
                return CacheLevel.fullyCacheable;
            }
            
        }

        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound) {
            throw new NotImplementedException();
        }
    }
}
