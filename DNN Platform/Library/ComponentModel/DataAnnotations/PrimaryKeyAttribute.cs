﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel.DataAnnotations
{
    using System;

    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKeyAttribute"/> class.
        /// </summary>
        /// <param name="columnName"></param>
        public PrimaryKeyAttribute(string columnName)
            : this(columnName, columnName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKeyAttribute"/> class.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="propertyName"></param>
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
