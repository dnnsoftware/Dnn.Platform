// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SqlConsole.Components
{
    using System;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

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
