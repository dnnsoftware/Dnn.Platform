// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;

#endregion

namespace DotNetNuke.ComponentModel.DataAnnotations
{
    public class PrimaryKeyAttribute : Attribute
    {
        public PrimaryKeyAttribute(string columnName) : this(columnName, columnName)
        {
        }

        public PrimaryKeyAttribute(string columnName, string propertyName)
        {
            ColumnName = columnName;
            PropertyName = propertyName;
            AutoIncrement = true;
        }

        public bool AutoIncrement { get; set; }
        public string ColumnName { get; set; }
        public string PropertyName { get; set; }

    }
}
