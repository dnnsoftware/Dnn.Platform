// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens;

using System;
using System.Collections;
using System.Globalization;

using DotNetNuke.Entities.Users;

public class ArrayListPropertyAccess : IPropertyAccess
{
    private readonly ArrayList custom;

    /// <summary>Initializes a new instance of the <see cref="ArrayListPropertyAccess"/> class.</summary>
    /// <param name="list"></param>
    public ArrayListPropertyAccess(ArrayList list)
    {
        this.custom = list;
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
        if (this.custom == null)
        {
            return string.Empty;
        }

        object valueObject = null;
        string outputFormat = format;
        if (string.IsNullOrEmpty(format))
        {
            outputFormat = "g";
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
