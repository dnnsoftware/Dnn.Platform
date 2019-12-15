// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Search.Entities;
using Lucene.Net.Search;

namespace DotNetNuke.Services.Search.Internals
{
    internal class SearchSecurityTrimmerContext
    {
        public IndexSearcher Searcher { get; set; }
        public SecurityCheckerDelegate SecurityChecker { get; set; }
        public LuceneQuery LuceneQuery { get; set; }
        public SearchQuery SearchQuery { get; set; }
    }
}
