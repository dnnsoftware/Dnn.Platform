// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Search.Internals;

using System.Collections.Generic;

internal class LuceneResults
{
    /// <summary>Initializes a new instance of the <see cref="LuceneResults"/> class.</summary>
    public LuceneResults()
    {
        this.Results = new List<LuceneResult>();
    }

    public IEnumerable<LuceneResult> Results { get; set; }

    public int TotalHits { get; set; }
}
