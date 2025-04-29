// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Search;

using DotNetNuke.Internal.SourceGenerators;

/// Namespace:  DotNetNuke.Services.Search
/// Project:    DotNetNuke.Search.DataStore
/// Class:      SearchCriteria
/// <summary>The SearchCriteria represents a search criterion.</summary>
[DnnDeprecated(7, 1, 0, "No longer used in the Search infrastructure", RemovalVersion = 10)]
public partial class SearchCriteria
{
    public string Criteria { get; set; }

    public bool MustExclude { get; set; }

    public bool MustInclude { get; set; }
}
