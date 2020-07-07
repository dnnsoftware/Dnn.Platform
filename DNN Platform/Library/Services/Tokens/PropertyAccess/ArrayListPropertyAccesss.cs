// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Collections;
    using System.Globalization;

    using DotNetNuke.Entities.Users;

    public class ArrayListPropertyAccess : IPropertyAccess
    {
        private readonly ArrayList custom;

        public ArrayListPropertyAccess(ArrayList list)
        {
            this.custom = list;
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            if (this.custom == null)
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
            if ((this.custom != null) && this.custom.Count > intIndex)
            {
                valueObject = this.custom[intIndex].ToString();
            }

            if (valueObject != null)
            {
                switch (valueObject.GetType().Name)
                {
                    case "String":
                        return PropertyAccess.FormatString((string)valueObject, format);
                    case "Boolean":
                        return PropertyAccess.Boolean2LocalizedYesNo((bool)valueObject, formatProvider);
                    case "DateTime":
                    case "Double":
                    case "Single":
                    case "Int32":
                    case "Int64":
                        return ((IFormattable)valueObject).ToString(OutputFormat, formatProvider);
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
    }
}
