// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel.DataAnnotations
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="PrimaryKeyAttribute"/> class.</summary>
        /// <param name="columnName">The name of the column (or a comma-delimited list of column names) of the primary key for the table.</param>
        public PrimaryKeyAttribute(string columnName)
            : this(columnName, columnName)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PrimaryKeyAttribute"/> class.</summary>
        /// <param name="columnName">The name of the column (or a comma-delimited list of column names) of the primary key for the table.</param>
        /// <param name="propertyName">The name of the property for the primary key.</param>
        public PrimaryKeyAttribute(string columnName, string propertyName)
        {
            this.ColumnName = columnName;
            this.PropertyName = propertyName;
            this.AutoIncrement = true;
        }

        public bool AutoIncrement { get; set; }

        public string ColumnName { get; set; }

        public string PropertyName { get; set; }
    }
}
