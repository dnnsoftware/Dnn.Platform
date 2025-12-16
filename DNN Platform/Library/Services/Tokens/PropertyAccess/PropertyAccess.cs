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

    /// <summary>Property Access to Objects using Reflection.</summary>
    public class PropertyAccess : IPropertyAccess
    {
        private readonly object obj;

        /// <summary>Initializes a new instance of the <see cref="PropertyAccess"/> class.</summary>
        /// <param name="tokenSource">The object to use as the source for the tokens.</param>
        public PropertyAccess(object tokenSource)
        {
            this.obj = tokenSource;
        }

        public static string ContentLocked
        {
            get
            {
                return "*******";
            }
        }

        /// <inheritdoc/>
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        /// <summary>Boolean2LocalizedYesNo returns the translated string for "yes" or "no" against a given boolean value.</summary>
        /// <param name="value">The value to localize.</param>
        /// <param name="formatProvider">The culture info.</param>
        /// <returns><c>"Yes"</c> or <c>"No"</c> localized into the given culture.</returns>
        public static string Boolean2LocalizedYesNo(bool value, CultureInfo formatProvider)
        {
            string strValue = Convert.ToString(value ? "Yes" : "No");
            return Localization.GetString(strValue, null, formatProvider.ToString());
        }

        /// <summary>Returns a formatted String if a format is given, otherwise it returns the unchanged value.</summary>
        /// <param name="value">string to be formatted.</param>
        /// <param name="format">format specification.</param>
        /// <returns>formatted string.</returns>
        public static string FormatString(string value, string format)
        {
            if (format.Trim() == string.Empty)
            {
                return value;
            }
            else if (value != string.Empty)
            {
                return string.Format(CultureInfo.CurrentCulture, format, value);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>Returns the localized property of any object as string using reflection.</summary>
        /// <param name="objObject">Object to access.</param>
        /// <param name="strPropertyName">Name of property.</param>
        /// <param name="strFormat">Format String.</param>
        /// <param name="formatProvider">specify formatting.</param>
        /// <param name="propertyNotFound">out: specifies, whether property was found.</param>
        /// <returns>Localized Property.</returns>
        public static string GetObjectProperty(object objObject, string strPropertyName, string strFormat, CultureInfo formatProvider, ref bool propertyNotFound)
        {
            propertyNotFound = false;
            if (CBO.GetProperties(objObject.GetType()).TryGetValue(strPropertyName, out var objProperty))
            {
                object propValue = objProperty.GetValue(objObject, null);
                Type t = typeof(string);
                if (propValue != null)
                {
                    switch (objProperty.PropertyType.Name)
                    {
                        case "String":
                            return FormatString(Convert.ToString(propValue, CultureInfo.InvariantCulture), strFormat);
                        case "Boolean":
                            return Boolean2LocalizedYesNo(Convert.ToBoolean(propValue, CultureInfo.InvariantCulture), formatProvider);
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

            propertyNotFound = true;
            return string.Empty;
        }

        /// <inheritdoc/>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            if (this.obj == null)
            {
                return string.Empty;
            }

            return GetObjectProperty(this.obj, propertyName, format, formatProvider, ref propertyNotFound);
        }
    }
}
