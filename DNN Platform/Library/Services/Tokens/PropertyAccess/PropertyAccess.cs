// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Globalization;
    using System.Reflection;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// Property Access to Objects using Relection.
    /// </summary>
    /// <remarks></remarks>
    public class PropertyAccess : IPropertyAccess
    {
        private readonly object obj;

        public PropertyAccess(object TokenSource)
        {
            this.obj = TokenSource;
        }

        public static string ContentLocked
        {
            get
            {
                return "*******";
            }
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

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
            return Localization.GetString(strValue, null, formatProvider.ToString());
        }

        /// <summary>
        /// Returns a formatted String if a format is given, otherwise it returns the unchanged value.
        /// </summary>
        /// <param name="value">string to be formatted.</param>
        /// <param name="format">format specification.</param>
        /// <returns>formatted string.</returns>
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
        ///     Returns the localized property of any object as string using reflection.
        /// </summary>
        /// <param name="objObject">Object to access.</param>
        /// <param name="strPropertyName">Name of property.</param>
        /// <param name="strFormat">Format String.</param>
        /// <param name="formatProvider">specify formatting.</param>
        /// <param name="PropertyNotFound">out: specifies, whether property was found.</param>
        /// <returns>Localized Property.</returns>
        /// <remarks></remarks>
        public static string GetObjectProperty(object objObject, string strPropertyName, string strFormat, CultureInfo formatProvider, ref bool PropertyNotFound)
        {
            PropertyInfo objProperty = null;
            PropertyNotFound = false;
            if (CBO.GetProperties(objObject.GetType()).TryGetValue(strPropertyName, out objProperty))
            {
                object propValue = objProperty.GetValue(objObject, null);
                Type t = typeof(string);
                if (propValue != null)
                {
                    switch (objProperty.PropertyType.Name)
                    {
                        case "String":
                            return FormatString(Convert.ToString(propValue), strFormat);
                        case "Boolean":
                            return Boolean2LocalizedYesNo(Convert.ToBoolean(propValue), formatProvider);
                        case "DateTime":
                        case "Double":
                        case "Single":
                        case "Int32":
                        case "Int64":
                            if (strFormat == string.Empty)
                            {
                                strFormat = "g";
                            }

                            return ((IFormattable)propValue).ToString(strFormat, formatProvider);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }

            PropertyNotFound = true;
            return string.Empty;
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            if (this.obj == null)
            {
                return string.Empty;
            }

            return GetObjectProperty(this.obj, propertyName, format, formatProvider, ref PropertyNotFound);
        }
    }
}
