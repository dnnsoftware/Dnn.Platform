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

        /// <summary>Initializes a new instance of the <see cref="DataRowPropertyAccess"/> class.</summary>
        /// <param name="row">The data row with token values.</param>
        public DataRowPropertyAccess(DataRow row)
        {
            this.dr = row;
        }

        /// <inheritdoc/>
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        /// <inheritdoc/>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            if (this.dr == null)
            {
                return string.Empty;
            }

            object valueObject = this.dr[propertyName];
            string outputFormat = format;
            if (string.IsNullOrEmpty(format))
            {
                outputFormat = "g";
            }

            if (valueObject != null)
            {
                switch (valueObject.GetType().Name)
                {
                    case "String":
                        return PropertyAccess.FormatString(Convert.ToString(valueObject, CultureInfo.InvariantCulture), format);
                    case "Boolean":
                        return PropertyAccess.Boolean2LocalizedYesNo(Convert.ToBoolean(valueObject, CultureInfo.InvariantCulture), formatProvider);
                    case "DateTime":
                    case "Double":
                    case "Single":
                    case "Int32":
                    case "Int64":
                        return ((IFormattable)valueObject).ToString(outputFormat, formatProvider);
                    default:
                        return PropertyAccess.FormatString(valueObject.ToString(), format);
                }
            }
            else
            {
                propertyNotFound = true;
                return string.Empty;
            }
        }
    }
}
