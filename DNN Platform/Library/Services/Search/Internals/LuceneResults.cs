// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Services.Search.Internals
{
    internal class LuceneResults
    {
        public IEnumerable<LuceneResult> Results { get; set; }
        public int TotalHits { get; set; }

        public LuceneResults()
        {
            Results = new List<LuceneResult>();
        }
    }
}
