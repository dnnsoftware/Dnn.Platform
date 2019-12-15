// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

using DotNetNuke.Services.Search.Entities;

namespace DotNetNuke.Services.Search.Internals
{
    [Serializable]
    public class SearchContentSource: SearchType
    {
        public string LocalizedName { get; set; }

        public int ModuleDefinitionId { get; set; }
    }
}
