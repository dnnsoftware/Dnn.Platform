using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Services.Tokens;
namespace DotNetNuke.Modules.Journal.Components {
    public class JournalControl : IPropertyAccess {
        public CacheLevel Cacheability {
            get {
                return CacheLevel.fullyCacheable;
            }
        }
        public string CommentLink { get; set; }
        public string LikeLink { get; set; }
        public string LikeList { get; set; }
        public string CommentArea { get; set; }
        public string AuthorNameLink { get; set; }
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound) {
            string OutputFormat = string.Empty;
            if (format == string.Empty) {
                OutputFormat = "g";
            } else {
                OutputFormat = format;
            }
            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName) {
                case "commentlink":
                    return CommentLink;
                case "likelink":
                    return LikeLink;
                case "likelist":
                    return LikeList;
                case "commentarea":
                    return CommentArea;
                case "authornamelink":
                    return AuthorNameLink;
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}