// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Search.Entities
{
    using System;

    [Serializable]
    public class SearchType
    {
        /// <summary>
        /// Gets or sets search Type Id.
        /// </summary>
        public int SearchTypeId { get; set; }

        /// <summary>
        /// Gets or sets search Type Name.
        /// </summary>
        public string SearchTypeName { get; set; }

        /// <summary>
        /// Gets or sets a class implementing BaseResultController. This class will be invoked by reflection.
        /// </summary>
        public string SearchResultClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether content from this SearchType will normally be not searched while performing site or module search.
        /// </summary>
        public bool IsPrivate { get; set; }
    }
}
