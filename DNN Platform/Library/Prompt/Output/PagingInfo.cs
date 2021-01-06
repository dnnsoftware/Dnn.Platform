// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;

namespace DotNetNuke.Prompt
{
    public class PagingInfo : IPagingInfo
    {
        /// <inheritdoc/>
        [JsonProperty(PropertyName = "pageNo")]
        public int PageNo { get; set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "pageSize")]
        public int PageSize { get; set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "totalPages")]
        public int TotalPages { get; set; }

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "totalRecords")]
        public int TotalRecords { get; set; }
    }
}
