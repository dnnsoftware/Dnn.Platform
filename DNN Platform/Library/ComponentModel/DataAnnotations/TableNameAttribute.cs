// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel.DataAnnotations
{
    using System;

    public class TableNameAttribute : Attribute
    {
        public TableNameAttribute(string tableName)
        {
            this.TableName = tableName;
        }

        public string TableName { get; set; }
    }
}
