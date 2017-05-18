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
using System.Globalization;
using System.Reflection;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Services.Tokens
{
    /// <summary>
    /// Property Access to Objects using Relection
    /// </summary>
    /// <remarks></remarks>
    public class PropertyAccess : IPropertyAccess
    {
        private readonly object obj;

        public PropertyAccess(object TokenSource)
        {
            obj = TokenSource;
        }

        public static string ContentLocked
        {
            get
            {
                return "*******";
            }
        }

        #region IPropertyAccess Members

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return GetObjectProperty(obj, propertyName, format, formatProvider, ref PropertyNotFound);
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        #endregion

        /// <summary>
        /// Boolean2LocalizedYesNo returns the translated string for "yes" or "no" against a given boolean value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Boolean2LocalizedYesNo(bool value, CultureInfo formatProvider)
        {
            string strValue = Convert.ToString(value ? "Yes" : "No");
            return Localization.Localization.GetString(strValue, null, formatProvider.ToString());
        }

        /// <summary>
        /// Returns a formatted String if a format is given, otherwise it returns the unchanged value.
        /// </summary>
        /// <param name="value">string to be formatted</param>
        /// <param name="format">format specification</param>
        /// <returns>formatted string</returns>
        /// <remarks></remarks>
        public static string FormatString(string value, string format)
        {
            if (format.Trim() == string.Empty)
            {
                return value;
            }
            else if (value != string.Empty)
            {
                return string.Format(format, value);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     Returns the localized property of any object as string using reflection
        /// </summary>
        /// <param name="objObject">Object to access</param>
        /// <param name="strPropertyName">Name of property</param>
        /// <param name="strFormat">Format String</param>
        /// <param name="formatProvider">specify formatting</param>
        /// <param name="PropertyNotFound">out: specifies, whether property was found</param>
        /// <returns>Localized Property</returns>
        /// <remarks></remarks>
        public static string GetObjectProperty(object objObject, string strPropertyName, string strFormat, CultureInfo formatProvider, ref bool PropertyNotFound)
        {
            PropertyInfo objProperty = null;
            PropertyNotFound = false;
            if (CBO.GetProperties(objObject.GetType()).TryGetValue(strPropertyName, out objProperty))
            {
                object propValue = objProperty.GetValue(objObject, null);
                Type t = typeof (string);
                if (propValue != null)
                {
                    switch (objProperty.PropertyType.Name)
                    {
                        case "String":
                            return FormatString(Convert.ToString(propValue), strFormat);
                        case "Boolean":
                            return (Boolean2LocalizedYesNo(Convert.ToBoolean(propValue), formatProvider));
                        case "DateTime":
                        case "Double":
                        case "Single":
                        case "Int32":
                        case "Int64":
                            if (strFormat == string.Empty)
                            {
                                strFormat = "g";
                            }
                            return (((IFormattable) propValue).ToString(strFormat, formatProvider));
                    }
                }
                else
                {
                    return "";
                }
            }
            PropertyNotFound = true;
            return string.Empty;
        }
    }
}
