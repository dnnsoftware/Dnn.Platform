#region Usings

using System;
using System.Collections;
using System.Globalization;

using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Services.Tokens
{
    public class DictionaryPropertyAccess : IPropertyAccess
    {
        private readonly IDictionary NameValueCollection;

        public DictionaryPropertyAccess(IDictionary list)
        {
            NameValueCollection = list;
        }

        #region IPropertyAccess Members

        public virtual string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            if (NameValueCollection == null)
            {
                return string.Empty;
            }
            object valueObject = NameValueCollection[propertyName];
            string OutputFormat = format;
            if (string.IsNullOrEmpty(format))
            {
                OutputFormat = "g";
            }
            if (valueObject != null)
            {
                switch (valueObject.GetType().Name)
                {
                    case "String":
                        return PropertyAccess.FormatString(Convert.ToString(valueObject), format);
                    case "Boolean":
                        return (PropertyAccess.Boolean2LocalizedYesNo(Convert.ToBoolean(valueObject), formatProvider));
                    case "DateTime":
                    case "Double":
                    case "Single":
                    case "Int32":
                    case "Int64":
                        return (((IFormattable) valueObject).ToString(OutputFormat, formatProvider));
                    default:
                        return PropertyAccess.FormatString(valueObject.ToString(), format);
                }
            }
            else
            {
                PropertyNotFound = true;
                return string.Empty;
            }
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        #endregion
    }
}
