// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Abstractions.Prompt;
using Newtonsoft.Json;

namespace DotNetNuke.Prompt.Ouput
{
    public class PagingInfo : IPagingInfo
    {
        [JsonProperty(PropertyName = "pageNo")]
        public int PageNo { get; set; }

        [JsonProperty(PropertyName = "pageSize")]
        public int PageSize { get; set; }

        [JsonProperty(PropertyName = "totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty(PropertyName = "totalRecords")]
        public int TotalRecords { get; set; }
    }
}
