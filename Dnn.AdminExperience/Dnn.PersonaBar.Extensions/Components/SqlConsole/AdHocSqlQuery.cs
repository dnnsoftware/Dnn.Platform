// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SqlConsole.Components
{
    [JsonObject]
    public class AdhocSqlQuery
    {
        [JsonProperty("connection")]
        public string ConnectionStringName { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("Timeout")]
        public int Timeout { get; set; }
    }
}
