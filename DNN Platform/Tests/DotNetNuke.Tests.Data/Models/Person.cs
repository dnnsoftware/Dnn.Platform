// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data.Models
{
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Tests.Utilities;

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
