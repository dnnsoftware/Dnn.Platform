// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel.DataAnnotations;

using System;

public class ColumnNameAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="ColumnNameAttribute"/> class.</summary>
    /// <param name="columnName"></param>
    public ColumnNameAttribute(string columnName)
    {
        this.ColumnName = columnName;
    }

    public string ColumnName { get; set; }
}
