// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Data;
    using System.Globalization;

    using DotNetNuke.Entities.Users;

    public class DataRowPropertyAccess : IPropertyAccess
    {
        private readonly DataRow dr;

        public DataRowPropertyAccess(DataRow row)
        {
            this.dr = row;
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
            if (this.dr == null)
            {
                return string.Empty;
            }

            object valueObject = this.dr[propertyName];
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
                        return PropertyAccess.Boolean2LocalizedYesNo(Convert.ToBoolean(valueObject), formatProvider);
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
