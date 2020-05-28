// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using Newtonsoft.Json;

namespace Dnn.PersonaBar.Library.Prompt.Models
{
    public class PagingInfo
    {
        [JsonProperty(PropertyName = "pageNo")]
        public int PageNo { get; set; }
        [JsonProperty(PropertyName = "pageSize")]
        public int PageSize { get; set; }
        [JsonProperty(PropertyName = "totalPages")]
        public int TotalPages { get; set; }
    }
}
