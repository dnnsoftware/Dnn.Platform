using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Dnn.Modules.DynamicContentManager.Components
{
    public class FontPropertyAccess : IPropertyAccess
    {
        public FontPropertyAccess()
        {
        }

        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.fullyCacheable; }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            var rootName = "dnni dnni-{0}";

            return string.Format(rootName, propertyName.ToLower());
        }
    }
}