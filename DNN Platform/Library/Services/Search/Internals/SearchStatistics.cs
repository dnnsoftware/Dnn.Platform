// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class SearchStatistics
    {
        public int TotalActiveDocuments { get; set; }

        public int TotalDeletedDocuments { get; set; }

        public string IndexLocation { get; set; }

        public DateTime LastModifiedOn { get; set; }

        public long IndexDbSize { get; set; }
    }
}
