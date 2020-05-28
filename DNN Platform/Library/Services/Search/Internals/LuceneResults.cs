// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
