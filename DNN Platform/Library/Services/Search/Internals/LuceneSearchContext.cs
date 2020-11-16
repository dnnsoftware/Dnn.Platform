// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Search.Internals
{
    using DotNetNuke.Services.Search.Entities;

    internal class LuceneSearchContext
    {
        public LuceneQuery LuceneQuery { get; set; }

        public SearchQuery SearchQuery { get; set; }

        public SecurityCheckerDelegate SecurityCheckerDelegate { get; set; }
    }
}
