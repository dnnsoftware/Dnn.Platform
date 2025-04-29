// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Prompt.Models;

using System;

using DotNetNuke.Internal.SourceGenerators;

using Newtonsoft.Json;

[DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project")]
public partial class PagingInfo
{
    [JsonProperty(PropertyName = "pageNo")]
    public int PageNo { get; set; }

    [JsonProperty(PropertyName = "pageSize")]
    public int PageSize { get; set; }

    [JsonProperty(PropertyName = "totalPages")]
    public int TotalPages { get; set; }
}
