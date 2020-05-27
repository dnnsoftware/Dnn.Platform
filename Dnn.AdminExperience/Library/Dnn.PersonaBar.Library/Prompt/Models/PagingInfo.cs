// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Newtonsoft.Json;
using System;

namespace Dnn.PersonaBar.Library.Prompt.Models
{
    [Obsolete("Moved to DotNetNuke.Prompt in the core library project. Will be removed in DNN 11.", false)]
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
