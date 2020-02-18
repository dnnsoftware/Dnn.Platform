// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SqlConsole.Components
{
    [TableName("SQLQueries")]
    [PrimaryKey("QueryId")]
    public partial class SqlQuery
    {
        [JsonProperty(PropertyName = "id")]
        public int QueryId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "query")]
        public string Query { get; set; }

        [JsonProperty(PropertyName = "connection")]
        public string ConnectionStringName { get; set; }

        [IgnoreDataMember]
        public int CreatedByUserId { get; set; }

        [IgnoreDataMember]
        public DateTime CreatedOnDate { get; set; }

        [IgnoreDataMember]
        public int LastModifiedByUserId { get; set; }

        [IgnoreDataMember]
        public DateTime LastModifiedOnDate { get; set; }
    }
}
