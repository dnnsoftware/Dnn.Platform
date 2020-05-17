// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Data.Models
{
    [PrimaryKey(Constants.TABLENAME_Person_Key)]
    [TableName(Constants.TABLENAME_Person)]
    public class Person
    {
        public int? Age { get; set; }
        public int ID { get; set; }

        [ColumnName(Constants.COLUMNNAME_PersonName)]
        public string Name { get; set; }
    }
}
