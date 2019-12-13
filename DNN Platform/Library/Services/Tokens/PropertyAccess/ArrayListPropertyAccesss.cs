#region Usings

using System;
using System.Collections;
using System.Globalization;

using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Services.Tokens
{
    public class ArrayListPropertyAccess : IPropertyAccess
    {
        private readonly ArrayList custom;

        public ArrayListPropertyAccess(ArrayList list)
        {
            custom = list;
        }

        #region IPropertyAccess Members

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            if (custom == null)
            {
                return string.Empty;
            }
            object valueObject = null;
            string OutputFormat = format;
            if (string.IsNullOrEmpty(format))
            {
                OutputFormat = "g";
            }
            int intIndex = int.Parse(propertyName);
            if ((custom != null) && custom.Count > intIndex)
            {
                valueObject = custom[intIndex].ToString();
            }
            if ((valueObject != null))
            {
                switch (valueObject.GetType().Name)
                {
                    case "String":
                        return PropertyAccess.FormatString((string) valueObject, format);
                    case "Boolean":
                        return (PropertyAccess.Boolean2LocalizedYesNo((bool) valueObject, formatProvider));
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
