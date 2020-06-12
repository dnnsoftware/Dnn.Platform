
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information


using System;

namespace DotNetNuke.ComponentModel.DataAnnotations
{
    public class ColumnNameAttribute : Attribute
    {
        public ColumnNameAttribute(string columnName)
        {
            this.ColumnName = columnName;
        }

        public string ColumnName { get; set; }
    }
}
