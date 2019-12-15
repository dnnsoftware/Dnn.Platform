// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Search.Entities;

namespace DotNetNuke.Services.Search.Internals
{
    internal class LuceneSearchContext
    {
        public LuceneQuery LuceneQuery { get; set; }
        public SearchQuery SearchQuery { get; set; }
        public SecurityCheckerDelegate SecurityCheckerDelegate { get; set; }
    }
}
